using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTheMasseysEvents.Models;
using WebTheMasseysEvents.Services;

namespace WebTheMasseysEvents.Pages
{
    public class OurKidsModel : PageModel
    {
        private readonly OurKidsStore _store;

        public OurKidsModel(OurKidsStore store)
        {
            _store = store;
        }

        public IReadOnlyList<OurKidItem> Kids { get; private set; } = Array.Empty<OurKidItem>();

        public void OnGet()
        {
            Kids = _store.GetAll();
        }
    }
}
