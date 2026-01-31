using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using WebTheMasseysEvents.Models;

namespace WebTheMasseysEvents.Services
{
    public class OurKidsStore
    {
        private readonly IWebHostEnvironment _env;

        public OurKidsStore(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IReadOnlyList<OurKidItem> GetAll()
        {
            var dir = Path.Combine(_env.ContentRootPath, "Content", "OurKids");
            if (!Directory.Exists(dir)) return Array.Empty<OurKidItem>();

            var files = Directory.GetFiles(dir, "*.md");
            var items = new List<OurKidItem>();

            foreach (var file in files)
            {
                var slug = Path.GetFileNameWithoutExtension(file);
                var text = File.ReadAllText(file);

                var (front, body) = SplitFrontMatter(text);
                var fm = ParseFrontMatter(front);

                items.Add(new OurKidItem
                {
                    Slug = slug,
                    Name = Get(fm, "name") ?? slug,
                    Partner = Get(fm, "partner"),
                    Location = Get(fm, "location"),
                    Order = GetInt(fm, "order", 100),
                    Cover = Get(fm, "cover"),
                    HomePhotos = GetList(fm, "home_photos"),
                    KidsLink = Get(fm, "kids_link"),
                    Blurb = Get(fm, "blurb"),
                    BodyMarkdown = body.Trim(),

                    // NEW: date of birth in frontmatter, e.g. dateofbirth: 1994-03-12
                    DateOfBirth = GetDateOnly(fm, "dateofbirth")
                });
            }

            return items
                .OrderBy(k => k.Order)
                .ThenBy(k => k.Name)
                .ToList();
        }

        public OurKidItem? GetBySlug(string slug)
        {
            return GetAll().FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
        }

        private static (string front, string body) SplitFrontMatter(string text)
        {
            // expects:
            // ---
            // key: value
            // ---
            // body...
            var m = Regex.Match(text, @"\A\s*---\s*(?<front>[\s\S]*?)\s*---\s*(?<body>[\s\S]*)\z");
            if (!m.Success) return ("", text);
            return (m.Groups["front"].Value, m.Groups["body"].Value);
        }

        private static Dictionary<string, string> ParseFrontMatter(string front)
        {
            // Minimal parser:
            // - key: value
            // - key:
            //     - item
            // This is intentionally simple.
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(front)) return dict;

            string? currentKey = null;
            var lines = front.Replace("\r\n", "\n").Split('\n');

            foreach (var raw in lines)
            {
                var line = raw.TrimEnd();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // list item
                if (currentKey != null && line.TrimStart().StartsWith("- "))
                {
                    dict[currentKey] = dict.TryGetValue(currentKey, out var existing)
                        ? existing + "\n" + line.Trim().Substring(2).Trim()
                        : line.Trim().Substring(2).Trim();
                    continue;
                }

                // key: value OR key:
                var idx = line.IndexOf(':');
                if (idx <= 0) continue;

                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 1).Trim();

                currentKey = key;

                // if key: (empty) -> start list mode
                if (string.IsNullOrEmpty(val))
                {
                    if (!dict.ContainsKey(key)) dict[key] = "";
                }
                else
                {
                    dict[key] = val.Trim().Trim('"');
                }
            }

            return dict;
        }

        private static string? Get(Dictionary<string, string> fm, string key)
            => fm.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : null;

        private static int GetInt(Dictionary<string, string> fm, string key, int fallback)
            => fm.TryGetValue(key, out var v) && int.TryParse(v, out var n) ? n : fallback;

        private static List<string> GetList(Dictionary<string, string> fm, string key)
        {
            if (!fm.TryGetValue(key, out var v) || string.IsNullOrWhiteSpace(v)) return new List<string>();
            return v.Split('\n')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();
        }

        // NEW helper: parse date-only from frontmatter
        // Accepts e.g. "1994-03-12" (recommended), also "1994/03/12" or "1994-3-12".
        private static DateOnly? GetDateOnly(Dictionary<string, string> fm, string key)
        {
            if (!fm.TryGetValue(key, out var v) || string.IsNullOrWhiteSpace(v))
                return null;

            v = v.Trim().Trim('"');

            // Prefer ISO (yyyy-MM-dd) but be a bit forgiving.
            if (DateOnly.TryParse(v, out var d))
                return d;

            // Fallback: try normal DateTime parsing and convert.
            if (DateTime.TryParse(v, out var dt))
                return DateOnly.FromDateTime(dt);

            return null;
        }
    }
}
