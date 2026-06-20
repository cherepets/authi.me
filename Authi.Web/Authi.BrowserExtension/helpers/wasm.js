import { dotnet } from '../wasm/dotnet.js?runtime=net10'

export class Wasm {
    static #assemblyExportsPromise = null;

    static async initAsync() {
        if (this.#assemblyExportsPromise) {
            return this.#assemblyExportsPromise;
        }
        this.#assemblyExportsPromise = (async () => {
            const { getAssemblyExports, getConfig } = await dotnet
                .withResourceLoader((_, __, defaultUri) => {
                    const uri = new URL(defaultUri);
                    uri.searchParams.set('runtime', 'net10');
                    return uri.href;
                })
                .create();

            const config = getConfig();
            return await getAssemblyExports(config.mainAssemblyName);
        })();
        return this.#assemblyExportsPromise;
    }
}
