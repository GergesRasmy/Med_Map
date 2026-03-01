using System.Text.Json.Serialization;

namespace Med_Map.DTO.ResponseDTOs
{
    public class SuccessResponseDTO<T>: ResponseDTO
    {
        public T? data { get; set; }
    }
}
