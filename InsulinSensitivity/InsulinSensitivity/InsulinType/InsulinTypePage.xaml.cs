using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace InsulinSensitivity.InsulinType
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InsulinTypePage : ContentPage
    {
        public InsulinTypePage() =>
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
    }
}