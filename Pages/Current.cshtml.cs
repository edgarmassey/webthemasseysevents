using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTheMasseysEvents.Services;

namespace WebTheMasseysEvents.Pages
{
    public class CurrentModel : PageModel
    {
        public List<CurrentItem> Items { get; private set; } = new();

        public void OnGet()
        {
            Items = CurrentStore.LoadAll()
                .OrderByDescending(x => x.Date)
                .Take(20)
                .ToList();
        }
    }
}
