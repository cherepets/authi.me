import { MainPage } from '/components/main-page.js';

import { Animator } from '/helpers/animator.js';
import { Cache } from '/helpers/cache.js';
import { DialogManager } from '/helpers/dialog-manager.js';
import { Loader } from '/helpers/loader.js';
import { Localization } from '/helpers/localization.js';
import { Navigator } from '/helpers/navigator.js';
import { Settings } from '/helpers/settings.js';
import { Task } from '/helpers/task.js';
import { Wasm } from '/helpers/wasm.js';

import '/helpers/json-converter.js';

export class SettingsPage extends HTMLElement {
    static async createAsync() {
        const instance = new SettingsPage(true);
        await instance.loadTemplateAsync();
        return instance;
    }

    constructor(usedCreateAsync) {
        if (!usedCreateAsync) {
            throw new Error('SettingsPage is supposed to be created using createAsync() function');
        }

        super();
    }

    async loadTemplateAsync() {
        const template = await Loader.loadTemplateAsync('/components/settings-page.html');
        this.appendChild(template.content.cloneNode(true));

        const pageTitle = this.querySelector('#settingsPageTitle');
        pageTitle.innerText = Localization.get('Settings.PageTitle');

        const cloudSyncHeader = this.querySelector('#settingsPageCloudSyncHeader');
        cloudSyncHeader.innerText = Localization.get('Settings.CloudSync');

        const syncToggle = this.querySelector('#settingsPageCloudSyncToggle');
        syncToggle.addEventListener('click', e => this.onSyncToggled(e.target.checked));

        const cloudSyncCopyButton = this.querySelector('#settingsPageCloudSyncPasteButton');
        cloudSyncCopyButton.innerText = Localization.get('Settings.CloudSyncPasteButtonTitle');
        cloudSyncCopyButton.addEventListener('click', () => this.onPasteClicked());

        const cloudSyncCopyButtonLabel = this.querySelector('#settingsPageCloudSyncPasteButtonLabel');
        cloudSyncCopyButtonLabel.innerText = Localization.get('Settings.CloudSyncPasteButtonCaption');

        const backupHeader = this.querySelector('#settingsPageBackupHeader');
        backupHeader.innerText = Localization.get('Settings.OfflineBackup');

        const backupImportButton = this.querySelector('#settingsPageBackupImportButton');
        backupImportButton.innerText = Localization.get('Settings.BackupImportButtonTitle');
        backupImportButton.addEventListener('click', () => this.onImportClicked());

        const backupImportButtonLabel = this.querySelector('#settingsPageBackupImportButtonLabel');
        backupImportButtonLabel.innerText = Localization.get('Settings.BackupImportButtonCaption');

        const closeLink = this.querySelector('#settingsPageCloseLink');
        closeLink.title = Localization.get('Generic.Close');
        closeLink.addEventListener('click', () => this.onCloseClicked());

        const getAppContent = this.querySelector('#settingsPageGetAppContent');
        getAppContent.innerHTML = `
            ${Localization.get('Settings.GetAppPrefix')}
            <a href="${Localization.get('Settings.GetAppLinkUrl')}" target="_blank">${Localization.get('Settings.GetAppLinkTitle')}</a>
            ${Localization.get('Settings.GetAppPostfix')}`;

        await this.updateVisualState();
    }

    async connectedCallback() {
        const ms = 180;
        await Animator.fadeInAsync(this.querySelector('#settingsPageContent'), ms, 60);
    }

    async updateVisualState() {
        const settings = await Settings.getAsync();
        const syncToggle = this.querySelector('#settingsPageCloudSyncToggle');
        const cloudSyncServerUrl = this.querySelector('#settingsPageCloudSyncServerUrl');
        const cloudSyncCopyButton = this.querySelector('#settingsPageCloudSyncPasteButton');
        const cloudSyncCopyButtonLabel = this.querySelector('#settingsPageCloudSyncPasteButtonLabel');
        const backupHeader = this.querySelector('#settingsPageBackupHeader');
        const backupImportButton = this.querySelector('#settingsPageBackupImportButton');
        const backupImportButtonLabel = this.querySelector('#settingsPageBackupImportButtonLabel');

        if (!settings || !settings.clientId || !settings.dataKey || !settings.syncPrivateKey || !settings.syncPublicKey) {
            syncToggle.checked = false;
            syncToggle.disabled = true;
            cloudSyncServerUrl.innerText = '';
            cloudSyncCopyButton.style.display = 'block';
            cloudSyncCopyButtonLabel.style.display = 'block';
            backupHeader.style.display = 'block';
            backupImportButton.style.display = 'block';
            backupImportButtonLabel.style.display = 'block';
        } else {
            syncToggle.checked = true;
            syncToggle.disabled = false;
            cloudSyncServerUrl.innerText = settings.serverUrl ?? '';
            cloudSyncCopyButton.style.display = 'none';
            cloudSyncCopyButtonLabel.style.display = 'none';
            backupHeader.style.display = 'none';
            backupImportButton.style.display = 'none';
            backupImportButtonLabel.style.display = 'none';
        }
    }

    onSyncToggled(isOn) {
        if (!isOn) {
            DialogManager.showDialog({
                title: Localization.get('Generic.Confirm'),
                message: Localization.get('Settings.CloudSyncConfirmDisable'),
                primaryButtonText: Localization.get('Generic.Yes'),
                cancelButtonText: Localization.get('Generic.No'),
                onPrimary: async () => {
                    await Settings.setAsync(null);
                    await this.updateVisualState();
                },
                onCancel: () => {
                    this.querySelector('#settingsPageCloudSyncToggle').checked = true;
                }
            });
        }
    }

    async onCloseClicked() {
        await Task.preventClicks(async () => {
            const ms = 180;
            await Promise.all([
                Animator.spinAsync(this.querySelector('#settingsPageCloseIcon'), ms, 90),
                Animator.fadeOutAsync(this.querySelector('#settingsPageTitle'), ms, 0),
                Animator.fadeOutAsync(this.querySelector('#settingsPageContent'), ms, 60)
            ]);
            await Navigator.navigateAsync(MainPage);
        });
    }

    async onPasteClicked() {
        await Task.preventClicks(async () => {
            try {
                this.style.opacity = 0.5;

                const syncCode = await navigator.clipboard.readText();

                const settings = await Settings.getAsync();

                const wasm = await Wasm.initAsync();
                const resultJson = await wasm.apiClient.consume(syncCode);
                const result = resultJson.fromJson();

                await Cache.setAsync([]);

                await Settings.setAsync({
                    clientId: result.clientId,
                    dataKey: result.dataKey.bytes,
                    syncPrivateKey: result.syncKeyPair.private.bytes,
                    syncPublicKey: result.syncKeyPair.public.bytes,
                    serverUrl: result.serverUrl
                });

                DialogManager.showDialog({
                    title: Localization.get('Generic.Success'),
                    message: Localization.get('Settings.CloudSyncSuccess'),
                    cancelButtonText: Localization.get('Generic.Close')
                });

                await this.updateVisualState();
            }
            catch (error) {
                DialogManager.showDialog({
                    title: Localization.get('Generic.Error'),
                    message: Localization.get('Settings.CloudSyncFailure'),
                    cancelButtonText: Localization.get('Generic.Close')
                });
                console.error(error);
            } finally {
                this.style.opacity = 1.0;
            }
        });
    }

    async onImportClicked() {
        await Task.preventClicks(async () => {
            try {
                this.style.opacity = 0.5;

                if (window.showOpenFilePicker) {
                    const [handle] = await window.showOpenFilePicker({
                        types: [{ description: "Backup Files", accept: { "text/plain": [".bak"] } }],
                        multiple: false
                    });
                    const file = await handle.getFile();
                    const text = await file.text();

                    const wasm = await Wasm.initAsync();
                    const resultJson = await wasm.backup.parse(text);
                    const result = resultJson.fromJson();

                    await Cache.setAsync(result);

                    await Settings.setAsync({ isOffline: true });

                    DialogManager.showDialog({
                        title: Localization.get('Generic.Success'),
                        message: Localization.get('Settings.BackupImportSuccess'),
                        cancelButtonText: Localization.get('Generic.Close')
                    });
                }

                await this.updateVisualState();
            }
            catch (error) {
                DialogManager.showDialog({
                    title: Localization.get('Generic.Error'),
                    message: Localization.get('Settings.BackupImportFailure'),
                    cancelButtonText: Localization.get('Generic.Close')
                });
                console.error(error);
            } finally {
                this.style.opacity = 1.0;
            }
        });
    }
}

customElements.define('settings-page', SettingsPage);
