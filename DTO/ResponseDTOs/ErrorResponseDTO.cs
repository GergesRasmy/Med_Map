using System.Text.Json.Serialization;

namespace Med_Map.DTO.ResponseDTOs
{
    public class ErrorResponseDTO<T> : ResponseDTO
    {
        public object? error { get; set; }
    }
}
