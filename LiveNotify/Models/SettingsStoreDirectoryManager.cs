using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using System.Configuration;

namespace LiveNotify.Models
{
    public class SettingsStoreDirectoryManager
    {
        public SettingsStoreDirectoryManager()
        {
            StorePath = new ReactiveProperty<string>();
            StorePathSelect = new ReactiveProperty<Enums.eSettingsStoreDirectorySelect>();
            StorePathSelect.Subscribe(x => StorePath.Value = GetStoreRootDirectory(x));

            Enums.eSettingsStoreDirectorySelect eSelect = Enums.eSettingsStoreDirectorySelect.ApplicationRoot;
            if (Enum.TryParse(Properties.Settings.Default.SettingsStoreDirectorySelect, out eSelect))
            {
                StorePathSelect.Value = eSelect;
            }
            else
            {
                StorePathSelect.Value = Enums.eSettingsStoreDirectorySelect.ApplicationRoot;
            }
        }

        private static string GetStoreRootDirectory(Enums.eSettingsStoreDirectorySelect select)
        {
            switch (select)
            {
                case Enums.eSettingsStoreDirectorySelect.ApplicationRoot:
                    System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                    return System.IO.Path.GetDirectoryName(asm.Location);
                case Enums.eSettingsStoreDirectorySelect.MyDocuments:
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + System.IO.Path.DirectorySeparatorChar + "LiveNotify";
                case Enums.eSettingsStoreDirectorySelect.LocalSettngs:
                    return System.IO.Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming).FilePath);
                default:
                    return string.Empty;
            }
        }

        public void Save()
        {
            Properties.Settings.Default.SettingsStoreDirectorySelect = StorePathSelect.Value.ToString();
            Properties.Settings.Default.Save();
            // Create directory
            if (!System.IO.Directory.Exists(StorePath.Value))
            {
                System.IO.Directory.CreateDirectory(StorePath.Value);
            }
            if (!System.IO.Directory.Exists(FavoritesPath))
            {
                System.IO.Directory.CreateDirectory(FavoritesPath);
            }
        }


        public static bool IsEmpty
        {
            get { return string.IsNullOrWhiteSpace(Properties.Settings.Default.SettingsStoreDirectorySelect); }
        }

        public static string FavoritesPath
        {
            get
            {
                Enums.eSettingsStoreDirectorySelect eSelect = Enums.eSettingsStoreDirectorySelect.ApplicationRoot;
                Enum.TryParse(Properties.Settings.Default.SettingsStoreDirectorySelect, out eSelect);
                return GetStoreRootDirectory(eSelect) + System.IO.Path.DirectorySeparatorChar + "Favorites";
            }
        }

        public ReactiveProperty<Enums.eSettingsStoreDirectorySelect> StorePathSelect { get; }

        public ReactiveProperty<string> StorePath { get; }
    }
}
