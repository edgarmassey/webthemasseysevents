using Markdig;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WebTheMasseysEvents.Services
{
    public static class CurrentStore
    {
        // Set from Program.cs (ContentRootPath)
        private static string _contentRoot = "";

        // Call once at startup:
        // CurrentStore.Init(app.Environment.ContentRootPath);
        public static void Init(string contentRootPath)
        {
            _contentRoot = contentRootPath ?? "";
        }

        // <project root>/content/current/*.md
        private static string ContentDir =>
            Path.Combine(_contentRoot, "content", "current");

        private static readonly MarkdownPipeline Pipeline =
            new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

        public static List<CurrentItem> LoadAll()
        {
            EnsureInit();

            var dir = Path.GetFullPath(ContentDir);
            if (!Directory.Exists(dir)) return new List<CurrentItem>();

            var files = Directory.GetFiles(dir, "*.md", SearchOption.TopDirectoryOnly);

            var items = new List<CurrentItem>();
            foreach (var file in files)
            {
                var item = LoadFromFile(file);
                if (item != null) items.Add(item);
            }

            return items;
        }

        public static CurrentItem? LoadBySlug(string slug)
        {
            EnsureInit();

            var dir = Path.GetFullPath(ContentDir);
            if (!Directory.Exists(dir)) return null;

            var files = Directory.GetFiles(dir, "*.md", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                var item = LoadFromFile(file);
                if (item != null &&
                    string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
        }

        private static CurrentItem? LoadFromFile(string path)
        {
            var text = File.ReadAllText(path, Encoding.UTF8);

            ParseFrontMatter(text, out var meta, out var body);

            var slug = meta.TryGetValue("slug", out var fmSlug) && !string.IsNullOrWhiteSpace(fmSlug)
                ? fmSlug.Trim()
                : SlugFromFilename(path);

            var title = meta.TryGetValue("title", out var t) && !string.IsNullOrWhiteSpace(t)
                ? t.Trim()
                : slug;

            var date = ParseDate(meta.TryGetValue("date", out var d) ? d : null)
                       ?? DateFromFilename(path)
                       ?? File.GetLastWriteTime(path);

            var summary = meta.TryGetValue("summary", out var s) ? s.Trim() : "";
            var cover = meta.TryGetValue("cover", out var c) ? c.Trim() : "";
            var coverCaption = meta.TryGetValue("covercaption", out var cc) ? cc.Trim() : "";

            // If cover is not a web path, assume /Photos/...
            if (!string.IsNullOrWhiteSpace(cover) &&
                !cover.StartsWith("/") &&
                !cover.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                cover = "/Photos/" + cover;
            }

            // ----- Build gallery from the cover folder -----
            var photoFolderWeb = "";
            var gallery = new List<string>();

            // If cover is like /Photos/Current/Niklas40th/NiklasBirthday.jpg
            // then folder becomes /Photos/Current/Niklas40th
            if (!string.IsNullOrWhiteSpace(cover) &&
                cover.StartsWith("/Photos/", StringComparison.OrdinalIgnoreCase))
            {
                var lastSlash = cover.LastIndexOf('/');
                if (lastSlash > 0)
                    photoFolderWeb = cover.Substring(0, lastSlash);
            }

            if (!string.IsNullOrWhiteSpace(photoFolderWeb))
            {
                var wwwroot = Path.Combine(_contentRoot, "wwwroot");
                var folderDisk = Path.Combine(
                    wwwroot,
                    photoFolderWeb.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );

                if (Directory.Exists(folderDisk))
                {
                    var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

                    gallery = Directory.GetFiles(folderDisk)
                        .Where(f => exts.Contains(Path.GetExtension(f)))
                        .Select(f => photoFolderWeb + "/" + Path.GetFileName(f))
                        .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
            }

            // Exclude cover so it doesn't show twice (full path OR just filename)
            if (!string.IsNullOrWhiteSpace(cover) && gallery.Count > 0)
            {
                var coverFile = Path.GetFileName(cover);

                gallery = gallery
                    .Where(x =>
                        !string.Equals(x, cover, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(Path.GetFileName(x), coverFile, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var html = MarkdownService.ToHtml(body);

            // wrap inline markdown images with modal click-to-zoom + filename caption
            html = WrapInlineImagesWithModal(html, slug);

            return new CurrentItem
            {
                Slug = slug,
                Title = title,
                Date = date,
                Summary = summary,
                Cover = cover,
                CoverCaption = coverCaption,
                Markdown = body ?? "",
                Html = html,
                SourcePath = path,

                PhotoFolderWeb = photoFolderWeb,
                GalleryImagesWeb = gallery
            };
        }

        private static void EnsureInit()
        {
            if (string.IsNullOrWhiteSpace(_contentRoot))
                throw new InvalidOperationException("CurrentStore.Init(contentRootPath) was not called at startup.");
        }

        private static void ParseFrontMatter(string text, out Dictionary<string, string> meta, out string body)
        {
            meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            body = text;

            using var sr = new StringReader(text);

            var first = sr.ReadLine();
            if (first == null || first.Trim() != "---")
                return;

            var lines = new List<string>();
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Trim() == "---")
                    break;
                lines.Add(line);
            }

            foreach (var l in lines)
            {
                var idx = l.IndexOf(':');
                if (idx <= 0) continue;

                var key = l.Substring(0, idx).Trim();
                var val = l.Substring(idx + 1).Trim();

                if (!string.IsNullOrWhiteSpace(key))
                    meta[key] = val;
            }

            body = sr.ReadToEnd();
        }

        private static DateTime? ParseDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            if (DateTime.TryParseExact(s.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out var dt))
                return dt;

            if (DateTime.TryParse(s.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
                return dt;

            return null;
        }

        private static DateTime? DateFromFilename(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (name.Length < 10) return null;

            var prefix = name.Substring(0, 10);
            if (DateTime.TryParseExact(prefix, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out var dt))
                return dt;

            return null;
        }

        private static string SlugFromFilename(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);

            if (name.Length > 11 &&
                char.IsDigit(name[0]) &&
                name[4] == '-' &&
                name[7] == '-' &&
                name[10] == '-')
            {
                name = name.Substring(11);
            }

            return Slugify(name);
        }

        private static string Slugify(string s)
        {
            s = s.Trim().ToLowerInvariant();

            var sb = new StringBuilder(s.Length);
            bool dash = false;

            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                    dash = false;
                }
                else if (ch == ' ' || ch == '-' || ch == '_' || ch == '.')
                {
                    if (!dash && sb.Length > 0)
                    {
                        sb.Append('-');
                        dash = true;
                    }
                }
            }

            while (sb.Length > 0 && sb[^1] == '-') sb.Length--;

            return sb.Length == 0 ? "item" : sb.ToString();
        }

        // Inline image -> click-to-zoom modal         // Inline image -> click-to-zoom modal + filename caption
        private static string WrapInlineImagesWithModal(string html, string idPrefix)
        {
            if (string.IsNullOrWhiteSpace(html)) return html;

            int i = 0;

            return Regex.Replace(
                html,
                @"<img\b[^>]*\bsrc\s*=\s*([""'])(?<src>.*?)\1[^>]*>",
                match =>
                {
                    i++;

                    var imgTag = match.Value;
                    var src = match.Groups["src"].Value;

                    // filename from src (strip ?query / #hash)
                    var cleanSrc = src.Split('?', '#')[0];
                    var fileName = System.IO.Path.GetFileName(cleanSrc);


                    var safePrefix = Slugify(idPrefix);
                    var modalId = $"imgModal-{safePrefix}-{i}";

                    // add thumbnail style
                    var thumbImgTag = imgTag;
                    if (Regex.IsMatch(thumbImgTag, @"\bstyle\s*=", RegexOptions.IgnoreCase))
                    {
                        thumbImgTag = Regex.Replace(
                            thumbImgTag,
                            @"\bstyle\s*=\s*([""'])(?<s>.*?)\1",
                            m => $"style=\"{m.Groups["s"].Value};max-height:260px;width:auto;cursor:pointer;\"",
                            RegexOptions.IgnoreCase
                        );
                    }
                    else
                    {
                        thumbImgTag = Regex.Replace(
                            thumbImgTag,
                            @"\s*/?>$",
                            " style=\"max-height:260px;width:auto;cursor:pointer;\" />"
                        );
                    }

                    return $@"
<div class=""inline-img-block mb-3"">
  <button type=""button""
          class=""p-0 border-0 bg-transparent""
          data-bs-toggle=""modal""
          data-bs-target=""#{modalId}""
          aria-label=""Open image full size"">
    {thumbImgTag}
  </button>
  <div class=""inline-img-caption text-muted"">{fileName}</div>
</div>

<div class=""modal fade"" id=""{modalId}"" tabindex=""-1"" aria-hidden=""true"">
  <div class=""modal-dialog modal-dialog-centered modal-lg"">
    <div class=""modal-content bg-transparent border-0"">
      <button type=""button""
              class=""btn-close btn-close-white ms-auto me-2 mt-2""
              data-bs-dismiss=""modal""
              aria-label=""Close""></button>

      <img src=""{src}"" class=""img-fluid rounded"" alt="""" />
    </div>
  </div>
</div>
";
                },
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );
        }
    }
}
