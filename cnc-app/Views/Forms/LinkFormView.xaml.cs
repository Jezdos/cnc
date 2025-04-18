using APP.Domain;
using APP.ViewModels.Pages;
using System.Windows.Controls;

namespace APP.Views.Forms
{
    public partial class LinkFormView : UserControl
    {
        public LinkFormViewModel ViewModel { get; }

        public LinkFormView(LinkFormViewModel ViewModel)
        {
            this.ViewModel = ViewModel;
            this.DataContext = this;
            InitializeComponent();

            ModelCombo.ItemsSource = Enum.GetValues(typeof(LinkModelEnum)).Cast<LinkModelEnum>();
            ModelCombo.SelectedIndex = (int)ViewModel.Model;

        }

        private void ModelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;
            ComboBoxItem? selectedItem = comboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;
            var selectedParent = selectedItem.Content as string;
            if (selectedParent != null)
            {
                ViewModel.Model = (LinkModelEnum)Enum.Parse(typeof(LinkModelEnum), selectedParent);
            }
        }
    }
}
