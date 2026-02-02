using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace WebTheMasseysEvents.Pages.Poetry;

public class IndexModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public IndexModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    public List<string> ImageUrls { get; private set; } = new();

    public void OnGet()
    {
        // Physical folder on disk
        var dir = Path.Combine(_env.WebRootPath, "Photos", "Poetry", "English");
        if (!Directory.Exists(dir)) return;

        var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

        // Sort newest first (by file name). If you name them like 2026-01-15_poem.jpg, this works great.
        var files = Directory.GetFiles(dir)
            .Where(f => exts.Contains(Path.GetExtension(f)))
            .OrderByDescending(f => Path.GetFileName(f))
            .ToList();

        ImageUrls = files
            .Select(f => "/Photos/Poetry/English/" + Path.GetFileName(f))
            .ToList();
    }
}
