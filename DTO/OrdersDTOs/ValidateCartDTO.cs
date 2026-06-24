namespace Med_Map.DTO.OrdersDTOs
{
    /// <summary>
    /// Request body for POST /api/order/validate-cart.
    /// Mirrors the client-side cart: one entry per pharmacy (the cart may span several pharmacies).
    /// Shapes match the Flutter <c>CartPharmacyOrder</c> / <c>CartItem</c> models so the client can
    /// post its local cart verbatim. The server only relies on <c>pharmacyId</c> and each item's
    /// <c>type</c> + id + <c>quantity</c>; the remaining fields are accepted and (for prices/names) echoed back.
    /// </summary>
    public class CartPharmacyValidationDTO
    {
        [Required]
        public string pharmacyId { get; set; }
        public string? pharmacyName { get; set; }
        public double? pharmacyLatitude { get; set; }
        public double? pharmacyLongitude { get; set; }
        public bool deliveryAvailability { get; set; }

        public string? paymentOption { get; set; }
        public string? fulfillmentType { get; set; }
        public double? deliveryLatitude { get; set; }
        public double? deliveryLongitude { get; set; }
        public string? deliveryAddressDescription { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Each pharmacy cart must contain at least one item")]
        public List<CartItemValidationDTO> items { get; set; }
    }

    public class CartItemValidationDTO
    {
        [Required]
        public string type { get; set; } = "medicine"; // "medicine" | "service"

        public Guid? medicineId { get; set; }
        public Guid? serviceId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int quantity { get; set; }

        // stale values the client currently holds — used for change detection, not trusted
        public string? tradeName { get; set; }
        public string? genericName { get; set; }
        public string? serviceName { get; set; }
        public decimal? unitPrice { get; set; }
        public string? priceUnitIsoCode { get; set; }
    }
}
