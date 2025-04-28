using APP.Domain;
using APP.Domain.Enums;
using APP.ViewModels.Pages.DeviceForm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Handler;
using System.ComponentModel.DataAnnotations;

namespace APP.ViewModels.Pages
{
    public partial class DeviceFormViewModel : ViewModelEditor
    {

        [ObservableProperty]
        private bool _IsEdit = true;

        private readonly Device Entity;

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        private string? _Name;
        partial void OnNameChanged(string? value) => ValidateProperty(value, nameof(Name));

        [ObservableProperty]
        public DeviceModelEnum _Model = DeviceModelEnum.AUTO;

        [ObservableProperty]
        public DeviceKindEnum _Kind = DeviceKindEnum.CNC;

        [ObservableProperty]
        public IDeviceFormView? _DynamicForm;

        private readonly FormSubmitEventHandler<Device> SubmitEvent;

        public DeviceFormViewModel(Device entity, FormSubmitEventHandler<Device> submitEvent, bool autoClose = true) : base(autoClose: autoClose)
        {

            this.Entity = entity;

            if (entity.DeviceId.HasValue)
            {
                IsEdit = false;
            }

            this.Name = entity.Name;
            this.Model = entity.Model;
            this.Kind = entity.Kind;

            SubmitEvent = submitEvent;

            PropertyChanged += (s, e) => { if (e.PropertyName == nameof(Kind)) UpdateDynamicForm(); };

            UpdateDynamicForm();
        }

        [RelayCommand(CanExecute = nameof(IsFree))]
        private async Task Submit() => await ExecuteAsync(async () =>
        {
            ValidateAllProperties();

            if (HasErrors) return false;

            this.Entity.Name = this.Name;
            this.Entity.Model = this.Model;
            this.Entity.Kind = this.Kind;
            this.Entity.Params = DynamicForm is not null ? DynamicForm.ReadParams() : "";
            return await SubmitEvent.SafeInvoke(this.Entity);
        });

        private void UpdateDynamicForm()
        {
            IDeviceFormViewModel? formModel = (IDeviceFormViewModel?)Activator.CreateInstance(Kind.Descript().Model);
            if (formModel is not null) formModel.AutoFill(this.Entity);
            var formView = (IDeviceFormView?)Activator.CreateInstance(Kind.Descript().View, formModel);
            DynamicForm = formView;
        }

    }
}
