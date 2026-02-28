namespace Med_Map.DTO.AccountDTOs
{
    public class AuthResponseDataDTO
    {
        public string token { get; set; }
        public DateTime expiration { get; set; }
        public string role { get; set; }
        public Guid? sessionId { get; set; }
    }
}
