using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTheMasseysEvents.Models;
using WebTheMasseysEvents.Services;

namespace WebTheMasseysEvents.Pages;

public class EventModel : PageModel
{
    private readonly EventStore _store;
    private readonly IWebHostEnvironment _env;

    public EventModel(EventStore store, IWebHostEnvironment env)
    {
        _store = store;
        _env = env;
    }

    public EventItem? Event { get; private set; }
    public string? CoverUrl { get; private set; }

    public IActionResult OnGet(string slug, string? backYear)
    {
        Event = _store.GetBySlug(slug);
        if (Event == null) return NotFound();

        // Cover (optional)
        if (!string.IsNullOrWhiteSpace(Event.Cover))
            CoverUrl = $"/Photos/Events/{Event.Slug}/{Event.Cover}";

        // Load gallery photos from: wwwroot/Photos/Events/{slug}/
        var dir = Path.Combine(_env.WebRootPath, "Photos", "Events", Event.Slug);

        if (Directory.Exists(dir))
        {
            Event.PhotoFiles = Directory.GetFiles(dir)
                .Select(Path.GetFileName)
                .Where(f =>
                    f != null &&
                    !string.Equals(f, Event.Cover, StringComparison.OrdinalIgnoreCase) && // avoid duplicate cover in gallery
                    (f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                     f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                     f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                     f.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(f => f) // filename sort, good if you prefix with date
                .ToList()!;
        }
        else
        {
            Event.PhotoFiles = new List<string>();
        }

        return Page();
    }
}
