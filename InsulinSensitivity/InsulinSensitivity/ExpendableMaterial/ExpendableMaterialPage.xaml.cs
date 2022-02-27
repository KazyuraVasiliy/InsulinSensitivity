using BusinessLogicLayer.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace InsulinSensitivity.ExpendableMaterial
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExpendableMaterialPage : ContentPage
    {
        public ExpendableMaterialPage() =>
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
            if (sender is BindableObject bindable)
            {
                var tag = Tag.GetTag(bindable);
                if (bindable.BindingContext is ObservableBase observable && tag is string property)
                    observable.OnPropertyChanged(property);
            }
        }
    }
}