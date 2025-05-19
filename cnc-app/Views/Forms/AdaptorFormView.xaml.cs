using APP.Domain;
using APP.ViewModels.Pages;
using System.Windows.Controls;

namespace APP.Views.Forms
{
    public partial class AdaptorFormView : UserControl
    {
        public AdaptorFormViewModel ViewModel { get; }

        public AdaptorFormView(AdaptorFormViewModel ViewModel)
        {
            this.ViewModel = ViewModel;
            this.DataContext = this;
            InitializeComponent();

            DeviceCombo.SelectedIndex = ViewModel.DeviceList.Select(e => e.DeviceId).ToList().IndexOf(ViewModel.DeviceId);
            LinkCombo.SelectedIndex = ViewModel.LinkList.Select(e => e.LinkId).ToList().IndexOf(ViewModel.LinkId);
        }

        private void LinkCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;
            var selected = comboBox.SelectedItem as LinkMqtt;
            if (selected == null)
            {
                ViewModel.LinkId = null;
                return;
            }
            ViewModel.LinkId = selected.LinkId;
        }

        private void DeviceCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;
            var selected = comboBox.SelectedItem as Device;
            if (selected == null)
            {
                ViewModel.DeviceId = null;
                return;
            }
            ViewModel.DeviceId = selected.DeviceId;
        }
    }
}
