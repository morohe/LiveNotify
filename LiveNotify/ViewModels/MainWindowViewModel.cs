using Autofac;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace LiveNotify.ViewModels
{
    public class MainWindowViewModel
        : BindableBase
        , IDisposable
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private CompositeDisposable _Disposables = new CompositeDisposable();

        public MainWindowViewModel(IRegionManager regionManager, IContainer container)
        {
            ContentRegion = regionManager;
            
            var manager = container.Resolve<Models.AlertManager>();
            List<Models.ViewItem> ViewItems = new List<Models.ViewItem>();

            // Add views
            foreach(var i in manager.AlertNames)
            {
                ViewItems.Add(new Models.ViewItem(
                    manager.GetAlertLocalizedName(i)
                    , nameof(Views.LiveListView)
                    , new NavigationParameters($"view={i}")));
            }

            ViewItems.Add(new Models.ViewItem("Setup", nameof(Views.SetupView), null));
            ViewItems.Add(new Models.ViewItem("About", nameof(Views.AboutView), null));

            Contents = ViewItems.ToArray();

            SelectedContent = new ReactiveProperty<Models.ViewItem>();
            SelectedContent.Subscribe(x =>
            {
                if ((null != x) && !string.IsNullOrEmpty(x.ViewName))
                {
                    ContentRegion.RequestNavigate(ContentRegionName, x.ViewName, x.Parameters);
                }
            });

            FirstViewCommand = new ReactiveCommand();
            FirstViewCommand.Subscribe(() => SelectedContent.Value = Contents[0]);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _logger.Info("Cleanup");
                    // Cleanup region resource
                    List<string> RegionNames = new List<string>();
                    foreach (IRegion i in ContentRegion.Regions)
                    {
                        i.RemoveAll();
                        RegionNames.Add(i.Name);
                    }
                    // Remove regions
                    foreach(string i in RegionNames)
                    {
                        ContentRegion.Regions.Remove(i);
                    }
                    _Disposables.Dispose();
                    _logger.Info("Cleanup Complete");
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public IRegionManager ContentRegion { get; }

        public static string ContentRegionName { get { return "ContentRegion"; } }

        public Models.ViewItem[] Contents { get; }

        public ReactiveProperty<Models.ViewItem> SelectedContent { get; set; }

        public ReactiveCommand FirstViewCommand { get; }
    }
}
