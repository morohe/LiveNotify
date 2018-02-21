using LicenseViewer.Models;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Windows.Navigation;
using System.Diagnostics;

namespace LiveNotify.ViewModels
{
    public class AboutViewModel
        : BindableBase
        , IDisposable
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private const string LicenseDirectory = "Licenses";

        private CompositeDisposable _Disposables = new CompositeDisposable();

        public AboutViewModel(IRegionManager regionManger)
        {
            LicenseRegion = regionManger;
            
            // Get licenses
            List<License> lic = new List<License>();
            try
            {
                foreach (string f in System.IO.Directory.GetFiles(
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location)
                    + System.IO.Path.DirectorySeparatorChar
                    + LicenseDirectory, "*.xml"))
                {
                    lic.Add(License.ImportFromXmlFile(f));
                }
            }
            catch
            {

            }
            Licenses = lic.ToArray();

            SelectedLicense = new ReactiveProperty<License>();
            SelectedLicense.Subscribe(x =>
            {
                NavigationParameters param = new NavigationParameters();

                if (null != SelectedLicense.Value)
                {
                    param.Add(nameof(License), SelectedLicense.Value);
                    param.Add("LicenseDirectory"
                        , System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location)
                        + System.IO.Path.DirectorySeparatorChar
                        + LicenseDirectory);
                    LicenseRegion.RequestNavigate(LicenseRegionName, nameof(LicenseViewer.Views.LicenseView), param);
                }
            });

            FirstViewCommand = new ReactiveCommand();
            FirstViewCommand.Subscribe(() =>
            {
                if ((null != Licenses) && (0 < Licenses.Length))
                {
                    SelectedLicense.Value = Licenses[0];
                }
            });

            NavigateWeb = new ReactiveCommand();
            NavigateWeb.Subscribe(x =>
            {
                if (x?.GetType() == typeof(Uri))
                {
                    Process.Start(new ProcessStartInfo((x as Uri).ToString()) { UseShellExecute = true });
                }
            });
        }

        #region IDisposable Support
        public void Dispose()
        {
            _logger.Info(nameof(Dispose));
            _Disposables.Dispose();
        }
        #endregion

        public string ProductName
        {
            get
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                return asm.GetName().Name;
            }
        }

        public string Version
        {
            get
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                return "Version:" + asm.GetName().Version.ToString();
            }
        }

        public IRegionManager LicenseRegion { get; }

        public static string LicenseRegionName { get { return "LicenseRegion"; } }

        public ReactiveCommand FirstViewCommand { get; }

        public ReactiveCommand NavigateWeb { get; }

        /// <summary>
        /// 
        /// </summary>
        public License[] Licenses { get; }

        /// <summary>
        /// 
        /// </summary>
        public ReactiveProperty<License> SelectedLicense { get; set; }
    }
}
