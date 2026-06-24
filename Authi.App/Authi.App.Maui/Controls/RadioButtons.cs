using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace Authi.App.Maui.Controls
{
    public partial class RadioButtons : VerticalStackLayout
    {
        private bool _isUpdatingSelection;

        public static readonly BindableProperty HeaderProperty =
            BindableProperty.Create(nameof(Header), typeof(string), typeof(RadioButtons), propertyChanged: OnHeaderChanged);

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(RadioButtons), propertyChanged: OnItemsSourceChanged);

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(RadioButtons), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private static void OnHeaderChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((RadioButtons)bindable).RebuildUI();
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (RadioButtons)bindable;

            if (oldValue is INotifyCollectionChanged oldCollection)
                oldCollection.CollectionChanged -= control.OnItemsSourceCollectionChanged;

            if (newValue is INotifyCollectionChanged newCollection)
                newCollection.CollectionChanged += control.OnItemsSourceCollectionChanged;

            control.RebuildUI();
        }

        private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildUI();
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (RadioButtons)bindable;
            control.UpdateCheckedStates();
        }

        private void RebuildUI()
        {
            Children.Clear();

            if (!string.IsNullOrWhiteSpace(Header))
            {
                Children.Add(new Label
                {
                    Text = Header,
                    Margin = new Thickness(0, 0, 0, 4),
                    Style = AuthiApp.Current.GetResource<Style>("BodyLabelStyle")
                });
            }

            if (ItemsSource == null) return;

            var groupName = Guid.NewGuid().ToString();

            foreach (var item in ItemsSource)
            {
                var radioButton = new RadioButton
                {
                    Content = item,
                    GroupName = groupName,
                    Value = item
                };

                radioButton.CheckedChanged += OnRadioButtonCheckedChanged;

                Children.Add(radioButton);
            }

            UpdateCheckedStates();
        }

        private void OnRadioButtonCheckedChanged(object? sender, CheckedChangedEventArgs e)
        {
            if (_isUpdatingSelection || !e.Value) return;

            if (sender is RadioButton radioButton)
            {
                _isUpdatingSelection = true;
                SelectedItem = radioButton.Value;
                _isUpdatingSelection = false;
            }
        }

        private void UpdateCheckedStates()
        {
            if (_isUpdatingSelection) return;

            _isUpdatingSelection = true;

            foreach (var child in Children)
            {
                if (child is RadioButton radioButton)
                {
                    radioButton.IsChecked = Equals(radioButton.Value, SelectedItem);
                }
            }

            _isUpdatingSelection = false;
        }
    }
}
