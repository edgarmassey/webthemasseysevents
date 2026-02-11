using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTheMasseysEvents.Services;

namespace WebTheMasseysEvents.Pages;

public class VideoModel : PageModel
{
    private readonly VideoStore _store;

    public VideoModel(VideoStore store)
    {
        _store = store;
    }

    public VideoItem? Video { get; private set; }

    public IActionResult OnGet(string slug)
    {
        Video = _store.GetBySlug(slug);
        if (Video == null)
            return NotFound();

        return Page();
    }
}
