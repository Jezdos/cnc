using APP.Domain;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Handler;
using System.ComponentModel.DataAnnotations;

namespace APP.ViewModels.Pages
{
    public partial class LinkFormViewModel : ViewModelEditor
    {

        [ObservableProperty]
        private bool _IsEdit = true;

        private readonly LinkMqtt Entity;

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        private string? _Name;
        partial void OnNameChanged(string? value) => ValidateProperty(value, nameof(Name));

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        private string? _Host;
        partial void OnHostChanged(string? value) => ValidateProperty(value, nameof(Host));

        [Range(minimum: 1000, maximum: 99999, ErrorMessage = "请输入数字: 1000-99999")]
        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        private int? _Port;
        partial void OnPortChanged(int? value) => ValidateProperty(value, nameof(Port));

        [ObservableProperty]
        private string? _ClientId;

        [ObservableProperty]
        private int? _KeepAlive;

        [ObservableProperty]
        private string? _Username;

        [ObservableProperty]
        public string? _Password;

        [ObservableProperty]
        public LinkModelEnum _Model = LinkModelEnum.AUTO;

        private readonly FormSubmitEventHandler<LinkMqtt> SubmitEvent;

        public LinkFormViewModel(LinkMqtt entity, FormSubmitEventHandler<LinkMqtt> submitEvent, bool autoClose = true) : base(autoClose: autoClose)
        {

            this.Entity = entity;

            if (entity.LinkId.HasValue)
            {
                IsEdit = false;
            }

            this.Name = entity.Name;
            this.Host = entity.Host;
            this.Port = entity.Port;
            this.ClientId = entity.ClientId;
            this.KeepAlive = entity.KeepAlive;
            this.Username = entity.Username;
            this.Password = entity.Password;
            this.Model = entity.Model;

            SubmitEvent = submitEvent;
        }

        [RelayCommand(CanExecute = nameof(IsFree))]
        private async Task Submit() => await ExecuteAsync(async () =>
        {
            ValidateAllProperties();

            if (HasErrors) return false;

            this.Entity.Name = this.Name;
            this.Entity.Host = this.Host;
            this.Entity.Port = this.Port;
            this.Entity.ClientId = this.ClientId;
            this.Entity.KeepAlive = this.KeepAlive;
            this.Entity.Username = this.Username;
            this.Entity.Password = this.Password;
            this.Entity.Model = this.Model;

            return await SubmitEvent.SafeInvoke(this.Entity);
        });





    }
}
