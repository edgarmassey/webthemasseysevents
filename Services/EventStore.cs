using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using WebTheMasseysEvents.Models;

namespace WebTheMasseysEvents.Services
{
    public class EventStore
    {
        private readonly IWebHostEnvironment _env;

        public EventStore(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IReadOnlyList<EventItem> GetAll()
        {
            // Content/Events/*.md
            var dir = Path.Combine(_env.ContentRootPath, "Content", "Events");
            if (!Directory.Exists(dir)) return Array.Empty<EventItem>();

            var files = Directory.GetFiles(dir, "*.md");
            var items = new List<EventItem>();

            foreach (var file in files)
            {
                var slug = Path.GetFileNameWithoutExtension(file);
                var text = File.ReadAllText(file);

                var (front, body) = SplitFrontMatter(text);
                var map = ParseFrontMatter(front);

                map.TryGetValue("title", out var title);
                map.TryGetValue("date", out var dateStr);
                map.TryGetValue("location", out var location);
                map.TryGetValue("cover", out var cover);
                map.TryGetValue("number", out var numberStr);

                DateTime.TryParse(dateStr, out var date);
                int.TryParse(numberStr, out var number);

                var coverFile = string.IsNullOrWhiteSpace(cover) ? null : cover.Trim();

                items.Add(new EventItem
                {
                    Slug = slug,
                    Title = string.IsNullOrWhiteSpace(title) ? slug : title.Trim(),
                    Date = date == default ? File.GetCreationTime(file) : date,
                    Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
                    Cover = coverFile,
                    BodyMarkdown = body.Trim(),

                    // FIX: store number (null if missing/0)
                    Number = number == 0 ? null : number,

                    // FIX: load photos from folder, excluding cover so it won't appear twice
                    PhotoFiles = GetPhotoFilesForSlug(slug, coverFile),
                });
            }

            return items
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        public EventItem? GetBySlug(string slug) =>
            GetAll().FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

        private static (string frontMatter, string body) SplitFrontMatter(string text)
        {
            // expects:
            // ---
            // key: value
            // ---
            // body...
            var m = Regex.Match(text, @"\A---\s*(.*?)\s*---\s*(.*)\z", RegexOptions.Singleline);
            if (!m.Success) return ("", text);
            return (m.Groups[1].Value, m.Groups[2].Value);
        }

        private static Dictionary<string, string> ParseFrontMatter(string fm)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in fm.Split('\n'))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                var idx = trimmed.IndexOf(':');
                if (idx < 0) continue;

                var key = trimmed[..idx].Trim();
                var val = trimmed[(idx + 1)..].Trim();
                dict[key] = val;
            }

            return dict;
        }

        private List<string> GetPhotoFilesForSlug(string slug, string? coverFile)
        {
            // wwwroot/Photos/Events/{slug}/
            var photosDir = Path.Combine(_env.WebRootPath, "Photos", "Events", slug);
            if (!Directory.Exists(photosDir)) return new List<string>();

            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

            return Directory.GetFiles(photosDir)
                .Where(f => allowed.Contains(Path.GetExtension(f)))
                .Select(Path.GetFileName)
                .Where(name =>
                    !string.IsNullOrWhiteSpace(name) &&
                    (string.IsNullOrWhiteSpace(coverFile) ||
                     !string.Equals(name, coverFile, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(name => name) // e.g. 01.jpg, 02.jpg...
                .ToList()!;
        }
    }
}

