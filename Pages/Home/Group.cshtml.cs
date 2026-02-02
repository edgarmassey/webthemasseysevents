using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTheMasseysEvents.Pages.Home;

public class GroupModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public GroupModel(IWebHostEnvironment env) => _env = env;

    public string Group { get; private set; } = "";
    public List<string> Images { get; private set; } = new();

    public IActionResult OnGet(string group)
    {
        Group = group;

        var dir = Path.Combine(_env.WebRootPath, "Photos", "Home", group);
        if (!Directory.Exists(dir)) return NotFound();

        Images = Directory.GetFiles(dir, "*.jpg")
            .OrderByDescending(f => f)
            .Select(f => $"/Photos/Home/{group}/{Path.GetFileName(f)}")
            .ToList();

        return Page();
    }
}
 
