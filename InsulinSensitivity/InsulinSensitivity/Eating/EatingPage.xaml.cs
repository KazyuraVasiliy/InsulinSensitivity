using BusinessLogicLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                var tag = Tag.GetTag(entry);
                if (entry.BindingContext is ObservableBase observable && tag is string property)
                    observable.OnPropertyChanged(property);
            }
        }
    }
}