using System.Text.Json.Serialization;

namespace Med_Map.DTO
{
    public class ErrorResponseDTO<T>
    {
        public bool success { get; set; }
        public string code { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public object? error { get; set; }
    }
}
