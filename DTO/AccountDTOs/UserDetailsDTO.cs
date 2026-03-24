namespace Med_Map.DTO.AccountDTOs
{
    public class UserDetailsDTO
    {
        public string id { get; set; }
        public string role { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public bool isRegistered { get; set; } = false;
        public string displayName { get; set; }
    }
}
