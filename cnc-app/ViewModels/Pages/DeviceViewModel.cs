using APP.Domain;
using APP.Domain.Enums;
using APP.Domain.Views;
using APP.Services;
using APP.Views.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Enums;
using Core.Extensions;
using Core.Utils;
using Data.UnitOfWork;
using log4net;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using transport_common;
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
        private readonly DeviceClientManagement deviceManagement;

        [ObservableProperty]
        private ObservableCollection<DeviceView> _Items = [];

        public DeviceViewModel(IUnitOfWork unitOfWork, IContentDialogService contentDialogService, DeviceClientManagement deviceManagement)
        {
            this.unitOfWork = unitOfWork;
            this.repository = unitOfWork.GetRepository<Device>();
            this.contentDialogService = contentDialogService;
            this.deviceManagement = deviceManagement;
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

                // remove conmunication link in deviceManagement
                deviceManagement.Remove(entity.DeviceId);

                await LoadData();
            }
        }


        public async Task LoadData()
        {
            var list = await repository.GetAllAsync();
            Items = new ObservableCollection<DeviceView>([.. list.Select(entity => new DeviceView(entity))]);

            Task.Run(() =>
            {
                foreach (var item in Items)
                {
                    if (item.DeviceId is not null)
                    {
                        bool flag = deviceManagement.IsClientConnected(item.DeviceId.Value);
                        item.Status = flag ? BaseStatusEnum.NORMAL : BaseStatusEnum.EXCEPTION;
                    }
                }
            });
        }

        public override Task OnNavigatedFromAsync()
        {
            deviceManagement.ChangeActionNotice -= (obj, param) => ChangeActionNotice(param.deviceId, param.status);
            return Task.CompletedTask;
        }

        public async override Task OnNavigatedToAsync()
        {
            await LoadData();
            deviceManagement.ChangeActionNotice += (obj, param) => ChangeActionNotice(param.deviceId, param.status);
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

            // refresh current conmunication link in deviceManagement

            if (entity.Model == DeviceModelEnum.AUTO)
            {
                await deviceManagement.Submit(entity);
            }
            else
            {
                deviceManagement.Remove(entity.DeviceId);
            }

            return true;
        }

        private void ChangeActionNotice(long deviceId, ConnectStatus status)
        {
            Task.Run(() => Items.Where(var => var.DeviceId == deviceId).GetFirstIfPresent(var =>
            {
                var.Status = ConnectStatus.CONNECTED == status ? BaseStatusEnum.NORMAL : BaseStatusEnum.EXCEPTION;
            }));
        }
    }
}
