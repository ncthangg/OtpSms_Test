using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpeedSms.Models;
using SpeedSms.Services;
using StackExchange.Redis;

namespace SpeedSms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpGet("getUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var success =  authService.getUserInfo();
            if (success != null)
                return Ok(new { message = success });

            return StatusCode(500, new { message = success });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(Register request)
        {
            var success = await authService.Register(request);
            if (success)
                return Ok(new { message = "Mã OTP đã được gửi thành công." });

            return StatusCode(500, new { message = "Mã OTP đã được gửi thất bại." });
        }
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtp request)
        {
            var success = await authService.VerifyCode(request);
            if (success)
                return Ok(new { message = "Xác thực thành công" });

            return StatusCode(500, new { message = "Xác thực thất bại." });
        }

    }
}
