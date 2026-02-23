using System;
using System.IO;
using System.Linq;
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
            Kids = _store.GetAll().ToList();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var logPath = Path.Combine(AppContext.BaseDirectory, "Content", "OurKids", "ourkids_newlog.txt");

            foreach (var k in Kids)
            {
                var showNew = k.IsNew && (k.NewUntil is null || today <= k.NewUntil.Value);

                try
                {
                    System.IO.File.AppendAllText(
                        logPath,
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} kid={k.Slug} IsNew={k.IsNew} NewUntil={k.NewUntil?.ToString() ?? "null"} today={today} showNew={showNew}{Environment.NewLine}"
                    );
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(
                        Path.Combine(AppContext.BaseDirectory, "ourkids_newlog_error.txt"),
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {ex}{Environment.NewLine}"
                    );
                }
            }
        }
    }
}