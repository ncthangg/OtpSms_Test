using API.Models;
using System.Text.Json;
using System.Text;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace API.Services
{
    public interface IAuthService
    {
        Task<bool> Register(Register req);
        Task<bool> VerifyCode(VerifyOtp req);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IDatabase _redis;

        public AuthService(HttpClient httpClient, IConfiguration config, IDatabase redis)
        {
            _httpClient = httpClient;
            _config = config;
            _redis = redis;
        }
        
        public async Task<bool> Register(Register req)
        {
            if(req.Password.Equals(req.ConfirmPassword))
            {
                var result = await this.SendOtpAsync(req.UserName);
                if (result)
                {
                    return true;
                }
                return false;
            }
            return false; 
        }

        public async Task<bool> VerifyCode(VerifyOtp req)
        {
            var result = await VerifyOtpAsync(req.Phone, req.Code);

            if (result)
            {
                await _redis.KeyDeleteAsync(req.Phone);
                return true;
            }

            return false;
        }

        #region
        private async Task<bool> SendOtpAsync(string phoneNumber)
        {
            var code = GenerateOtpCode();
            var payload = new
            {
                ApiKey = _config["SmsSetting:ApiKey"],
                SecretKey = _config["SmsSetting:SecretKey"],
                Content = $"{code} la ma xac minh dang ky Baotrixemay cua ban",
                Phone = phoneNumber,
                Brandname = "Baotrixemay",
                SmsType = "2",
                IsUnicode = "0",
                campaignid = "Cảm ơn sau mua hàng tháng 7",
                RequestId = Guid.NewGuid().ToString(),
                CallbackUrl = _config["SmsSetting:CallbackUrl"],
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_post_json/", content);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            await SaveOtpAsync(phoneNumber, code);
            return response.IsSuccessStatusCode;
        }
        private async Task SaveOtpAsync(string phoneNumber, string code)
        {
            await _redis.StringSetAsync(phoneNumber, code, TimeSpan.FromMinutes(5));
        }
        private async Task<bool> VerifyOtpAsync(string phoneNumber, string inputCode)
        {
            var storedCode = await _redis.StringGetAsync(phoneNumber);

            return storedCode == inputCode;
        }
        #endregion

        private static string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

    }
}
