namespace Med_Map.DTO.OrdersDTOs
{
    /// <summary>
    /// Result of validating a stale local cart against current server state
    /// (POST /api/order/validate-cart). Lets the client reconcile prices/stock and
    /// show the applicable fees before calling /place.
    /// </summary>
    public class CartValidationResponseDTO
    {
        public List<PharmacyCartValidationResultDTO> pharmacies { get; set; } = new();
        public CartFeesDTO fees { get; set; } = new();

        /// <summary>True when every pharmacy in the cart is valid (exists, active, all items in stock).</summary>
        public bool isValid => pharmacies.All(p => p.isValid);
    }

    public class PharmacyCartValidationResultDTO
    {
        public string pharmacyId { get; set; }
        /// <summary>Authoritative pharmacy name from the server (may differ from the stale client value).</summary>
        public string? pharmacyName { get; set; }
        /// <summary>False if the pharmacy no longer exists or has no active profile.</summary>
        public bool found { get; set; }
        public bool deliveryAvailable { get; set; }
        public decimal deliveryFee { get; set; }

        public List<CartItemValidationResultDTO> items { get; set; } = new();

        /// <summary>Sum of (currentUnitPrice × requestedQuantity) over available items. Excludes fees.</summary>
        public decimal subtotal { get; set; }

        /// <summary>True when the pharmacy was found and every item is available at the requested quantity.</summary>
        public bool isValid { get; set; }
    }

    public class CartItemValidationResultDTO
    {
        public Guid medicineId { get; set; }
        public string? tradeName { get; set; }
        public string? genericName { get; set; }

        /// <summary>Current price from the pharmacy inventory (null if the medicine is no longer stocked).</summary>
        public decimal? currentUnitPrice { get; set; }
        /// <summary>Price the client had cached; echoed back for diffing.</summary>
        public decimal? previousUnitPrice { get; set; }
        public bool priceChanged { get; set; }
        public string? priceUnitIsoCode { get; set; }

        public int requestedQuantity { get; set; }
        /// <summary>Current stock at the pharmacy (0 if not stocked).</summary>
        public int availableQuantity { get; set; }
        /// <summary>True when the medicine is stocked with enough quantity to fulfil the request.</summary>
        public bool isAvailable { get; set; }

        /// <summary>currentUnitPrice × requestedQuantity, or 0 when unavailable.</summary>
        public decimal lineTotal { get; set; }

        /// <summary>Human-readable note when something is off (out of stock, removed, price changed, …).</summary>
        public string? message { get; set; }
    }

    public class CartFeesDTO
    {
        public decimal appFee { get; set; }
        public decimal cashOnDeliveryFee { get; set; }
        public decimal onlineFee { get; set; }
    }
}
