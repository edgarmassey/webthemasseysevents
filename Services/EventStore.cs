using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using WebTheMasseysEvents.Models;
using Markdig;

namespace WebTheMasseysEvents.Services
{
    public class EventStore
    {
        private readonly IWebHostEnvironment _env;

        private static readonly MarkdownPipeline Pipeline =
            new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

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

                // NEW
                map.TryGetValue("highlight", out var highlightStr);
                map.TryGetValue("link", out var link);
                map.TryGetValue("linkText", out var linkText);
                map.TryGetValue("tags", out var tagsStr);

                DateTime.TryParse(dateStr, out var date);
                int.TryParse(numberStr, out var number);

                var coverFile = string.IsNullOrWhiteSpace(cover) ? null : cover.Trim();

                // NEW
                var highlight = ParseBool(highlightStr);
                var tags = ParseTags(tagsStr);
                var bodyMd = body.Trim();
                var bodyHtml = Markdown.ToHtml(bodyMd, Pipeline);

                items.Add(new EventItem
                {
                    Slug = slug,
                    Title = string.IsNullOrWhiteSpace(title) ? slug : title.Trim(),
                    Date = date == default ? File.GetCreationTime(file) : date,
                    Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
                    Cover = coverFile,

                    BodyMarkdown = bodyMd,
                    BodyHtml = bodyHtml,   // ✅ THIS WAS MISSING

                    Number = number == 0 ? null : number,
                    PhotoFiles = GetPhotoFilesForSlug(slug, coverFile),

                    Highlight = highlight,
                    Tags = tags,
                    Link = string.IsNullOrWhiteSpace(link) ? null : link.Trim(),
                    LinkText = string.IsNullOrWhiteSpace(linkText) ? null : linkText.Trim(),
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

            // Supports:
            // key: value
            // tags:
            //   - a
            //   - b
            //
            // For multi-line lists, we store as "a,b" in dict["tags"].

            string? currentKey = null;
            var listItems = new List<string>();

            foreach (var rawLine in fm.Split('\n'))
            {
                var line = rawLine.TrimEnd('\r');
                var trimmed = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                // Multi-line list item: "- something"
                if (currentKey != null && trimmed.StartsWith("-", StringComparison.Ordinal))
                {
                    var item = trimmed[1..].Trim();
                    if (!string.IsNullOrWhiteSpace(item))
                        listItems.Add(item);
                    continue;
                }

                // New key
                var idx = trimmed.IndexOf(':');
                if (idx < 0) continue;

                // If we were collecting a list, flush it
                if (currentKey != null)
                {
                    dict[currentKey] = string.Join(",", listItems);
                    currentKey = null;
                    listItems.Clear();
                }

                var key = trimmed[..idx].Trim();
                var val = trimmed[(idx + 1)..].Trim();

                // If "tags:" with no value, begin list capture
                if (string.Equals(key, "tags", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(val))
                {
                    currentKey = key;
                    continue;
                }

                dict[key] = val;
            }

            // Flush any pending list capture
            if (currentKey != null)
            {
                dict[currentKey] = string.Join(",", listItems);
            }

            return dict;
        }
      
        private static bool ParseBool(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();

            if (bool.TryParse(s, out var b)) return b;

            // allow 1/0, yes/no
            if (s == "1") return true;
            if (s == "0") return false;
            if (s.Equals("yes", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.Equals("no", StringComparison.OrdinalIgnoreCase)) return false;

            return false;
        }

        private static List<string> ParseTags(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return new List<string>();

            s = s.Trim();

            // Handle [a, b, c]
            if (s.StartsWith("[") && s.EndsWith("]"))
            {
                s = s[1..^1];
            }

            // Split by comma
            var parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Trim().Trim('"').Trim('\''))
                         .Where(x => x.Length > 0)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .ToList();

            return parts;
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
                .OrderBy(name => name)
                .ToList()!;
        
        }

    }

}
