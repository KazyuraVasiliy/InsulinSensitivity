using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace InsulinSensitivity
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPageMaster : ContentPage
    {
        public MainPageMaster() =>
            InitializeComponent();

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is MainPageMasterItemModel item)
                item.Command.Execute(null);
        }
    }
}