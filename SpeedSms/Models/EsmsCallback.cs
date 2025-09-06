namespace SpeedSms.Models
{
    public class EsmsCallback
    {
        public required string Phone { get; set; }
        public required string Status { get; set; }
        public required string MessageID { get; set; }
        public required string Content { get; set; }
    }
}
