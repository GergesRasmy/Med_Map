namespace Med_Map.DTO.ResponseDTOs
{
    public class PagedDTO<T>
    {
        public List<T> items { get; set; }
        public int totalCount { get; set; }
        public int currentPage { get; set; }
        public int pageSize { get; set; }
        public int totalPages { get; set; }
    }
}
