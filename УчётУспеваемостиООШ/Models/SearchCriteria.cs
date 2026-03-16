namespace УчётУспеваемостиООШ.Models
{
    public class SearchCriteria
    {
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? ClassName { get; set; }
        public string? SubjectName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool IsActive { get; set; }
    }
}