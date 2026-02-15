namespace WebTheMasseysEvents.Services
{
    public class CurrentItem
    {
        public string Slug { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.MinValue;
        public string Summary { get; set; } = "";
        public string Cover { get; set; } = "";   // optional (e.g. "/Photos/xyz.jpg")
        public string Markdown { get; set; } = "";
        public string Html { get; set; } = "";
        public string SourcePath { get; set; } = "";
        public string PhotoFolderWeb { get; set; } = "";      // e.g. "/Photos/Current/Niklas40th"
        public List<string> GalleryImagesWeb { get; set; } = new();

    }
}
