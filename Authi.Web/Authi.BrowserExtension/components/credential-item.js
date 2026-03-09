import { Animator } from '/helpers/animator.js';
import { Loader } from '/helpers/loader.js';
import { Task, CancellationToken } from '/helpers/task.js';
import { TotpGenerator } from '/helpers/totp-generator.js';

export class CredentialItem extends HTMLElement {
    #copyAnimationCtx;

    #title;
    #subtitle;
    #secret;
    #onClick;

    #totp;

    static async createAsync(title, subtitle, secret, onClick) {
        const instance = new CredentialItem(title, subtitle, secret, onClick, true);
        await instance.loadTemplateAsync();
        return instance;
    }

    constructor(title, subtitle, secret, onClick, usedCreateAsync) {
        if (!usedCreateAsync) {
            throw new Error('CredentialItem is supposed to be created using createAsync() function');
        }

        super();

        this.#title = title;
        this.#subtitle = subtitle;
        this.#secret = secret;
        this.#onClick = onClick;
    }

    async loadTemplateAsync() {
        const template = await Loader.loadTemplateAsync('/components/credential-item.html');
        this.appendChild(template.content.cloneNode(true));

        const rootElement = this.querySelector('#credentialItemRoot');
        const titleSpan = this.querySelector('#credentialItemTitle');
        const subtitleSpan = this.querySelector('#credentialItemSubtitle');

        rootElement.addEventListener('pointerdown', event => this.onPointerDown(event));
        rootElement.addEventListener('pointerout', () => this.endInteraction());
        rootElement.addEventListener('pointerup', () => this.endInteraction());
        rootElement.addEventListener('click', event => this.onClick(event));

        titleSpan.innerText = this.#title;
        if (this.#subtitle) {
            subtitleSpan.innerText = this.#subtitle;
        }
    }

    async connectedCallback() {
        this.refreshTotpAsync();
        await Animator.fadeInAsync(this.parentElement, 180, 60);
    }

    async refreshTotpAsync() {
        let totpSpan = this.querySelector('#credentialItemTotp');
        this.#totp = await TotpGenerator.calculateTotpAsync(this.#secret);
        totpSpan.innerText = this.#totp
            ? this.#totp.slice(0, 3) + ' ' + this.#totp.slice(3)
            : '!';
    }

    onPointerDown(event) {
        if (event.pointerType != 'touch') {
            this.startInteraction();
        }
    }

    onClick(event) {
        if (event.pointerType == 'touch') {
            this.startInteraction();
            this.endInteraction();
        }
    }

    startInteraction() {
        if (!this.#totp) {
            return;
        }

        if (this.#copyAnimationCtx) {
            this.#copyAnimationCtx.cancel();
        }

        const rootElement = this.querySelector('#credentialItemRoot');
        rootElement.classList.add('active');

        this.#onClick(this.#totp);
    }

    async endInteraction() {
        if (!this.#totp) {
            return;
        }

        if (this.#copyAnimationCtx) {
            this.#copyAnimationCtx.cancel();
        }

        const ctx = new CancellationToken();
        this.#copyAnimationCtx = ctx;

        await Task.sleepAsync(240);
        if (!ctx.cancelled) {
            const rootElement = this.querySelector('#credentialItemRoot');
            rootElement.classList.remove('active');
        }
    }
}

customElements.define('credential-item', CredentialItem);
