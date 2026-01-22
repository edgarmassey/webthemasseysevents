using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTheMasseysEvents.Models;
using WebTheMasseysEvents.Services;

namespace WebTheMasseysEvents.Pages;

public class EventsModel : PageModel
{
    private readonly EventStore _store;

    public EventsModel(EventStore store)
    {
        _store = store;
    }

    public IReadOnlyList<EventItem> Events { get; private set; } = Array.Empty<EventItem>();

    public void OnGet()
    {
        Events = _store.GetAll();
    }
}
