# authi.me: 2fa totp with sync

[![App Tests](https://github.com/cherepets/Authi-OSS/actions/workflows/test-app.yml/badge.svg)](https://github.com/cherepets/Authi-OSS/actions/workflows/test-app.yml)
[![Server Tests](https://github.com/cherepets/Authi-OSS/actions/workflows/test-server.yml/badge.svg)](https://github.com/cherepets/Authi-OSS/actions/workflows/test-server.yml)
[![Common Tests](https://github.com/cherepets/Authi-OSS/actions/workflows/test-common.yml/badge.svg)](https://github.com/cherepets/Authi-OSS/actions/workflows/test-common.yml)

**authi.me** is a modern selfhostable open source two-factor authentication (2FA) application that generates secure TOTP (Time-based One-Time Password) codes and syncs them across your devices through the cloud – no accounts required.

<img width="1399" height="892" alt="image" src="https://cherepets.me/images/authi_screenshot.webp" />

## Guides

* [Selfhosting guide](https://github.com/cherepets/Authi-OSS/wiki/Selfhosting-guide)

## Sync design

Sync is accountless, user doesn't have to sign up to use sync.

User has to upload data from one device, then download to another devices and from that point all devices are staying in sync.

### Init

Uploading data from the first device is called **Init**.

During **Init** the client generates a key pair to communicate with server and an aes key to encrypt data locally.

Public communication key is sent to the server and data key is never sent to the server.

Server also generates a key pair (unique per client, so same user on different devices has different ids and different keys), it stores it's own private key and client's private key and shares it's public key with client.

### Publish

To initiate sync on another device, an already synced device has to initiate a sync session, this operation is called **Publish**.

During **Publish** the first client generates a one time key pair, the server also generates a key pair and the exchange keys similar to how it happens during **Init**.

After sync session is initiated with **Publish**, it can be used one time only and only for 3 minutes since it was created.

The first client creates a qr-code (or text code in case user can't scan a qr on the target device) that has: sync session id, the local data key and one time session key (already exchanged version).

### Consume

Connecting a new client using an active sync session is called **Consume**.

The second client scans a qr-code (or obtains text code in any way the user prefers), generates it's new communication key pair and sends it's public part and sync id to the server.

The server again generates a key pair and the exchange keys similar to how it happens during **Init**.

                Init {comm key 1}             Consume {comm key 2}
    Client 1  <------------------->  Server  <-------------------->  Client 2
       |      Publish {one time key}                                    ˄
       |                                                                |
       +--------------- QR Code {one time key, data key} ---------------+


## Projects

| Project | Description |
|---------|-------------|
| **Authi.App.Logic** | Mobile and desktop application logic, all view models, stored data types, services. |
| **Authi.App.Maui** | Android mobile app, views, controls, styles. Can be built for iOS as well but it's not tested. |
| **Authi.App.WinUI** | Desktop Windows app, views, controls, styles. Not using MAUI as controls for Windows in MAUI are not yet good enough. |
| **Authi.App.Test** | Tests covering app view models and services. |
| **Authi.Common.Client** | Shared app client code (used by mobile and desktop apps, browser extensions), mostly an API client. |
| **Authi.Common.Dto** | DTOs used by both client and server, requests and responses. |
| **Authi.Common** | Shared code used by all projects, common extensions, serializers, cryptography. |
| **Authi.Common.Test** | Tests covering shared code, serializers, cryptography. |
| **Authi.Server** | Server code, all APIs and data access. |
| **Authi.Server.Test** | Tests covering server code, APIs. |
| **Authi.BrowserExtension** | Browser extension, separate manifests for Chrome and Firefox due to API differences. |
| **Authi.Wasm** | WASM wrapper for client code so it can be used from browser extensions. |
| **Authi.BrowserPack** | Creates packages for browser extensions. |
| **Authi.Localizer** | Creates a .NET wrapper for a JS based localization file. |

## Built with

* [BouncyCastle.Cryptography](https://github.com/bcgit/bc-csharp) - The Bouncy Castle Cryptography Library For .NET.
* [Camera.MAUI](https://github.com/hjam40/Camera.MAUI) - A Camera View control and a Barcode Endode/Decode control for .NET MAUI.
* [CommunityToolkit.Maui](https://github.com/CommunityToolkit/Maui) - .NET MAUI Community Toolkit.
* [LiteDB](https://github.com/litedb-org/LiteDB) - A .NET NoSQL Document Store in a single data file.
* [MaterialColorUtilities.Maui](https://github.com/albi005/MaterialColorUtilities) - C# implementation of Google's Material color utilities.
* [Otp.NET](https://github.com/kspearrin/Otp.NET) - An implementation TOTP RFC 6238 and HOTP RFC 4226 in C#.
* [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql) - Entity Framework Core provider for MySQL compatible databases.
* [SharpHook](https://github.com/TolikPylypchuk/SharpHook) - Global keyboard and mouse hook, event simulation, and text entry simulation for .NET.
* [SkiaSharp](https://github.com/mono/SkiaSharp) - SkiaSharp for .NET MAUI is a set of views that can be used to draw on the screen.
* [WinUI.SingleInstanced](https://github.com/cherepets/WinUI.SingleInstanced) - Zero-setup single instancing for WinUI 3 applications.
* [WinUIEx](https://github.com/dotMorten/WinUIEx) - A set of extension methods and classes to fill some gaps in WinUI 3, mostly around windowing.
* [ZXing.Net](https://github.com/micjahn/ZXing.Net/) - ZXing.Net Bindings for Windows Compatibility Pack.
