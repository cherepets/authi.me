import { Extension } from '/helpers/extension.js';

export const Storage = {
    async getAsync(key, defaultValue) {
        if (Extension.isInstalled) {
            const result = await Extension.api.storage.local.get({ [key]: defaultValue });
            return result[key];
        } else {
            const value = localStorage.getItem(key);
            if (value == null) {
                return defaultValue;
            }
            try {
                return JSON.parse(value);
            } catch (e) {
                console.error(`Error parsing storage key "${key}":`, e);
                return defaultValue;
            }
        }
        return defaultValue;
    },

    async setAsync(key, value) {
        if (Extension.isInstalled) {
            await Extension.api.storage.local.set({ [key]: value });
        } else {
            const json = JSON.stringify(value);
            localStorage.setItem(key, json);
        }
    }
};