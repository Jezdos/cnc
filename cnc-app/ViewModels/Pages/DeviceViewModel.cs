using APP.Domain;
using APP.Domain.Views;
using APP.Views.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Utils;
using Data.UnitOfWork;
using log4net;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using UI;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace APP.ViewModels.Pages
{
    public partial class DeviceViewModel : ViewModel
    {
        private readonly ILog logger = LogManager.GetLogger(nameof(DeviceViewModel));
        private readonly IUnitOfWork unitOfWork;
        private readonly IRepository<Device> repository;
        private readonly IContentDialogService contentDialogService;

        [ObservableProperty]
        private ObservableCollection<DeviceView> _Items = [];

        public DeviceViewModel(IUnitOfWork unitOfWork, IContentDialogService contentDialogService)
        {
            this.unitOfWork = unitOfWork;
            this.repository = unitOfWork.GetRepository<Device>();
            this.contentDialogService = contentDialogService;
        }

        [RelayCommand]
        private async Task OpenLinkForm(Device? entity)
        {
            Device data = new();
            if (entity != null) data = entity;
            DeviceFormViewModel editor = new DeviceFormViewModel(data, (var) => SubmitEvent(var));
            var form = new DeviceFormView(editor);
            await DialogHost.Show(form, AppConstants.DIALOG_ROOT);
        }

        [RelayCommand]
        private async Task RemoveLinkForm(Device? entity)
        {
            if (entity is null || !entity.DeviceId.HasValue) return;
            ContentDialogResult result = await contentDialogService.ShowSimpleDialogAsync(UIConstant.DEL_CONFIRM);

            if (result == ContentDialogResult.Primary)
            {
                repository.Delete(entity.DeviceId);
                await unitOfWork.SaveChangesAsync();

                await LoadData();
            }
        }


        public async Task LoadData()
        {
            var list = await repository.GetAllAsync();
            Items = new ObservableCollection<DeviceView>([.. list.Select(entity => new DeviceView(entity))]);
        }

        public override Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }

        public async override Task OnNavigatedToAsync()
        {
            await LoadData();
        }

        private async Task<bool> SubmitEvent(Device entity)
        {
            if (!entity.DeviceId.HasValue)
            {
                entity.DeviceId = SnowflakeIdWorker.Singleton.nextId();
                await repository.InsertAsync(entity);
            }
            else
            {
                repository.Update(entity);
            }
            await unitOfWork.SaveChangesAsync();
            repository.ChangeEntityState(entity, Microsoft.EntityFrameworkCore.EntityState.Detached);

            await LoadData();

            return true;
        }
    }
}
