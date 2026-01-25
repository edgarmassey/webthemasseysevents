# WebTheMasseysEvents

Lightweight Razor Pages site (ASP.NET Core, .NET 8) for listing family events and photos.

This README is an onboarding / quick-reference for a new co-developer — how the project is organized, how to run it, and where to find the important bits.

---

## Project summary

- Framework: .NET 8 (C# 12), Razor Pages
- Purpose: Static-content-driven site. Events are authored as Markdown files and served with optional photo galleries per event.

## Repo layout (important files)

- `Program.cs` - app startup, dependency injection and middleware
- `Pages/`
  - `Index.cshtml` (+ `Index.cshtml.cs`) - landing page
  - `Kids.cshtml`, `Home.cshtml` - placeholder pages
  - `Shared/Events.cshtml` (+ `Shared/Events.cshtml.cs`) - events list page (route: `/events`)
  - `Event.cshtml` (+ `Event.cshtml.cs`) - single-event page, expects a `slug` parameter
  - `_Layout.cshtml`, `_ViewImports.cshtml`, `_ViewStart.cshtml` - shared layout and tag helper imports
- `Services/EventStore.cs` - reads `Content/Events/*.md`, parses front matter, builds `EventItem` instances
- `Models/EventItem.cs` - event model (Slug, Title, Date, Location, Cover, BodyMarkdown, Number, PhotoFiles)
- `Content/Events/` - Markdown files for events (`{slug}.md`)
- `wwwroot/Photos/Events/{slug}/` - photo folders for each event


## How events are authored

Event files live under `Content/Events/{slug}.md`. Files use a simple front-matter format:

```
---
title: My Event
date: 2026-06-12
location: Backyard
cover: 01.jpg
number: 5
---

Event body in markdown...
```

- Supported front-matter keys: `title`, `date` (parseable by .NET), `location`, `cover` (filename contained in `wwwroot/Photos/Events/{slug}`), `number` (int).
- If `date` is missing or can't be parsed, `EventStore` falls back to the file creation time.
- Photos for an event should be placed under `wwwroot/Photos/Events/{slug}/`. Filenames are preserved; the `cover` value must match one of the files there to be used as the cover.

## Routing

- Events list: GET `/events` (page at `Pages/Shared/Events.cshtml` with `@page "/events"`)
- Single event: `Pages/Event.cshtml` (link generated as `/Event/{slug}` in the UI)
- Other pages: `/`, `/Kids`, `/Home`

Note: `asp-page` TagHelper expects a page path under `Pages` (for example `asp-page="/Index"` or `asp-page="/Kids"`). Using raw `href` (e.g. `/events`) is fine for explicit routes.

## Run locally

- Using CLI:
  - `dotnet build`
  - `dotnet run`
  - Open the URL reported in console (usually `https://localhost:5001`)

- Using Visual Studio:
  - Open the solution and run (F5)

## Development notes & gotchas

- There was previously a duplicate file with a malformed path (`csharp Services/EventStore.cs`) that caused ambiguous-type compiler errors. Keep file names and paths clean; avoid leading/trailing spaces in file names.
- If you change markdown files or photos frequently during development, EventStore currently rereads files per request (no caching). Consider adding `IMemoryCache` for production and short caching for development convenience.
- `EventStore.GetAll()` sorts events descending by date. Keep ordering logic in a single place if you add further callers.
- If you want simpler routing for the Events page, consider moving `Pages/Shared/Events.cshtml` to `Pages/Events.cshtml` (then `asp-page="/Events"` will map directly). The current route works, but naming/location can be made more conventional.

## Suggested improvements (short-term)

- Move `Pages/Shared/Events.cshtml` to `Pages/Events.cshtml` and create `Pages/Events.cshtml.cs` PageModel for any server logic (pagination/filtering).
- Add caching in `EventStore` (inject `IMemoryCache`) with a short expiration.
- Improve event list UI with friendly dates (e.g. `MMM d, yyyy`), cover thumbnail column, and empty-state message.
- Add unit tests for `EventStore` parsing logic (parse front matter, date fallback, photo discovery).
- Add `CONTRIBUTING.md` with branch/PR guidelines and developer setup steps.

## Commands & common tasks

- Build: `dotnet build`
- Run: `dotnet run`
- Test (if tests added): `dotnet test`
- Add memory cache (if implementing): in `Program.cs` add `builder.Services.AddMemoryCache();` and update `EventStore` constructor to accept `IMemoryCache`.

## Git workflow recommended

- Branch: `feature/<short-description>`
- Commit messages: short summary + brief detail
- Open pull request with description and screenshots for UI changes

## Where to look when something breaks

- Blank page / navigation not working: check `@page` route lines and the `asp-page` or `href` used in the link.
- Events not appearing / wrong order: check `Content/Events/*.md` front matter and `EventStore.GetAll()` parsing/ordering.
- Duplicate type errors: search the repo for multiple files declaring the same class name.

---

If you want, I can:
- Add a `CONTRIBUTING.md` with these policies
- Move the Events page to `Pages/Events.cshtml` and update links across the site
- Add a basic `README` badge or usage examples

Tell me which of the follow-up tasks you'd like me to do.
