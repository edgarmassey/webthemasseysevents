using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTheMasseysEvents.Services;

namespace WebTheMasseysEvents.Pages
{
    public class CurrentItemModel : PageModel
    {
        public CurrentItem? Item { get; private set; }
        public string CoverCaption { get; set; } = "";

        public IActionResult OnGet(string slug)
        {
            Item = CurrentStore.LoadBySlug(slug);
            if (Item == null) return NotFound();
            return Page();
        }
    }
}
