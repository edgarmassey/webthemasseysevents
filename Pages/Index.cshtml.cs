using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTheMasseysEvents.Models;
using WebTheMasseysEvents.Services;
using System.Linq;

namespace WebTheMasseysEvents.Pages;

public class IndexModel : PageModel
{
    private readonly EventStore _store;

    public IndexModel(EventStore store)
    {
        _store = store;
    }

    public IReadOnlyList<EventItem> Events { get; private set; } = Array.Empty<EventItem>();

    public CurrentItem? LatestCurrent { get; private set; }

    public void OnGet()
    {
        Events = _store.GetAll().Take(9).ToList(); // show latest 9

        LatestCurrent = CurrentStore.LoadAll()
            .OrderByDescending(x => x.Date)
            .FirstOrDefault();
    }
}
