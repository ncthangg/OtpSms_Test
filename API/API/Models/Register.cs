namespace API.Models
{
    public sealed class Register
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }

    }
}
