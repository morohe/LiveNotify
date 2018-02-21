using NLog;
using NLog.Config;
using NLog.Targets;
using System.Windows;
using System;

namespace LiveNotify
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Bootstrapper bootstrapper = null;
        internal static StartupEventArgs CommandLineArgs;

        protected override void OnStartup(StartupEventArgs e)
        {
            CommandLineArgs = e;
            // NLog Configulations.
            var config = new LoggingConfiguration();
            // Setup Debugger Target
            var DebuggerTarget = new DebuggerTarget();
            config.AddTarget("Debugger", DebuggerTarget);
            var DebuggerRule = new LoggingRule("*", LogLevel.Debug, DebuggerTarget);
            config.LoggingRules.Add(DebuggerRule);
            // Apply Configulation
            LogManager.Configuration = config;

            LogManager.GetCurrentClassLogger().Info("Start application");

            base.OnStartup(e);

            // Show at first run.
            try
            {
                if (Models.SettingsStoreDirectoryManager.IsEmpty)
                {
                    Views.FirstSetupWindow setup = new Views.FirstSetupWindow();
                    if (true != setup.ShowDialog())
                    {
                        Current.Shutdown();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                Current.Shutdown();
                return;
            }

            bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            bootstrapper?.CloseShell();

            base.OnExit(e);
        }
    }
}
