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

namespace Uvon.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyMenuMaster : ContentPage
    {
        public ListView ListView;

        public MyMenuMaster()
        {
            InitializeComponent();

            BindingContext = new MyMenuMasterViewModel();
            ListView = MenuItemsListView;
        }

        class MyMenuMasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<MyMenuMasterMenuItem> MenuItems { get; set; }

            public MyMenuMasterViewModel()
            {
                MenuItems = new ObservableCollection<MyMenuMasterMenuItem>(new[]
                {
                    new MyMenuMasterMenuItem { Id = 0, Title = "Connect", TargetType = typeof(GetConnect) },
                    new MyMenuMasterMenuItem { Id = 1, Title = "My List", TargetType = typeof(MyList) },
                    new MyMenuMasterMenuItem { Id = 2, Title = "About", TargetType = typeof(About) },
                });
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}