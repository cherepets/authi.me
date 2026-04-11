export const Extension = {
    get api() {
        const api = globalThis.browser ?? globalThis.chrome;
        if (api?.runtime?.id) {
            return api;
        }
        return null;
    },

    get isInstalled() {
        return this.api !== null;
    }
};