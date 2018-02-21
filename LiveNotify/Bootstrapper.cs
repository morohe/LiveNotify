using Autofac;
using LiveNotify.Views;
using Prism.Autofac;
using Prism.Regions;
using System;
using System.Windows;

namespace LiveNotify
{
    class Bootstrapper : AutofacBootstrapper
    {
        private static SystemTrayView _SystemTray = null;

        protected override DependencyObject CreateShell()
        {
            // Initialize Configulation
            if (2 == App.CommandLineArgs.Args.Length)
            {
                if (0 == string.Compare("-t", App.CommandLineArgs.Args[0]))
                {
                    Models.LocalTestConfigulator config = Container.Resolve<Models.LocalTestConfigulator>();
                    config.IsLocalTest = true;
                    config.Server = new Uri(App.CommandLineArgs.Args[1]);
                }
            }
            // Start tray view
            _SystemTray = Container.Resolve<SystemTrayView>();
            // Main window stay null.
            return null;
        }

        public void CloseShell()
        {
            _SystemTray?.Dispose();
        }

        protected override void InitializeShell()
        {
        }

        protected override void ConfigureContainerBuilder(ContainerBuilder builder)
        {
            base.ConfigureContainerBuilder(builder);
            // Register Views
            builder.RegisterType<SystemTrayView>();
            builder.RegisterType<MainWindow>();
            builder.RegisterType<FirstSetupWindow>();
            builder.RegisterTypeForNavigation<AboutView>();
            builder.RegisterTypeForNavigation<LicenseViewer.Views.LicenseView>();
            builder.RegisterTypeForNavigation<SetupView>();
            builder.RegisterTypeForNavigation<LiveListView>();
            // Register Autofac Modules
#if DEBUG
            builder.RegisterModule<TestAlert.TestAlertModuleRegistry>();
#endif
            builder.RegisterModule<PixivSketch.PixivSketchModuleRegistry>();
            // Register as Singleton
            builder.RegisterType<Models.AlertManager>().SingleInstance();
            builder.RegisterType<Models.LocalTestConfigulator>().SingleInstance();
            builder.RegisterType<Models.NotifyBalloonMessage>().SingleInstance();
        }

        protected override IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
        {
            IRegionBehaviorFactory f = base.ConfigureDefaultRegionBehaviors();
            // Dispose Behavior
            f.AddIfMissing(Behaviors.DisposeRegionBehavior.Key, typeof(Behaviors.DisposeRegionBehavior));
            return f;
        }
    }
}
