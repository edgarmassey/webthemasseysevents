using WebTheMasseysEvents.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<EventStore>();
builder.Services.AddSingleton<OurKidsStore>();
builder.Services.AddSingleton<VideoStore>();
builder.Services.AddSingleton<SiteFlagsService>();

var app = builder.Build();

CurrentStore.Init(app.Environment.ContentRootPath);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.Run(); app.MapRazorPages();
app.Run();
