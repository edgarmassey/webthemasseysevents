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

                var coverFile = Get(fm, "cover")?.Trim();

                items.Add(new OurKidItem
                {
                    Slug = slug,
                    Name = Get(fm, "name") ?? slug,
                    Partner = Get(fm, "partner"),
                    Location = Get(fm, "location"),
                    Order = GetInt(fm, "order", 100),

                    Cover = coverFile,

                    // load all photos from folder
                    HomePhotos = GetPhotoFilesForSlug(slug),

                    KidsLink = Get(fm, "kids_link"),
                    Blurb = Get(fm, "blurb"),
                    BodyMarkdown = body.Trim(),

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

        // =========================
        // Photos for parent (OurKid)
        // =========================
        private List<string> GetPhotoFilesForSlug(string slug)
        {
            // wwwroot/Photos/OurKids/{slug}/
            var photosDir = Path.Combine(_env.WebRootPath, "Photos", "OurKids", slug);
            if (!Directory.Exists(photosDir)) return new List<string>();

            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

            return Directory.GetFiles(photosDir)
                .Where(f => allowed.Contains(Path.GetExtension(f)))
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .OrderByDescending(name => name) // newest first if you use date prefixes
                .ToList()!;
        }

        // =========================
        // Grandchildren auto-discover
        // =========================

        public IReadOnlyList<GrandchildLink> GetGrandchildrenForParent(string parentSlug)
        {
            // wwwroot/Photos/OurKids/{parentSlug}/{childSlug}/...
            var parentDir = Path.Combine(_env.WebRootPath, "Photos", "OurKids", parentSlug);
            if (!Directory.Exists(parentDir)) return Array.Empty<GrandchildLink>();

            var childDirs = Directory.GetDirectories(parentDir);

            var result = new List<GrandchildLink>();

            foreach (var dir in childDirs)
            {
                var childSlug = Path.GetFileName(dir);

                if (string.IsNullOrWhiteSpace(childSlug)) continue;
                if (childSlug.StartsWith("_")) continue; // optional convention to ignore folders like _thumbs

                result.Add(new GrandchildLink
                {
                    Slug = childSlug,
                    Name = SlugToName(childSlug),
                    BornYear = TryReadGrandchildBornYear(parentSlug, childSlug)
                });
            }

            return result
                .OrderBy(g => g.BornYear ?? 9999)
                .ThenBy(g => g.Name)
                .ToList();
        }

        private static string SlugToName(string slug)
        {
            // "ella-rose" -> "Ella Rose"
            var parts = slug.Split('-', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
        }

        private int? TryReadGrandchildBornYear(string parentSlug, string childSlug)
        {
            // Content/OurKids/{parentSlug}/Children/{childSlug}.md
            var mdPath = Path.Combine(_env.ContentRootPath, "Content", "OurKids", parentSlug, "Children", childSlug + ".md");
            if (!File.Exists(mdPath)) return null;

            var text = File.ReadAllText(mdPath);

            var (front, _) = SplitFrontMatter(text);
            var fm = ParseFrontMatter(front);

            // allow either key
            var born = Get(fm, "born") ?? Get(fm, "dateofbirth");
            if (born is null) return null;

            if (DateTime.TryParse(born.Trim().Trim('"'), out var dt)) return dt.Year;
            return null;
        }

        // =========================
        // Helpers (frontmatter etc.)
        // =========================

        private static (string front, string body) SplitFrontMatter(string text)
        {
            var m = Regex.Match(text, @"\A\s*---\s*(?<front>[\s\S]*?)\s*---\s*(?<body>[\s\S]*)\z");
            if (!m.Success) return ("", text);
            return (m.Groups["front"].Value, m.Groups["body"].Value);
        }

        private static Dictionary<string, string> ParseFrontMatter(string front)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(front)) return dict;

            string? currentKey = null;
            var lines = front.Replace("\r\n", "\n").Split('\n');

            foreach (var raw in lines)
            {
                var line = raw.TrimEnd();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (currentKey != null && line.TrimStart().StartsWith("- "))
                {
                    dict[currentKey] = dict.TryGetValue(currentKey, out var existing)
                        ? existing + "\n" + line.Trim().Substring(2).Trim()
                        : line.Trim().Substring(2).Trim();
                    continue;
                }

                var idx = line.IndexOf(':');
                if (idx <= 0) continue;

                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 1).Trim();

                currentKey = key;

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

        private static DateOnly? GetDateOnly(Dictionary<string, string> fm, string key)
        {
            if (!fm.TryGetValue(key, out var v) || string.IsNullOrWhiteSpace(v))
                return null;

            v = v.Trim().Trim('"');

            if (DateOnly.TryParse(v, out var d))
                return d;

            if (DateTime.TryParse(v, out var dt))
                return DateOnly.FromDateTime(dt);

            return null;
        }
    }
}
