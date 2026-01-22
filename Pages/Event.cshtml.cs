using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTheMasseysEvents.Models;
using WebTheMasseysEvents.Services;

namespace WebTheMasseysEvents.Pages;

public class EventModel : PageModel
{
    private readonly EventStore _store;

    public EventModel(EventStore store)
    {
        _store = store;
    }

    public EventItem? Event { get; private set; }
    public string? CoverUrl { get; private set; }

    public IActionResult OnGet(string slug)
    {
        Event = _store.GetBySlug(slug);
        if (Event == null) return NotFound();

        // Your photos folder is: wwwroot/Photos/Events/{slug}/...
        if (!string.IsNullOrWhiteSpace(Event.Cover))
            CoverUrl = $"/Photos/Events/{Event.Slug}/{Event.Cover}";

        return Page();
    }
}
