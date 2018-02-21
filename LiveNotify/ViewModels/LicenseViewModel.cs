using Prism.Mvvm;
using Prism.Regions;
using System;

namespace LicenseViewer.ViewModels
{
    public class LicenseViewModel
        : BindableBase
        , INavigationAware
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public LicenseViewModel()
        {

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (null != navigationContext)
            {
                Models.License model = (navigationContext.Parameters[nameof(Models.License)] as Models.License);
                string LicenseDir = (navigationContext.Parameters["LicenseDirectory"] as string);

                try
                {
                    if (null != model)
                    {
                        Title = model.Title;
                        Author = model.Author;
                        string LicenseFile = LicenseDir + System.IO.Path.DirectorySeparatorChar + model.LicenseFile;
                        if (System.IO.File.Exists(LicenseFile))
                        {
                            License = System.IO.File.ReadAllText(LicenseFile);
                        }
                        RaisePropertyChanged("");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        public string Title { get; private set; }

        public string Author { get; private set; }

        public string License { get; private set; }
    }
}
