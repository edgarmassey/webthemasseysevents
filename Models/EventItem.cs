namespace WebTheMasseysEvents.Models
{
    public class EventItem
    {
        public string Slug { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public string? Cover { get; set; }
        public string BodyMarkdown { get; set; } = "";
        public int? Number { get; set; }
        public List<string> PhotoFiles { get; set; } = new();

    }
}   
