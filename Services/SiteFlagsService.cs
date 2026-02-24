using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

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
            try
            {
                var path = Path.Combine(_env.ContentRootPath, "Content", "OurKids", "kidsNewVersion.txt");
                if (!File.Exists(path)) return "default";

                var v = File.ReadAllText(path).Trim();
                return string.IsNullOrWhiteSpace(v) ? "default" : v;
            }
            catch
            {
                return "default";
            }
        }
    }
}