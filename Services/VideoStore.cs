using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;

namespace WebTheMasseysEvents.Services
{
    public class VideoStore
    {
        private readonly IWebHostEnvironment _env;

        public VideoStore(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IReadOnlyList<VideoItem> GetAll()
        {
            // Content/Videos/*.md
            var dir = Path.Combine(_env.ContentRootPath, "Content", "Videos");
            if (!Directory.Exists(dir)) return Array.Empty<VideoItem>();

            var files = Directory.GetFiles(dir, "*.md");
            var items = new List<VideoItem>();

            foreach (var file in files)
            {
                var slug = Path.GetFileNameWithoutExtension(file);
                var text = File.ReadAllText(file);

                var (front, body) = SplitFrontMatter(text);
                var map = ParseFrontMatter(front);

                map.TryGetValue("title", out var title);
                map.TryGetValue("date", out var dateStr);
                map.TryGetValue("location", out var location);
                map.TryGetValue("video", out var video);
                map.TryGetValue("videoFolder", out var videoFolder);
                map.TryGetValue("description", out var description);

                DateTime.TryParse(dateStr, out var date);

                // Normalize folder: trim and remove trailing slash
                videoFolder = (videoFolder ?? "").Trim().TrimEnd('/');

                items.Add(new VideoItem
                {
                    Slug = slug,
                    Title = string.IsNullOrWhiteSpace(title) ? slug : title.Trim(),
                    Date = date == default ? File.GetCreationTime(file) : date,
                    Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
                    VideoFile = string.IsNullOrWhiteSpace(video) ? null : video.Trim(),
                    VideoFolder = string.IsNullOrWhiteSpace(videoFolder) ? null : videoFolder,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                    BodyMarkdown = body.Trim()
                });
            }

            return items.OrderByDescending(v => v.Date).ToList();
        }

        public VideoItem? GetBySlug(string slug) =>
            GetAll().FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

        private static (string frontMatter, string body) SplitFrontMatter(string text)
        {
            var m = Regex.Match(text, @"\A---\s*(.*?)\s*---\s*(.*)\z", RegexOptions.Singleline);
            if (!m.Success) return ("", text);
            return (m.Groups[1].Value, m.Groups[2].Value);
        }

        private static Dictionary<string, string> ParseFrontMatter(string fm)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rawLine in fm.Split('\n'))
            {
                var trimmed = rawLine.TrimEnd('\r').Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                var idx = trimmed.IndexOf(':');
                if (idx < 0) continue;

                var key = trimmed[..idx].Trim();
                var val = trimmed[(idx + 1)..].Trim();
                dict[key] = val;
            }

            return dict;
        }
    }

    public class VideoItem
    {
        public string Slug { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime Date { get; set; }
        public string? Location { get; set; }

        public string? VideoFile { get; set; }      // e.g. samslaughter.mp4
        public string? VideoFolder { get; set; }    // e.g. Videos/Family (relative to wwwroot)
        public string? Description { get; set; }
        public string BodyMarkdown { get; set; } = "";

        public string? VideoUrl =>
            (!string.IsNullOrWhiteSpace(VideoFolder) && !string.IsNullOrWhiteSpace(VideoFile))
                ? "/" + VideoFolder.Trim('/').Replace("\\", "/") + "/" + VideoFile
                : null;
    }
}
