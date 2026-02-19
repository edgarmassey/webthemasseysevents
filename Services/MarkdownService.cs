using Markdig;

namespace WebTheMasseysEvents.Services
{
    public static class MarkdownService
    {
        public static readonly MarkdownPipeline Pipeline =
            new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

        public static string ToHtml(string markdown)
            => Markdown.ToHtml(markdown ?? "", Pipeline);
    }
}
