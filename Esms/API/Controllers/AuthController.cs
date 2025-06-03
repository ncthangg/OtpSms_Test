using API.Models;
using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

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

        //[HttpPost("callback")]
        //    public IActionResult EsmsCallback([FromBody] EsmsCallback callback)
        //    {
        //        var allowedIps = new List<string>
        //{
        //    "123.30.145.12",  // ví dụ IP của ESMS
        //    "192.168.1.63"  // thêm các IP khác nếu cần
        //};

        //        var ip = GetRequestIp(HttpContext);

        //        if (!allowedIps.Contains(ip))
        //        {
        //            return Unauthorized("Unauthorized IP");
        //        }

        //        Console.WriteLine($"Phone: {callback.Phone}, Status: {callback.Status}, MessageID: {callback.MessageID}, Content: {callback.Content}");

        //        return Ok();
        //    }
        //[HttpPost("callback")]
        //public async Task<IActionResult> Callback()
        //{
        //    using var reader = new StreamReader(Request.Body);
        //    var body = await reader.ReadToEndAsync();
        //    Console.WriteLine("Callback Received: " + body);
        //    return Ok();
        //}

        //private string GetRequestIp(HttpContext context)
        //{
        //    return context.Connection.RemoteIpAddress?.ToString();
        //}
    }
}
