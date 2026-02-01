using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebTheMasseysEvents.Pages.OurKids
{
    public class GrandchildModel : PageModel
    {
        public string ParentSlug { get; private set; } = "";
        public string ChildSlug { get; private set; } = "";
        public string ChildName { get; private set; } = "";
        public List<string> Photos { get; private set; } = new();

        public DateOnly? DateOfBirth { get; private set; }
        public int? AgeToday { get; private set; }
        public string? BodyText { get; private set; }

        public IActionResult OnGet(string parentSlug, string childSlug)
        {
            ParentSlug = parentSlug;
            ChildSlug = childSlug;

            LoadGrandchildContent(parentSlug, childSlug);

            // Fallback name if not defined in markdown
            if (string.IsNullOrWhiteSpace(ChildName))
                ChildName = SlugToName(childSlug);

            var photosDir = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Photos",
                "OurKids",
                parentSlug,
                childSlug
            );

            if (!Directory.Exists(photosDir))
            {
                Photos = new();
                return Page();
            }

            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".webp", ".gif"
            };

            Photos = Directory.GetFiles(photosDir)
                .Where(f => allowed.Contains(Path.GetExtension(f)))
                .Select(Path.GetFileName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .OrderByDescending(n => n)
                .ToList()!;

            return Page();
        }

        private void LoadGrandchildContent(string parentSlug, string childSlug)
        {
            var mdPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Content",
                "OurKids",
                parentSlug,
                "Children",
                childSlug + ".md"
            );

            if (!System.IO.File.Exists(mdPath))
                return;

            var text = System.IO.File.ReadAllText(mdPath);

            var match = Regex.Match(
                text,
                @"\A\s*---\s*(?<front>[\s\S]*?)\s*---\s*(?<body>[\s\S]*)\z"
            );

            if (!match.Success)
                return;

            var front = match.Groups["front"].Value;
            BodyText = match.Groups["body"].Value.Trim();

            foreach (var raw in front.Replace("\r\n", "\n").Split('\n'))
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("name:", StringComparison.OrdinalIgnoreCase))
                {
                    ChildName = line.Substring(5).Trim().Trim('"');
                    continue;
                }

                if (line.StartsWith("born:", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("dateofbirth:", StringComparison.OrdinalIgnoreCase))
                {
                    var idx = line.IndexOf(':');
                    var val = line.Substring(idx + 1).Trim().Trim('"');

                    if (DateOnly.TryParse(val, out var dob))
                    {
                        DateOfBirth = dob;

                        var today = DateOnly.FromDateTime(DateTime.Today);
                        var age = today.Year - dob.Year;
                        if (today < dob.AddYears(age)) age--;
                        AgeToday = age;
                    }
                }
            }
        }

        private static string SlugToName(string slug)
        {
            var parts = slug.Split('-', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts.Select(p =>
                char.ToUpperInvariant(p[0]) + p[1..]));
        }
    }
}
