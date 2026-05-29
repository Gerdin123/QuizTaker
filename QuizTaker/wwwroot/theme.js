(() => {
    const storageKey = "quiztaker-theme";

    function normalize(theme) {
        return theme === "dark" ? "dark" : "light";
    }

    function preferredTheme() {
        const storedTheme = localStorage.getItem(storageKey);
        if (storedTheme === "dark" || storedTheme === "light") {
            return storedTheme;
        }

        return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    function apply(theme) {
        const nextTheme = normalize(theme);
        document.documentElement.dataset.theme = nextTheme;
        document.documentElement.classList.toggle("theme-dark", nextTheme === "dark");
        document.documentElement.classList.toggle("theme-light", nextTheme === "light");
        if (document.body) {
            document.body.dataset.theme = nextTheme;
            document.body.classList.toggle("theme-dark", nextTheme === "dark");
            document.body.classList.toggle("theme-light", nextTheme === "light");
        }

        return nextTheme;
    }

    window.quizTakerTheme = {
        get() {
            return apply(preferredTheme());
        },
        set(theme) {
            const nextTheme = apply(theme);
            localStorage.setItem(storageKey, nextTheme);
            return nextTheme;
        },
        applyStored() {
            return apply(preferredTheme());
        }
    };

    window.quizTakerTheme.applyStored();
    function applyStoredSoon() {
        window.quizTakerTheme.applyStored();
        window.setTimeout(window.quizTakerTheme.applyStored, 0);
        window.setTimeout(window.quizTakerTheme.applyStored, 25);
    }

    document.addEventListener("DOMContentLoaded", applyStoredSoon);
    document.addEventListener("enhancedload", applyStoredSoon);
    document.addEventListener("blazor-enhanced-load", applyStoredSoon);
    window.addEventListener("pageshow", window.quizTakerTheme.applyStored);
    window.addEventListener("storage", event => {
        if (event.key === storageKey) {
            window.quizTakerTheme.applyStored();
        }
    });
})();
