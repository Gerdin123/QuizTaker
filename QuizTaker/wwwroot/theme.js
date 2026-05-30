(() => {
    const storageKey = "quiztaker-theme";
    const themes = [
        "blue-light",
        "blue-dark",
        "gray-light",
        "gray-dark",
        "rose-light",
        "rose-dark",
        "violet-light",
        "violet-dark",
        "forest-light",
        "forest-dark"
    ];
    const aliases = {
        light: "blue-light",
        dark: "blue-dark",
        gray: "gray-light",
        rose: "rose-light",
        violet: "violet-dark",
        forest: "forest-dark"
    };

    function normalize(theme) {
        const nextTheme = aliases[theme] ?? theme;
        return themes.includes(nextTheme) ? nextTheme : "blue-light";
    }

    function preferredTheme() {
        const storedTheme = localStorage.getItem(storageKey);
        if (themes.includes(storedTheme) || aliases[storedTheme]) {
            return normalize(storedTheme);
        }

        return window.matchMedia("(prefers-color-scheme: dark)").matches ? "blue-dark" : "blue-light";
    }

    function apply(theme) {
        const nextTheme = normalize(theme);
        document.documentElement.dataset.theme = nextTheme;
        for (const themeName of themes) {
            document.documentElement.classList.toggle(`theme-${themeName}`, nextTheme === themeName);
        }

        if (document.body) {
            document.body.dataset.theme = nextTheme;
            for (const themeName of themes) {
                document.body.classList.toggle(`theme-${themeName}`, nextTheme === themeName);
            }
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
