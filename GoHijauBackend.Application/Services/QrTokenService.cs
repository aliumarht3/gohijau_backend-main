using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Services
{
    public class QrTokenService
    {
        private readonly IQrTokenRepository _repository;
        private readonly IUserRepository _userRepository;

        public QrTokenService(IQrTokenRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<string> GenerateTokenAsync(string userId,string panel)
        {
            var qrToken = new QrToken
            {
                UserId = userId,
                Panel = panel
            };

            await _repository.CreateAsync(qrToken);
            return qrToken.Token;
        }


        public async Task<QrUserInfoDTO> VerifyTokenAsync(string token, string machineId)
        {
            var qrToken = await _repository.GetByTokenAsync(token);
            if (qrToken == null || qrToken.Status != "pending")
                return new QrUserInfoDTO { Panel =  "expired"};
            if (qrToken.ExpiresAt < DateTime.UtcNow)
                return new QrUserInfoDTO { Panel = "expired" };
            if (qrToken.UsedAt!=null)
                return new QrUserInfoDTO { Panel = "expired" };
            var user = await _userRepository.GetByIdAsync(qrToken.UserId);
            await _repository.MarkUsedAsync(token, machineId);
            var userRole = "";
            if (user != null)
            {
                var role = user.Roles.FirstOrDefault();
                userRole = ((UserRole)role).ToString(); ;
            }
            return new QrUserInfoDTO { Panel = String.IsNullOrEmpty(qrToken.Panel) ? "expired" : qrToken.Panel, Role = userRole };
        }
        public async Task<string> GetMachineId(string token) 
        {
            var qrToken = await _repository.GetByTokenAsync(token);
            if (qrToken == null || qrToken.Status == "pending")
            {
                return "expired"; 
            }
            return qrToken.MachineId; 

        }
        public async Task<QrUserInfoDTO> GetUserByQrToken(string token) 
        {
            var qrToken = await _repository.GetByTokenAsync(token);
            if (qrToken == null || qrToken.UserId == "") {
               return new QrUserInfoDTO { UserId = "usernotfound"};
            }
            var user = await _userRepository.GetByIdAsync(qrToken.UserId);
            var userRole = "";
            if (user != null)
            {
                var role = user.Roles.FirstOrDefault();
                 userRole = ((UserRole)role).ToString(); ;
            }
            return new QrUserInfoDTO { UserId = qrToken.UserId, Role = userRole };
        }
    }
}
