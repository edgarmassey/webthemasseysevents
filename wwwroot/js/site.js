// wwwroot/js/site.js

(function () {
    // Close the mobile navbar after clicking a nav link
    document.addEventListener("click", function (e) {
        const link = e.target.closest(".navbar a.nav-link");
        if (!link) return;

        const nav = document.getElementById("mainNavbar");
        if (!nav) return;

        if (nav.classList.contains("show")) {
            const instance = bootstrap.Collapse.getInstance(nav) || new bootstrap.Collapse(nav, { toggle: false });
            instance.hide();
        }
    });

    // When an accordion year opens, re-run any image/link setup (if you have it)
    document.addEventListener("shown.bs.collapse", function () {
        if (typeof initEventImageLinks === "function") {
            initEventImageLinks();
        }
    });
})();

