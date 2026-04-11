import { Storage } from '/helpers/storage.js';

export const Cache = {
    async getAsync() {
        return await Storage.getAsync('cache', null);
    },

    async setAsync(value) {
        await Storage.setAsync('cache', value);
    }
}
