import { Storage } from '/helpers/storage.js';

export const Settings = {
    async getAsync() {
        return await Storage.getAsync('settings', null);
    },

    async setAsync(value) {
        await Storage.setAsync('settings', value);
    }
}
