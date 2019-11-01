﻿namespace Thinning.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using Caliburn.Micro;
    using Thinning.Infrastructure;
    using Thinning.Infrastructure.Interfaces;
    using Thinning.Infrastructure.Models;
    using Thinning.UI.Helpers;
    using Thinning.UI.Helpers.Interfaces;

    public class MainWindowViewModel : Conductor<IScreen>.Collection.AllActive
    {
        private ICardContent cardContent;

        private IApplicationSetup applicationSetup;

        private IWindowManager windowManager;

        public MainWindowViewModel(
            ICardContent cardContent,
            IApplicationSetup applicationSetup,
            IWindowManager windowManager)
        {
            this.cardContent = cardContent;
            this.applicationSetup = applicationSetup;
            this.windowManager = windowManager;

            this.SetTabsForPerformanceCharts();

            this.HardwareInfo = this.cardContent.GetHardwareInfo();
            this.NotifyOfPropertyChange(() => this.HardwareInfo);
        }

        public string BaseImageUrl { get; set; }

        public ObservableCollection<Tuple<string, ImageSource>> Images { get; set; } = new ObservableCollection<Tuple<string, ImageSource>>();

        public bool IsButtonsEnabled { get; set; } = true;

        public string HardwareInfo { get; set; }

        public string ImageInfo { get; set; }

        public void LoadImage()
        {
            var fileDialog = new FileDialog();

            this.BaseImageUrl = fileDialog.GetImageFilepath();
            this.NotifyOfPropertyChange(() => this.BaseImageUrl);

            this.ImageInfo = this.cardContent.GetImageInfo(this.BaseImageUrl);
            this.NotifyOfPropertyChange(() => this.ImageInfo);
        }

        public async void RunAlgorithms()
        {
            this.IsButtonsEnabled = false;
            this.NotifyOfPropertyChange(() => this.IsButtonsEnabled);

            var testResult = await this.ExecuteTests();

            if (testResult != null)
            {
                this.AttachResultsToAlgorithms(testResult);
            }

            this.IsButtonsEnabled = true;
            this.NotifyOfPropertyChange(() => this.IsButtonsEnabled);
        }

        private void SetTabsForPerformanceCharts()
        {
            var algorithmNames = this.applicationSetup.GetRegisteredAlgorithmNames();
            foreach (var algorithm in algorithmNames)
            {
                this.Items.Add(new PerformanceChartViewModel { DisplayName = algorithm });
            }
        }

        private async Task<TestResult> ExecuteTests()
        {
            var progressViewModel = new ProgressViewModel();
            await this.windowManager.ShowWindowAsync(progressViewModel, null, null);

            var algorithmTest = new AlgorithmTest();
            return await algorithmTest.ExecuteAsync(this.BaseImageUrl, progressViewModel);
        }

        private void AttachResultsToAlgorithms(TestResult testResult)
        {
            var conversion = new ImageConversion();
            int algorithmCount = 0;

            foreach (var timesList in testResult.ResultTimes)
            {
                this.Items[algorithmCount] = new PerformanceChartViewModel(timesList, this.Items[algorithmCount].DisplayName);
                this.Images.Add(Tuple.Create(
                    this.Items[algorithmCount].DisplayName,
                    (ImageSource)conversion.BitmapToBitmapImage(testResult.ResultBitmaps[algorithmCount])));

                algorithmCount++;
            }

            this.NotifyOfPropertyChange(() => this.Images);
            this.NotifyOfPropertyChange(() => this.Items);
        }
    }
}
