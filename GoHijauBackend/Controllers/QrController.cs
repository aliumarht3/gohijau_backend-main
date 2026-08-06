using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Requests;
using GoHijauBackend.Application.Services;
using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrController : ControllerBase
    {
        private readonly QrTokenService _service;
        private readonly TransactionService _transactionService;
        private readonly IHubContext<QRHub> _hubContext;
        private readonly IMachineUCOTrackingService _machineUCOTrackingService;
        private readonly ICollectionPostProcessQueue _collectionPostProcessQueue;
        public QrController(
            QrTokenService service,
            TransactionService transactionService,
            IHubContext<QRHub> hubContext,
            IMachineUCOTrackingService machineUCOTrackingService,
            ICollectionPostProcessQueue collectionPostProcessQueue)
        {
            _service = service;
            _hubContext = hubContext;
            _transactionService = transactionService;
            _machineUCOTrackingService = machineUCOTrackingService;
            _collectionPostProcessQueue = collectionPostProcessQueue;
        }


        [HttpPost("verify")]
        public async Task<IActionResult> VerifyToken([FromBody] TokenVerifyRequest request)
        {
            var result = await _service.VerifyTokenAsync(request.Token, request.MachineId);
            string isValid = result.Panel;
            string role = result.Role;
            if (!String.IsNullOrEmpty(isValid))
            {
                if (isValid != "expired")
                {
                    if (role == UserRole.Customer.ToString())
                    {
                        await _hubContext.Clients.Group(request.Token).SendAsync("TokenVerified", new
                        {
                            message = "Authorized! You can start pouring.",
                            token = request.Token
                        });
                    }
                    if (role == UserRole.OilCollector.ToString())
                    {
                        await _hubContext.Clients.Group(request.Token).SendAsync("TokenVerifiedCollector", new
                        {
                            message = "Authorized! You can start collecting.",
                            token = request.Token
                        });
                    }
                }
                else {
                    await _hubContext.Clients.Group(request.Token).SendAsync("TokenExpired", new
                    {
                        message = "Token expired.",
                        token = request.Token
                    });
                }    
                }
            return Ok(new { success = isValid });
        }
        [HttpPost("getMachineId")]
        public async Task<IActionResult> GetMachineIdFromQR([FromBody] Token token)
        {
            var result = await _service.GetMachineId(token.token) ;
            if (!String.IsNullOrEmpty(result))
            {
                return Ok(new { success = result });
            }
            return Ok();
        }
        [HttpPost("overflow")]
        public async Task<IActionResult> OverflowDetected([FromBody] TokenVerifyRequest request)
        {
            await _hubContext.Clients.Group(request.Token).SendAsync("Overflow", new
            {
                message = "Overflow detected! Please end pouring.",
                token = request.Token
            });

            return Ok(new { success = true, message = "Process overflow signal sent" });

        }
        [HttpPost("complete/pouring")]
        public async Task<IActionResult> CompletePouringProcess([FromBody] CompleteProcessRequest request)
        {
            var result = await _service.GetUserByQrToken(request.Token);
            if (result == null) return Unauthorized();

            string userId = result.UserId;
            string role = result.Role;
                await _hubContext.Clients.Group(request.Token).SendAsync("Finalizing", new
                {
                    message = "Finalizing! Please wait.",
                    token = request.Token
                });

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { success = false, message = "Invalid token" });
            }
            // 2️⃣ Create transaction object
            var transactionRequest = new RecordTransactionRequest
            {
                UserId = userId,
                OilPoured = request.OilAmount,
                CO2Saved = CalculateCO2Saved(request.OilAmount),
                PointsAwarded = CalculateRewardPoint(request.OilAmount),
                MachineId = request.MachineId,
                AccessToken = request.Token
            };
            var getMachineUcoTracking = await _machineUCOTrackingService.GetMachineUCOTrackingByMachineId(request.MachineId);
            if (getMachineUcoTracking != null)
            {
                getMachineUcoTracking.BufferVolume += transactionRequest.OilPoured;
                getMachineUcoTracking.ModifiedAt = DateTime.UtcNow;
                var resetMachineUcoTracking = await _machineUCOTrackingService.UpdateMachineUCOTracking(getMachineUcoTracking);
            }
            else {
                await _machineUCOTrackingService.CreateNewUCOMachineTracking(request.MachineId,transactionRequest.OilPoured);
            }

            // 3️⃣ Record transaction (insert + update totals)
            await _transactionService.RecordTransactionAsync(transactionRequest);
            // Notify all clients in the token group
            await _hubContext.Clients.Group(request.Token).SendAsync("PouringComplete", new
            {
                token = request.Token,
                oilAmount = request.OilAmount,
                points = transactionRequest.PointsAwarded
            });

            return Ok(new { success = true, message = "Process complete signal sent" });
        }

        [HttpPost("complete/collection")]
        public async Task<IActionResult> CompleteCollectionProcess([FromBody] CompleteCollectorProcessRequest request)
        {
            var result = await _service.GetUserByQrToken(request.Token);
            if (result == null) return Unauthorized();

            string userId = result.UserId;
            string role = result.Role;
            await _hubContext.Clients.Group(request.Token).SendAsync("Finalizing", new
            {
                message = "Finalizing! Please wait.",
                token = request.Token
            });

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { success = false, message = "Invalid token" });
            }
            double oilAmount = 0;
            var getMachineUcoTracking = await _machineUCOTrackingService.GetMachineUCOTrackingByMachineId(request.MachineId);
            if (getMachineUcoTracking != null) 
            {   
                oilAmount = getMachineUcoTracking.BufferVolume;
                getMachineUcoTracking.BufferVolume = 0; 
                getMachineUcoTracking.ModifiedAt = DateTime.UtcNow;
                var resetMachineUcoTracking = await _machineUCOTrackingService.UpdateMachineUCOTracking(getMachineUcoTracking); 
            }

            var transactionRequest = new RecordCollectorTransactionRequest
            {
                UserId = userId,
                OilCollected = oilAmount,
                CO2Saved = CalculateCO2Saved(oilAmount),
                MachineId = request.MachineId,
                AccessToken = request.Token
            };
            // 3️⃣ Record transaction (insert + update totals)
            await _transactionService.UpdateCollectorTransactionAsync(transactionRequest);

            // Run invoice/pdf/email in background to keep endpoint latency stable.
            await _collectionPostProcessQueue.EnqueueAsync(new Application.Dto.CollectionPostProcessJob
            {
                UserId = userId,
                MachineId = request.MachineId,
                AccessToken = request.Token,
                OilCollected = oilAmount
            });

            // Notify all clients in the token group
            await _hubContext.Clients.Group(request.Token).SendAsync("CollectionComplete", new
            {
                token = request.Token,
                oilAmount = oilAmount,
            });

            return Ok(new { success = true, message = "Process complete signal sent" });
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetTokenStatus([FromQuery] string token)
        {
            var tx = await _transactionService.GetTransactionByTokenAsync(token);
            if (tx != null)
                return Ok(new { phase = "PouringComplete", oilAmount = tx.OilPoured, points = tx.PointsAwarded });

            var ctx = await _transactionService.GetCollectorTransactionByTokenAsync(token);
            if (ctx != null)
                return Ok(new { phase = "CollectionComplete", oilAmount = ctx.OilCollected });

            return Ok(new { phase = "InProgress" });
        }

        [Authorize]
        [HttpPost("generate/customer")]
        public async Task<IActionResult> GenerateTokenCustomer()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var token = await _service.GenerateTokenAsync(userId, "CUSTOMERPANEL");
            return Ok(new { success = true, token });
        }

        [Authorize]
        [HttpPost("generate/collector")]
        public async Task<IActionResult> GenerateTokenCollector()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var token = await _service.GenerateTokenAsync(userId, "COLLECTORPANEL");
            return Ok(new { success = true, token });
        }

        [Authorize]
        [HttpPost("generate/technician")]
        public async Task<IActionResult> GenerateTokenTechnician()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var token = await _service.GenerateTokenAsync(userId,"TECHNICIANPANEL");
            return Ok(new { success = true, token });
        }
        private double CalculateCO2Saved(double oilAmount)
        {
            const double co2PerLitre = 2.5; // Example conversion factor
            return oilAmount * co2PerLitre;
        }
        private double CalculateRewardPoint(double oilAmount) {
            const double rate = 1.2; 
            return oilAmount * rate;
        }
        public class CompleteProcessRequest
        {
            public required string Token { get; set; }
            public double OilAmount { get; set; }
            public required string MachineId { get; set; }
         }
        public class CompleteCollectorProcessRequest
        {
            public required string Token { get; set; }
            public required string MachineId { get; set; }
        }
        public class TokenRequest
        {
            public required string UserId { get; set; }
        }
        public class Token
        {
            public required string token { get; set; }
        }
        public class TokenVerifyRequest
        {
            public required string Token { get; set; }
            public required string MachineId { get; set; }
        }
    }
}
