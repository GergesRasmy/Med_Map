namespace Med_Map.DTO.OrdersDTOs
{
    public class OrderStatsDTO
    {
        public int newOrders { get; set; }       // StatusList.Recorded
        public int preparing { get; set; }       // StatusList.Packaged
        public int outForDelivery { get; set; }  // StatusList.OutForDelivery
        public int completedToday { get; set; }  // StatusList.Delivered, DeliveredAt = today
    }
}
