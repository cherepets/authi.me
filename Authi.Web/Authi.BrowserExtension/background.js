const extensionApi = globalThis.browser ?? globalThis.chrome;

function getSystemTheme() {
    return globalThis.matchMedia?.('(prefers-color-scheme: dark)').matches
        ? 'dark'
        : 'light';
}

function applySystemTheme() {
    const action = extensionApi?.action;
    if (!action) {
        return;
    }

    const theme = getSystemTheme();
    const result = action.setIcon({ path: "icons/icon_" + theme + ".png" });
    result?.catch?.(() => { });
}

extensionApi?.runtime?.onStartup?.addListener(applySystemTheme);
extensionApi?.runtime?.onInstalled?.addListener(applySystemTheme);

applySystemTheme();
