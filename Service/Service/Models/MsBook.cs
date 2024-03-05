namespace Service.Models
{
    public class MsBook
    {
        public string? Id { get; set; }

        public string BookName { get; set; } = null!;

        public decimal? Price { get; set; }

        public string Category { get; set; } = null!;

        public string Author { get; set; } = null!;
    }
}
