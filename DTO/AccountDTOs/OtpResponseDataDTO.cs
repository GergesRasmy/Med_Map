namespace Med_Map.DTO.AccountDTOs
{
    public class OtpResponseDataDTO
    {
        public DateTime expiration { get; set; }
        public Guid? sessionId { get; set; }
    }
}
