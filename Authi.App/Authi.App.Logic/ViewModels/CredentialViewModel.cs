using Authi.App.Logic.Data;
using Authi.Common.Extensions;
using System;

namespace Authi.App.Logic.ViewModels
{
    public class CredentialViewModel : ViewModelBase
    {
        public string Title
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }

        public string? Subtitle
        {
            get => Get<string>();
            set => Set(value);
        }

        public string Secret
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }

        public string? Totp
        {
            get => Get<string>();
            internal set
            {
                Set(value);
                OnPropertyChanged(nameof(DisplayTotp));
            }
        }

        public string DisplayTotp => !string.IsNullOrEmpty(Totp)
            ? string.Concat(Totp.AsSpan(0, 3), " ", Totp.AsSpan(3))
            : "!";

        public bool IsEditing
        {
            get => Get<bool>();
            set => Set(value);
        }

        internal Credential Model { get; private set; }

        public CredentialViewModel(Credential dto)
        {
            Model = dto;
            UpdateModel(dto);
        }

        public void CopyToClipboard()
        {
            if (IsEditing)
            {
                IsEditing = false;
                return;
            }

            if (string.IsNullOrEmpty(Totp))
            {
                return;
            }

            Services.Clipboard.SetTextAsync(Totp);
            Services.Messenger.Copied.Publish(this);
        }

        public void Edit()
        {
            IsEditing = false;
            Services.Messenger.NavigationPush.Publish(this, new EditCredentialViewModel(this));
        }

        public void Delete()
        {
            IsEditing = false;
            Services.Messenger.DeleteCredential.Publish(this, this);
        }

        internal void UpdateModel(Credential dto)
        {
            Model = dto;
            Model.MapPropertiesTo(this);
            Totp = CalculateTotp();
        }

        internal string? CalculateTotp()
        {
            if (string.IsNullOrEmpty(Secret))
            {
                return null;
            }
            Services.TotpGenerator.TryCalculateTotp(Secret, out var totp);
            return totp;
        }
    }
}
