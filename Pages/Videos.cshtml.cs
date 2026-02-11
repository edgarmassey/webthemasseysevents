using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTheMasseysEvents.Services;

namespace WebTheMasseysEvents.Pages;

public class VideosModel : PageModel
{
    private readonly VideoStore _store;

    public VideosModel(VideoStore store)
    {
        _store = store;
    }

    public IReadOnlyList<VideoItem> Videos { get; private set; } = new List<VideoItem>();

    public void OnGet()
    {
        Videos = _store.GetAll();
    }
}
