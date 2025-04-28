using APP.Domain;
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
    public partial class LinkViewModel : ViewModel
    {
        private readonly ILog logger = LogManager.GetLogger(nameof(LinkViewModel));
        private readonly IUnitOfWork unitOfWork;
        private readonly IRepository<LinkMqtt> repository;
        private readonly IContentDialogService contentDialogService;
        private readonly FMqttClientManagement clientManagement;

        [ObservableProperty]
        private ObservableCollection<LinkMqttView> _Items = [];

        public LinkViewModel(IUnitOfWork unitOfWork, IContentDialogService contentDialogService, FMqttClientManagement clientManagement)
        {
            this.unitOfWork = unitOfWork;
            this.repository = unitOfWork.GetRepository<LinkMqtt>();
            this.contentDialogService = contentDialogService;
            this.clientManagement = clientManagement;
        }

        [RelayCommand]
        private async Task OpenLinkForm(LinkMqtt? entity)
        {
            LinkMqtt data = new();
            if (entity != null) data = entity;
            LinkFormViewModel editor = new LinkFormViewModel(data, (var) => SubmitEvent(var));
            var form = new LinkFormView(editor);
            await DialogHost.Show(form, AppConstants.DIALOG_ROOT);
        }

        [RelayCommand]
        private async Task RemoveLinkForm(LinkMqtt? entity)
        {
            if (entity is null || !entity.LinkId.HasValue) return;
            ContentDialogResult result = await contentDialogService.ShowSimpleDialogAsync(UIConstant.DEL_CONFIRM);

            if (result == ContentDialogResult.Primary)
            {
                repository.Delete(entity.LinkId);
                await unitOfWork.SaveChangesAsync();

                // remove conmunication link in ClientManagement
                clientManagement.Remove(entity.LinkId);

                await LoadData();
            }
        }


        public async Task LoadData()
        {
            var list = await repository.GetAllAsync();
            Items = new ObservableCollection<LinkMqttView>([.. list.Select(entity => new LinkMqttView(entity))]);

            Task.Run(() =>
            {
                foreach (var item in Items)
                {
                    if (item.LinkId is not null)
                    {
                        bool flag = clientManagement.IsClientConnected(item.LinkId.Value);
                        item.Status = flag ? BaseStatusEnum.NORMAL : BaseStatusEnum.EXCEPTION;
                    }
                }
            });
        }

        public override Task OnNavigatedFromAsync()
        {
            clientManagement.ChangeActionNotice -= (obj, param) => ChangeActionNotice(param.linkId, param.status);
            return Task.CompletedTask;
        }

        public async override Task OnNavigatedToAsync()
        {
            await LoadData();
            clientManagement.ChangeActionNotice += (obj, param) => ChangeActionNotice(param.linkId, param.status);
        }

        private async Task<bool> SubmitEvent(LinkMqtt linkMqtt)
        {
            if (!linkMqtt.LinkId.HasValue)
            {
                linkMqtt.LinkId = SnowflakeIdWorker.Singleton.nextId();
                await repository.InsertAsync(linkMqtt);
            }
            else
            {
                repository.Update(linkMqtt);
            }
            await unitOfWork.SaveChangesAsync();
            repository.ChangeEntityState(linkMqtt, Microsoft.EntityFrameworkCore.EntityState.Detached);

            await LoadData();

            // refresh current conmunication link in ClientManagement

            if (linkMqtt.Model == LinkModelEnum.AUTO)
            {
                await clientManagement.Submit(linkMqtt);
            }
            else
            {
                clientManagement.Remove(linkMqtt.LinkId.Value);
            }

            return true;
        }


        private void ChangeActionNotice(long linkId, ConnectStatus status)
        {
            Task.Run(() => Items.Where(var => var.LinkId == linkId).GetFirstIfPresent(var =>
            {
                var.Status = ConnectStatus.CONNECTED == status ? BaseStatusEnum.NORMAL : BaseStatusEnum.EXCEPTION;
            }));
        }


    }
}
