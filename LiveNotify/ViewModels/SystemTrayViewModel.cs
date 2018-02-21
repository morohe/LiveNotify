using Autofac;
using Prism.Events;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace LiveNotify.ViewModels
{
    class SystemTrayViewModel
        : BindableBase
        , IDisposable
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private string[] TaskTrayIconPaths = new string[]
        {
            "/LiveNotify;component/Resources/LiveNone.ico"
            , "/LiveNotify;component/Resources/LiveFound.ico"
        };

        private CompositeDisposable _Disposables = new CompositeDisposable();
        private Models.NotifyBalloonMessage _Messanger = null;
        private Models.AlertManager _Alert = null;

        public SystemTrayViewModel(IContainer container)
        {
            TaskTrayIcon = new ReactiveProperty<string>("/LiveNotify;component/Resources/App.ico");

            try
            {
                // Create Window
                if (null == App.Current.MainWindow)
                {
                    _logger.Info("Open " + nameof(Views.MainWindow));
                    App.Current.MainWindow = container.Resolve<Views.MainWindow>();
                }
                _Messanger = container.Resolve<Models.NotifyBalloonMessage>();

                // Create Alert Manager
                _Alert = container.Resolve<Models.AlertManager>();
                _Disposables.Add(_Alert);
                _Alert.FavMatchedLiveArrived += Alert_FavMatchedLiveArrived;
                _Alert.FavMatchedLiveEnd += Alert_FavMatchedLiveEnd;
                // Watch favorite list changed
                _Alert.Favorites.ToCollectionChanged().Subscribe(x =>
                {
                    if (_Alert.Favorites.All(y => 0 >= y.MatchedLives.Count))
                    {
                        TaskTrayIcon.Value = TaskTrayIconPaths[0];
                    }
                });

                // Delayed alart start
                Task.Delay(500).ContinueWith(x =>
                {
                    if (!_Alert.Start())
                    {
                        throw new Exception("Can not start Alert");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            ShowDialogCommand = new ReactiveCommand();
            ShowDialogCommand.Subscribe(() =>
            {
                if (false == App.Current.MainWindow?.IsVisible)
                {
                    _logger.Info("Show " + nameof(Views.MainWindow));
                    App.Current.MainWindow.Show();
                }
                else if (true == App.Current.MainWindow?.IsVisible)
                {
                    _logger.Info("Hide " + nameof(Views.MainWindow));
                    App.Current.MainWindow.Hide();
                }
            });

            ExitCommand = new ReactiveCommand();
            ExitCommand.Subscribe(() =>
            {
                _logger.Info("Exit Application");
                this.Dispose();
                App.Current.MainWindow?.Close();
                App.Current.Shutdown();
            });
        }

        public void Dispose()
        {
            _logger.Info(nameof(Dispose));
            if ((null != _Alert) && !_Disposables.IsDisposed)
            {
                _Alert.FavMatchedLiveArrived -= Alert_FavMatchedLiveArrived;
            }
            _Disposables.Dispose();
        }

        private void Alert_FavMatchedLiveArrived(object sender, Models.FavMatchedLiveChangedEventArgs e)
        {
            foreach (var i in e.Matched)
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("Label:{0} Query:{1}\n", i.Label, i.Query);
                message.Append(i.MatchedLives[0].Url);
                message.Append("\n");
                _Messanger.GetEvent<PubSubEvent<string>>().Publish(message.ToString());
            }
            if (_Alert.Favorites.Any(x => 0 < x.MatchedLives.Count))
            {
                TaskTrayIcon.Value = TaskTrayIconPaths[1];
            }
        }

        private void Alert_FavMatchedLiveEnd(object sender, Models.FavMatchedLiveChangedEventArgs e)
        {
            if (_Alert.Favorites.All(x => 0 >= x.MatchedLives.Count))
            {
                TaskTrayIcon.Value = TaskTrayIconPaths[0];
            }
        }

        public ReactiveCommand ShowDialogCommand { get; }

        public ReactiveCommand ExitCommand { get; }

        public ReactiveProperty<string> TaskTrayIcon { get; }
    }
}
