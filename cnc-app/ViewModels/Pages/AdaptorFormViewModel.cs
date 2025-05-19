using APP.Domain;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Handler;
using System.ComponentModel.DataAnnotations;

namespace APP.ViewModels.Pages
{
    public partial class AdaptorFormViewModel : ViewModelEditor
    {

        [ObservableProperty]
        private bool _IsEdit = true;

        [ObservableProperty]
        private IList<LinkMqtt> _LinkList = [];

        [ObservableProperty]
        private IList<Device> _DeviceList = [];

        private readonly Adaptor Entity;

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        private string? _Name;
        partial void OnNameChanged(string? value) => ValidateProperty(value, nameof(Name));

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        private long? _DeviceId;

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        private long? _LinkId;

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        private string? _TopicTelemetry;
        partial void OnTopicTelemetryChanged(string? value) => ValidateProperty(value, nameof(TopicTelemetry));

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        private string? _TopicRpc;
        partial void OnTopicRpcChanged(string? value) => ValidateProperty(value, nameof(TopicRpc));

        private readonly FormSubmitEventHandler<Adaptor> SubmitEvent;

        public AdaptorFormViewModel(Adaptor entity, IList<LinkMqtt> linkList, IList<Device> deviceList, FormSubmitEventHandler<Adaptor> submitEvent, bool autoClose = true) : base(autoClose: autoClose)
        {
            this.Entity = entity;

            if (entity.DeviceId.HasValue)
            {
                IsEdit = false;
            }

            this.LinkList = linkList;
            this.DeviceList = deviceList;

            this.Name = entity.Name;
            this.DeviceId = entity.DeviceId;
            this.LinkId = entity.LinkId;
            this.TopicTelemetry = entity.TopicTelemetry;
            this.TopicRpc = entity.TopicRpc;

            SubmitEvent = submitEvent;
        }

        [RelayCommand(CanExecute = nameof(IsFree))]
        private async Task Submit() => await ExecuteAsync(async () =>
        {
            ValidateAllProperties();

            if (HasErrors) return false;

            this.Entity.Name = this.Name;
            this.Entity.DeviceId = this.DeviceId;
            this.Entity.LinkId = this.LinkId;
            this.Entity.TopicTelemetry = this.TopicTelemetry;
            this.Entity.TopicRpc = this.TopicRpc;

            return await SubmitEvent.SafeInvoke(this.Entity);
        });
    }
}
