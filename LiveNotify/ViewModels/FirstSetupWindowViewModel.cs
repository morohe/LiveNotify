using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Regions;
using System.Reactive.Disposables;
using Reactive.Bindings;
using System.Windows;
using System.Configuration;

namespace LiveNotify.ViewModels
{
    public class FirstSetupWindowViewModel
        : BindableBase
        , IDisposable
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private CompositeDisposable _Disposables = new CompositeDisposable();
        private Models.SettingsStoreDirectoryManager _StorePathManager = new Models.SettingsStoreDirectoryManager();
        public FirstSetupWindowViewModel()
        {
            SettingsStorePath = _StorePathManager.StorePath;
            SettingsStorePathSelect = _StorePathManager.StorePathSelect;

            OkCommand = new ReactiveCommand();
            OkCommand.Subscribe(x =>
            {
                _StorePathManager.Save();
                (x as Window).DialogResult = true;
            });

            CancelCommand = new ReactiveCommand();
            CancelCommand.Subscribe(x =>
            {
                (x as Window).DialogResult = false;
                (x as Window).Close();
            });
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

        public ReactiveProperty<Models.Enums.eSettingsStoreDirectorySelect> SettingsStorePathSelect { get; }

        public ReactiveProperty<string> SettingsStorePath { get; }

        public ReactiveCommand OkCommand { get; }

        public ReactiveCommand CancelCommand { get; }
    }
}
