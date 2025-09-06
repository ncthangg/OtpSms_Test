namespace SpeedSms.Models
{
    public class VerifyOtp
    {
        public required string Phone { get; set; }
        public required string Code { get; set; }
    }
}
