using System.Text.Json;
using System.Text;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using SpeedSms.Models;
using System.Net;
using System.Numerics;

namespace SpeedSms.Services
{
    public interface IAuthService
    {
        Task<bool> Register(Register req);
        Task<bool> VerifyCode(VerifyOtp req);
        string getUserInfo();
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
            if (req.Password.Equals(req.ConfirmPassword))
            {
                //var result = await SendBrandnameSmsAsync(req.UserName);
                var result = sendSMS(req.UserName);
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
        public String getUserInfo()
        {
            String url = _config["SmsSetting:RootUrl"] + "/user/info";
            NetworkCredential myCreds = new NetworkCredential(_config["SmsSetting:AccessToken"]!, ":x");
            WebClient client = new WebClient();
            client.Credentials = myCreds;
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);
            return reader.ReadToEnd();
        }
        public async Task<bool> SendBrandnameSmsAsync(string phone)
        {
            var code = GenerateOtpCode();
            string content = $"{code} la ma xac minh dang ky VietYClinic cua ban";
            string sender = "84335991255";
            // Kiểm tra đầu vào
            if (phone == null || phone.Length == 0 || string.IsNullOrEmpty(content) || string.IsNullOrEmpty(sender))
                return false;

            string accessToken = _config["SmsSetting:AccessToken"]!;
            string baseUrl = _config["SmsSetting:RootUrl"] + "/sms/send";
            int type = 4; 


            // Payload
            var payload = new
            {
                to = phone,
                content = content,
                type = type,
                sender = sender
            };

            string json = JsonSerializer.Serialize(payload);
            Console.WriteLine($"Request: {json}");

            using var httpClient = new HttpClient();
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{accessToken}:"));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var contentData = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(baseUrl, contentData);
            string result = await response.Content.ReadAsStringAsync();

            try
            {
                var jsonObj = JsonSerializer.Deserialize<JsonElement>(result);
                if (jsonObj.GetProperty("status").GetString() == "success" &&
                    jsonObj.GetProperty("code").GetString() == "00")
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Send Brandname SMS failed: " + result);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing response: {ex.Message}");
                return false;
            }
        }
        public bool sendSMS(string phone)
        {
            if (phone.Length <= 0)
                return false;

            string baseUrl = _config["SmsSetting:RootUrl"] + "/pin/create";
            string accessToken = _config["SmsSetting:AccessToken"]!;
            var code = GenerateOtpCode();
            string content = $"{code} la ma xac minh dang ky VietYClinic cua ban";
            string sender = _config["SmsSetting:AppId"];
            int type = 4;

            NetworkCredential myCreds = new NetworkCredential(accessToken, ":x");
            WebClient client = new WebClient();
            client.Credentials = myCreds;
            client.Headers[HttpRequestHeader.ContentType] = "application/json";

            var payload = new
            {
                to = phone,
                content = content,
                app_id = sender
            };

            try
            {
                String json = payload.ToString();
                string response = client.UploadString(baseUrl, json);
                Console.WriteLine("Response: " + response);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                string status = root.GetProperty("status").GetString();
                string codeResult = root.GetProperty("code").GetString();

                return status == "success" && codeResult == "00";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send SMS failed: " + ex.Message);
                return false;
            }
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
