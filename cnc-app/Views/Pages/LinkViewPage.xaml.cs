using APP.ViewModels.Pages;
using APP.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Abstractions.Controls;

namespace APP.Views.Pages
{
    /// <summary>
    /// LinkViewPage.xaml 的交互逻辑
    /// </summary>
    public partial class LinkViewPage : INavigableView<LinkViewModel>
    {
        public LinkViewModel ViewModel { get; }
        public LinkViewPage(LinkViewModel linkViewModel)
        {
            this.ViewModel = linkViewModel;
            DataContext = this;
            InitializeComponent();
        }

    }
}
