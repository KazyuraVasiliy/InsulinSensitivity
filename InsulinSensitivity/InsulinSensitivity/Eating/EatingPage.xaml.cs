﻿using BusinessLogicLayer.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace InsulinSensitivity.Eating
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EatingPage : ContentPage
    {
        public EatingPage() =>
            InitializeComponent();

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Send<object, bool>(this, "SwitchMaster", false);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Send<object, bool>(this, "SwitchMaster", true);
        }

        protected override bool OnBackButtonPressed()
        {
            if (BindingContext is EatingPageViewModel context)
            {
                if (context.IsModalInjection || context.IsModalDimension || context.IsModalSnack)
                {
                    context.IsModalInjection = context.IsModalDimension = context.IsModalSnack = false;
                    return true;
                }
            }
            return base.OnBackButtonPressed();
        }

        private void Element_Unfocused(object sender, FocusEventArgs e)
        {
            if (sender is BindableObject bindable)
            {
                var tag = Tag.GetTag(bindable);
                if (bindable.BindingContext is ObservableBase observable && tag is string property)
                    observable.OnPropertyChanged(property);
            }
        }
    }
}