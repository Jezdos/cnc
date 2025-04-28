using Core.Enums;
using System.ComponentModel;

namespace APP.Domain.Views
{
    public partial class LinkMqttView(LinkMqtt entity) : LinkMqtt(entity), INotifyPropertyChanged
    {
        public BaseStatusEnum _status { get; set; } = BaseStatusEnum.EXCEPTION;

        public BaseStatusEnum Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
