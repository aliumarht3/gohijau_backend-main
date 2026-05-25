using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/secret")]
    public class ManageRazorPayoutSecretKey : ControllerBase
    {
        private readonly ISecretService _manageRazorPayoutSecret;
        public ManageRazorPayoutSecretKey(ISecretService manageRazorPayoutSecretKey)
        {
            _manageRazorPayoutSecret = manageRazorPayoutSecretKey;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSecret([FromBody] SecretKeysDto secret)
        {
            try
            {
                var result = await _manageRazorPayoutSecret.CreateSecretKey(secret.KeySecret, secret.KeyId);
                return Ok(result.IsSuccess);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("get-secret")]
        public async Task<ActionResult<SecretKeysDto>?> GetSecret()
        {
            try
            {
                var result = await _manageRazorPayoutSecret.GetSecret();
                if (result.Value != null)
                {
                    return Ok(result.Value);
                }
                else {
                    return BadRequest("Secret not found"); 
                }
            } 
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }  
        }
    }
}
