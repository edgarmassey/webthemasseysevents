using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTheMasseysEvents.Models;
using WebTheMasseysEvents.Services;

namespace WebTheMasseysEvents.Pages.OurKids
{
    public class OurKidModel : PageModel
    {
        private readonly OurKidsStore _store;

        public OurKidModel(OurKidsStore store)
        {
            _store = store;
        }

        public OurKidItem? Kid { get; private set; }

        public IReadOnlyList<GrandchildLink> Grandchildren { get; private set; }
            = Array.Empty<GrandchildLink>();

        public IActionResult OnGet(string slug)
        {
            Kid = _store.GetBySlug(slug);
            if (Kid == null) return NotFound();

            Grandchildren = _store.GetGrandchildrenForParent(slug);

            return Page();
        }
    }
}
