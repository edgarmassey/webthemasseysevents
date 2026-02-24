// wwwroot/js/site.js

(function () {
    // Close the mobile navbar after clicking a nav link
    document.addEventListener("click", function (e) {
        const link = e.target.closest(".navbar a.nav-link");
        if (!link) return;

        const nav = document.getElementById("mainNavbar");
        if (!nav) return;

        if (nav.classList.contains("show")) {
            if (window.bootstrap?.Collapse) {
                const instance = bootstrap.Collapse.getInstance(nav) || new bootstrap.Collapse(nav, { toggle: false });
                instance.hide();
            } else {
                nav.classList.remove("show");
            }
        }
    });

    // When an accordion year opens, re-run any image/link setup (if you have it)
    document.addEventListener("shown.bs.collapse", function () {
        if (typeof initEventImageLinks === "function") {
            initEventImageLinks();
        }
    });

    // ===== Shared image modal (Poetry + Home), with swipe =====
    // Requires a modal with id="poetryModal" and an img with id="poetryModalImg" on the page.
    const modal = document.getElementById("poetryModal");
    const modalImg = document.getElementById("poetryModalImg");

    if (!modal || !modalImg) return;

    let currentButtons = [];
    let currentIndex = -1;

    function showAt(index) {
        if (!currentButtons.length) return;

        const count = currentButtons.length;

        // wrap around
        if (index < 0) index = count - 1;
        if (index >= count) index = 0;

        const btn = currentButtons[index];
        const full = btn.getAttribute("data-full") || "";
        modalImg.src = full;

        currentIndex = index;
    }

    modal.addEventListener("show.bs.modal", function (event) {
        const btn = event.relatedTarget;
        if (!btn) return;

        // All thumbnails on the current page that open this modal
        currentButtons = Array.from(document.querySelectorAll('[data-bs-target="#poetryModal"][data-full]'));

        // Which one was clicked
        currentIndex = currentButtons.indexOf(btn);
        if (currentIndex < 0) currentIndex = 0;

        showAt(currentIndex);
    });

    modal.addEventListener("hidden.bs.modal", function () {
        modalImg.src = "";
        currentButtons = [];
        currentIndex = -1;
    });

    // Keyboard support (optional but useful)
    document.addEventListener("keydown", function (e) {
        if (!modal.classList.contains("show")) return;
        if (!currentButtons.length) return;

        if (e.key === "ArrowRight") showAt(currentIndex + 1);
        if (e.key === "ArrowLeft") showAt(currentIndex - 1);
    });

    // Swipe support
    let touchStartX = 0;
    let touchStartY = 0;

    function onTouchStart(e) {
        const t = e.touches && e.touches[0];
        if (!t) return;
        touchStartX = t.clientX;
        touchStartY = t.clientY;
    }

    function onTouchEnd(e) {
        if (!modal.classList.contains("show")) return;
        if (!currentButtons.length) return;

        const t = e.changedTouches && e.changedTouches[0];
        if (!t) return;

        const dx = t.clientX - touchStartX;
        const dy = t.clientY - touchStartY;

        // Ignore mostly vertical gestures (scroll)
        if (Math.abs(dy) > Math.abs(dx)) return;

        // Threshold in px
        const threshold = 50;
        if (Math.abs(dx) < threshold) return;

        if (dx < 0) showAt(currentIndex + 1);   // swipe left = next
        else showAt(currentIndex - 1);          // swipe right = prev
    }

    // Attach swipe to the image (best feel)
    modalImg.addEventListener("touchstart", onTouchStart, { passive: true });
    modalImg.addEventListener("touchend", onTouchEnd, { passive: true });
})();
document.addEventListener("hidden.bs.modal", function () {
    // Remove any leftover backdrops
    document.querySelectorAll(".modal-backdrop").forEach(b => b.remove());
    // Ensure body is clickable again
    document.body.classList.remove("modal-open");
    document.body.style.removeProperty("padding-right");
});
(() => {
    // Store "seen" per kid per NEW-period (newUntil) and per site version
    function getStorageKey() {
        const ver = window.SITE_FLAGS?.kidsNewVersion || "default";
        return "visitedKids_" + ver;
    }

    function loadVisited() {
        const key = getStorageKey();
        try { return JSON.parse(localStorage.getItem(key) || "{}"); }
        catch { return {}; }
    }

    function saveVisited(visited) {
        const key = getStorageKey();
        try { localStorage.setItem(key, JSON.stringify(visited)); } catch { }
    }

    function applyBadges() {
        const visited = loadVisited();

        const badges = document.querySelectorAll(".kid-new-badge, .kid-card-new-badge");
        if (!badges.length) return;

        badges.forEach(b => {
            const slug = b.getAttribute("data-kid-slug");
            const newUntil = b.getAttribute("data-new-until") || "";
            if (!slug) return;

            const seenSamePeriod = visited[slug] === newUntil;

            if (seenSamePeriod) {
                b.classList.add("kid-badge-hidden");
                b.style.setProperty("display", "none", "important");
                b.setAttribute("hidden", "");
            } else {
                b.classList.remove("kid-badge-hidden");
                b.style.removeProperty("display");
                b.removeAttribute("hidden");
            }
        });
    }

    function markSeenFromBody() {
        const body = document.body;
        if (!body) return;

        const page = body.dataset.page;
        const slug = body.dataset.kidSlug;
        if (page !== "kid-detail" || !slug) return;

        // On detail page we don't know newUntil, so just store a marker.
        // (Click handler on list page stores the actual newUntil, which is what controls reset)
        const visited = loadVisited();
        if (!visited[slug]) {
            visited[slug] = "__seen__";
            saveVisited(visited);
        }
    }

    function initKidBadges() {
        applyBadges();

        document.querySelectorAll("a.kid-card-link").forEach(a => {
            a.addEventListener("click", () => {
                const badge = a.querySelector(".kid-new-badge, .kid-card-new-badge");
                if (!badge) return;

                const slug = badge.getAttribute("data-kid-slug");
                const newUntil = badge.getAttribute("data-new-until") || "";
                if (!slug) return;

                const visited = loadVisited();
                visited[slug] = newUntil;      // <-- THIS is the important change
                saveVisited(visited);

                // hide immediately
                badge.classList.add("kid-badge-hidden");
                badge.style.setProperty("display", "none", "important");
                badge.setAttribute("hidden", "");
            });
        });

        markSeenFromBody();
    }

    document.addEventListener("DOMContentLoaded", initKidBadges);
    window.addEventListener("pageshow", initKidBadges);
})();    
    

   