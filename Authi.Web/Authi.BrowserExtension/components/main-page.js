import { CredentialItem } from '/components/credential-item.js';
import { SettingsPage } from '/components/settings-page.js';

import { Animator } from '/helpers/animator.js';
import { Cache } from '/helpers/cache.js';
import { Clipboard } from '/helpers/clipboard.js';
import { Loader } from '/helpers/loader.js';
import { Localization } from '/helpers/localization.js';
import { Navigator } from '/helpers/navigator.js';
import { Settings } from '/helpers/settings.js';
import { Task, CancellationToken } from '/helpers/task.js';
import { TotpGenerator } from '/helpers/totp-generator.js';
import { Wasm } from '/helpers/wasm.js';

import '/helpers/json-converter.js';

export class MainPage extends HTMLElement {
    #copyAnimationCtx;

    static async createAsync() {
        const instance = new MainPage(true);
        await instance.loadTemplateAsync();
        return instance;
    }

    constructor(usedCreateAsync) {
        if (!usedCreateAsync) {
            throw new Error('MainPage is supposed to be created using createAsync() function');
        }

        super();
    }

    async loadTemplateAsync() {
        const template = await Loader.loadTemplateAsync('/components/main-page.html');
        this.appendChild(template.content.cloneNode(true));

        const mainPageTitle = this.querySelector('#mainPageTitle');
        mainPageTitle.innerText = Localization.get('Generic.AppName');

        const settingsLink = this.querySelector('#mainPageSettingsLink');
        settingsLink.addEventListener('click', () => this.onSettingsClicked());
    }

    async connectedCallback() {
        const settings = await Settings.getAsync();

        if (!settings ||
            (
                (!settings.isOffline) &&
                (!settings.clientId || !settings.dataKey || !settings.syncPrivateKey || !settings.syncPublicKey)
            )
        ) {
            const notConfiguredPopup = this.querySelector('#mainPageNotConfiguredPopup');
            notConfiguredPopup.style.display = 'block';

            const settingsButton = this.querySelector('#mainPageSettingsButton');
            settingsButton.addEventListener('click', () => this.onConfigureClicked());
            return;
        }

        const credentialsView = this.querySelector('#mainPageCredentialsView');

        try {
            var cache = await Cache.getAsync();
            if (cache) {
                await this.renderCredentialsAsync(cache, credentialsView);
            }

            try {
                var wasm = await Wasm.initAsync();
                const resultJson = await wasm.apiClient.read(
                    settings.serverUrl,
                    settings.clientId,
                    settings.version ?? '00000000-0000-0000-0000-000000000000',
                    settings.dataKey,
                    settings.syncPrivateKey,
                    settings.syncPublicKey);
                const result = resultJson.fromJson();

                if (result.hasChanges) {
                    await Cache.setAsync(result.credentials);

                    settings.version = result.version;
                    await Settings.setAsync(settings);

                    await this.renderCredentialsAsync(result.credentials, credentialsView);
                }
            }
            catch (error) {
                if (cache) {
                    console.log('No server data, using offline mode');
                } else {
                    throw error;
                }
            }

            this.updateLoop();
        }
        catch (error) {
            credentialsView.style.display = 'none';

            const errorPopup = this.querySelector('#mainPageErrorPopup');
            errorPopup.style.display = 'block';

            const retryButton = this.querySelector('#mainPageRetryButton');
            retryButton.addEventListener('click', () => this.onRetryClicked());

            console.error(error);
        }
    }

    async onSettingsClicked() {
        await Task.preventClicks(async () => {
            const ms = 180;
            await Promise.all([
                Animator.spinAsync(this.querySelector('#mainPageSettingsIcon'), ms, 60),
                Animator.fadeOutAsync(this.querySelector('#mainPageCredentialsView'), ms, 60),
                Animator.fadeOutAsync(this.querySelector('#mainPageTitle'), ms, 0)
            ]);
            await Navigator.navigateAsync(SettingsPage);
        });
    }

    async onConfigureClicked() {
        await Task.preventClicks(async () => {
            await Animator.fadeOutAsync(this.querySelector('#mainPageTitle'), 60, 0);
            await Navigator.navigateAsync(SettingsPage);
        });
    }

    async onRetryClicked() {
        await Navigator.navigateAsync(MainPage);
    }

    async onCredentialItemClicked(totp) {
        if (this.#copyAnimationCtx) {
            this.#copyAnimationCtx.cancel();
        }

        await Clipboard.writeAsync(totp.replace(' ', ''));
        const copyNotification = this.querySelector('#mainPageCopyNotification');
        copyNotification.style.display = 'inline-block';
        copyNotification.style.opacity = 1;

        const ctx = new CancellationToken();
        this.#copyAnimationCtx = ctx;

        await Task.sleepAsync(1000);
        if (!ctx.cancelled) {
            await Animator.fadeOutAsync(copyNotification, 180, 32, ctx);
        }
    }

    async renderCredentialsAsync(credentials, credentialsView) {
        credentialsView.innerHTML = '';

        for (const record of credentials) {
            const item = await CredentialItem.createAsync(record.title, record.subtitle, record.secret, totp => this.onCredentialItemClicked(totp));
            const li = document.createElement('li');
            li.appendChild(item);
            credentialsView.appendChild(li);
        }
    }

    async updateLoop() {
        const progressBarWrapper = this.querySelector('#mainPageProgressBarWrapper');
        progressBarWrapper.animate([
            { opacity: 0 },
            { opacity: 1 }
        ], {
            duration: 1000, easing: 'ease-in'
        });

        let validFor = TotpGenerator.getRemainingMs();
        while (true) {
            this.totpRefreshed(validFor);
            await Task.sleepAsync(validFor);
            validFor = TotpGenerator.getUpdateMs();
        }
    }

    totpRefreshed(validFor) {
        Array.from(this.querySelectorAll('credential-item')).forEach(async credentialItem => {
            await credentialItem.refreshTotpAsync();
        });

        const progressBar = this.querySelector('#mainPageProgressBar');
        const progress = (TotpGenerator.getUpdateMs() - validFor) / TotpGenerator.getUpdateMs();

        progressBar.animate([
            { transform: 'scaleX(' + progress + ')' },
            { transform: 'scaleX(' + 1 + ')' }
        ], {
            duration: validFor
        });
    }
}

customElements.define('main-page', MainPage);
