using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace InsulinSensitivity.MenstrualCycle
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenstrualCyclePage : ContentPage
    {
        public MenstrualCyclePage() =>
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
            if (BindingContext is MenstrualCyclePageViewModel context && context.IsModal)
            {
                context.IsModal = false;
                return true;
            }
            return base.OnBackButtonPressed();
        }
    }
}