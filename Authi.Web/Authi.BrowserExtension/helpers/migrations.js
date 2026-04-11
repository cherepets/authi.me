import { Cache } from '/helpers/cache.js';
import { Extension } from '/helpers/extension.js';
import { Settings } from '/helpers/settings.js';
import { Storage } from '/helpers/storage.js';

export const Migrations = {
    versions: {
        1: async () => {
            if (Extension.isInstalled) {
                const value = localStorage.getItem('cache');
                if (value) {
                    const parsed = JSON.parse(value);
                    await Cache.setAsync(parsed);

                    localStorage.removeItem('cache');
                    console.log('Moved cache to extension storage');
                }
            }
        },
        2: async () => {
            if (Extension.isInstalled) {
                const value = localStorage.getItem('settings');
                if (value) {
                    const parsed = JSON.parse(value);
                    await Settings.setAsync(parsed);

                    localStorage.removeItem('settings');
                    console.log('Moved settings to extension storage');
                }
            }
        },
    },

    async runAsync() {
        let lastVersion = await this.getLastVersionAsync();

        const availableVersions = Object.keys(this.versions).map(Number);
        const latestVersion = Math.max(...availableVersions, 0);

        while (lastVersion < latestVersion) {
            lastVersion++;
            console.log(`Migrating to v.${lastVersion}`);

            try {
                const migration = this.versions[lastVersion];
                await migration();
            } catch (error) {
                console.error(`Migration to v.${lastVersion} failed:`, error);
            }

            await this.setLastVersionAsync(lastVersion);
        }
    },

    async getLastVersionAsync() {
        return Storage.getAsync('schemaVersion', 0);
    },

    async setLastVersionAsync(version) {
        Storage.setAsync('schemaVersion', version);
    }
};