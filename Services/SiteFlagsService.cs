using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace WebTheMasseysEvents.Services
{
    public class SiteFlagsService
    {
        private readonly IWebHostEnvironment _env;

        public SiteFlagsService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string KidsNewVersion
        {
            get
            {
                var path = Path.Combine(
                    _env.WebRootPath,
                    "content",
                    "kids-new-version.txt"
                );

                return File.Exists(path)
                    ? File.ReadAllText(path).Trim()
                    : "default";
            }
        }
    }
}
