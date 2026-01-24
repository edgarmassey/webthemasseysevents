using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTheMasseysEvents.Models;
using WebTheMasseysEvents.Services;

namespace WebTheMasseysEvents.Pages;

public class IndexModel : PageModel
{
    private readonly EventStore _store;

    public IndexModel(EventStore store)
    {
        _store = store;
    }

    public IReadOnlyList<EventItem> Events { get; private set; } = Array.Empty<EventItem>();

    public void OnGet()
    {
        Events = _store.GetAll().Take(9).ToList(); // show latest 9
    }
}
