using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.DTO.MedicineDTOs
{
    public class MedicineResponseDTO
    {
        public Guid id { get; set; }
        public string tradeName { get; set; }
        public string genericName { get; set; }
        public decimal price { get; set; }
        public string imageURL { get; set; }
        public bool isRestricted { get; set; }
        public string manufacturer { get; set; }
        public string? dosageForm { get; set; }
        public string? strength { get; set; }
        public string? route { get; set; }
        public string? registrationNo { get; set; }
    }
}
