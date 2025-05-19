using APP.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace APP.Views.Pages
{
    public partial class AdaptorViewPage : INavigableView<AdaptorViewModel>
    {
        public AdaptorViewModel ViewModel { get; }

        public AdaptorViewPage(AdaptorViewModel viewModel)
        {
            this.ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

    }
}
