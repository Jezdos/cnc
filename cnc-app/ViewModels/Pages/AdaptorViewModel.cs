using APP.Domain;
using APP.Domain.Views;
using APP.Services;
using APP.Views.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Extensions;
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
    public partial class AdaptorViewModel : ViewModel
    {
        private readonly ILog logger = LogManager.GetLogger(nameof(AdaptorViewModel));
        private readonly IUnitOfWork unitOfWork;

        private readonly IRepository<Adaptor> repository;
        private readonly IContentDialogService contentDialogService;

        private readonly IList<Device> devices;
        private readonly IList<LinkMqtt> linkMqtts;

        private readonly AdaptorManagement adaptorManagement;

        [ObservableProperty]
        private ObservableCollection<AdaptorView> _Items = [];

        public AdaptorViewModel(IUnitOfWork unitOfWork, IContentDialogService contentDialogService, AdaptorManagement adaptorManagement)
        {
            this.unitOfWork = unitOfWork;
            this.repository = unitOfWork.GetRepository<Adaptor>();
            this.contentDialogService = contentDialogService;
            this.adaptorManagement = adaptorManagement;

            this.devices = unitOfWork.GetRepository<Device>().GetAll().ToList();
            this.linkMqtts = unitOfWork.GetRepository<LinkMqtt>().GetAll().ToList();
        }

        [RelayCommand]
        private async Task OpenLinkForm(Adaptor? entity)
        {
            Adaptor data = new();
            if (entity != null) data = entity;
            AdaptorFormViewModel editor = new AdaptorFormViewModel(data, linkMqtts, devices, (var) => SubmitEvent(var));
            var form = new AdaptorFormView(editor);
            await DialogHost.Show(form, AppConstants.DIALOG_ROOT);
        }

        [RelayCommand]
        private async Task RemoveLinkForm(Adaptor? entity)
        {
            if (entity is null || !entity.AdaptorId.HasValue) return;
            ContentDialogResult result = await contentDialogService.ShowSimpleDialogAsync(UIConstant.DEL_CONFIRM);

            if (result == ContentDialogResult.Primary)
            {
                repository.Delete(entity.AdaptorId);
                await unitOfWork.SaveChangesAsync();
                await LoadData();
            }
        }


        public async Task LoadData()
        {
            var adaptorList = await repository.GetAllAsync();

            Items = new ObservableCollection<AdaptorView>([.. adaptorList.Select(entity => {
                AdaptorView view = new AdaptorView(entity);
                devices.Where(d => d.DeviceId == entity.DeviceId).GetFirstIfPresent(d => {
                    view.DeviceName = d.Name;
                });

                linkMqtts.Where(l => l.LinkId == entity.LinkId).GetFirstIfPresent(l => {
                    view.LinkName = l.Name;
                });
                return view;
            })]);
        }

        public override Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }

        public async override Task OnNavigatedToAsync()
        {
            await LoadData();
        }

        private async Task<bool> SubmitEvent(Adaptor adaptor)
        {
            if (!adaptor.AdaptorId.HasValue)
            {
                adaptor.AdaptorId = SnowflakeIdWorker.Singleton.nextId();
                await repository.InsertAsync(adaptor);
            }
            else
            {
                repository.Update(adaptor);
            }
            await unitOfWork.SaveChangesAsync();
            repository.ChangeEntityState(adaptor, Microsoft.EntityFrameworkCore.EntityState.Detached);
            await adaptorManagement.Reload(true);
            await LoadData();
            return true;
        }
    }
}
