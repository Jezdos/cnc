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
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore;
using log4net;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using transport_common;
using UI;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

namespace APP.ViewModels.Pages
{
    public partial class HomeViewModel : ViewModel
    {
        private readonly ILog logger = LogManager.GetLogger(nameof(HomeViewModel));
        private readonly IUnitOfWork unitOfWork;
        private readonly IRepository<LinkMqtt> repository;

        public IEnumerable<ISeries> Series { get; set; } =
        GaugeGenerator.BuildSolidGauge(
            new GaugeItem(
                30,          // the gauge value
                series =>    // the series style
                {
                    series.MaxRadialColumnWidth = 50;
                    series.DataLabelsSize = 50;
                }));


        public ISeries[] Graph { get; set; } = [
        new ColumnSeries<double?>
        {
            Values = [5, 4, null, 3, 2, 6, 5, 6, 2]
        },
        new LineSeries<double?>
        {
            Values = [2, 6, 5, 3, null, 5, 2, 4, null]
        },
        new LineSeries<ObservablePoint?>
        {
            Values = [
                new ObservablePoint { X = 0, Y = 1 },
                new ObservablePoint { X = 1, Y = 4 },
                null,
                new ObservablePoint { X = 4, Y = 5 },
                new ObservablePoint { X = 6, Y = 1 },
                new ObservablePoint { X = 8, Y = 6 },
            ]
        }
    ];

        public HomeViewModel(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            this.repository = unitOfWork.GetRepository<LinkMqtt>();
        }


        public async Task LoadData()
        {
            
        }

        public override Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }

        public async override Task OnNavigatedToAsync()
        {
            await LoadData();
        }

    }
}
