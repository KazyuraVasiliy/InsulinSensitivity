using Xamarin.Forms;

namespace BusinessLogicLayer.ViewModel
{
    public static class Tag
    {
        public static readonly BindableProperty TagProperty = 
            BindableProperty.Create("Tag", typeof(object), typeof(Tag), null);

        public static object GetTag(BindableObject bindable) =>
            bindable.GetValue(TagProperty);

        public static void SetTag(BindableObject bindable, string value) =>
            bindable.SetValue(TagProperty, value);
    }
}