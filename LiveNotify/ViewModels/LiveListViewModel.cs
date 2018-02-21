using Autofac;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Data;

namespace LiveNotify.ViewModels
{
    public class LiveListViewModel
        : BindableBase
        , IDisposable
        , INavigationAware
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private CompositeDisposable _Disposables = new CompositeDisposable();
        private IContainer _Container = null;
        private string _AlertSource = string.Empty;

        public LiveListViewModel(IContainer container)
        {
            _Container = container;

            FavoriteLabel = new ReactiveProperty<string>();
            FavoriteQuery = new ReactiveProperty<string>()
                .SetValidateNotifyError(x => string.IsNullOrWhiteSpace(x) ? "Please input query" : null);

            FavoriteQueryTargets = new Dictionary<Models.LiveDescriptor, int>();
            SelectedFavoriteQueryTarget = new ReactiveProperty<KeyValuePair<Models.LiveDescriptor, int>>();

            Favorites = _Container.Resolve<Models.AlertManager>().Favorites.ToReadOnlyReactiveCollection();
            CollectionViewSource.GetDefaultView(Favorites).Filter = x =>
            {
                return (x as Models.Favorite)?.AlertSource == _AlertSource;
            };

            // Open in System default WebBrowser
            DoubleClickLiveItemCommand = new ReactiveCommand();
            DoubleClickLiveItemCommand.Subscribe(x =>
            {
                if (x?.GetType() == typeof(Models.LiveItem))
                {
                    Process.Start(new ProcessStartInfo((x as Models.LiveItem).Url.ToString()) { UseShellExecute = true });
                }
            });

            AddFavoriteCommand = FavoriteQuery.ObserveHasErrors.Inverse().ToReactiveCommand();
            AddFavoriteCommand.Subscribe(async () =>
            {
                try
                {
                    Models.AlertManager manager = _Container.Resolve<Models.AlertManager>();
                    Models.Favorite fav = new Models.Favorite(
                        _AlertSource
                        , FavoriteLabel.Value
                        , FavoriteQuery.Value
                        , SelectedFavoriteQueryTarget.Value.Key.DataName);
                    fav.IsEnabled = true;
                    if (await manager.AddFavorite(fav))
                    {
                        manager.Save();
                        // Clear edits.
                        FavoriteLabel.Value = string.Empty;
                        FavoriteQuery.Value = string.Empty;
                        SelectedFavoriteQueryTarget.Value = FavoriteQueryTargets.First();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            });

            RemoveFavoriteCommand = new ReactiveCommand();
            RemoveFavoriteCommand.Subscribe(x =>
            {
                try
                {
                    Models.AlertManager manager = _Container.Resolve<Models.AlertManager>();
                    manager.RemoveFavorite((x as Models.Favorite));
                    manager.Save();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            });

            SearchCommand = new ReactiveCommand();
            SearchCommand.Subscribe(() =>
            {
                // Refresh view
                CollectionViewSource.GetDefaultView(Lives).Refresh();
            });

            ClearSearchBoxCommand = new ReactiveCommand();
            ClearSearchBoxCommand.Subscribe(() =>
            {
                FavoriteLabel.Value = string.Empty;
                FavoriteQuery.Value = string.Empty;
                SelectedFavoriteQueryTarget.Value = FavoriteQueryTargets.First();
                CollectionViewSource.GetDefaultView(Lives).Refresh();
            });

        }

        public void Dispose()
        {
            _logger.Info(nameof(Dispose));
            _Disposables.Dispose();
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
            if ((null != navigationContext)
                && !navigationContext.Parameters["view"].Equals(_AlertSource))
            {
                try
                {
                    _AlertSource = (string)navigationContext.Parameters["view"];

                    Models.AlertManager Alert = _Container.Resolve<Models.AlertManager>();
                    // Replace Live list
                    if (null != Lives)
                    {
                        _Disposables.Remove(Lives);
                    }
                    Lives?.Dispose();
                    Lives = Alert.GetLiveList(_AlertSource).ToReadOnlyReactiveCollection().AddTo(_Disposables);
                    // View filter
                    CollectionViewSource.GetDefaultView(Lives).Filter = x =>
                    {
                        if (string.IsNullOrWhiteSpace(FavoriteQuery.Value))
                        {
                            return true;
                        }
                        // sanitize
                        //string escaped = System.Text.RegularExpressions.Regex.Escape(FavoriteQuery.Value);
                        //return System.Text.RegularExpressions.Regex.IsMatch(x.Title, $"({escaped})+");
                        return 0 <= (x as Models.LiveItem)?.Descriptors[SelectedFavoriteQueryTarget.Value.Value].IndexOf(FavoriteQuery.Value);
                    };
                    // Generate columns
                    List<Models.LiveItemColumn> columns = new List<Models.LiveItemColumn>();
                    Models.LiveDescriptor[] descs = Alert.GetQueryTargets(_AlertSource).Keys.ToArray();
                    for (int i = 0; i < descs.Length; i++)
                    {
                        columns.Add(new Models.LiveItemColumn(descs[i].Label, $"Descriptors[{i}]"));
                    }
                    columns.Add(new Models.LiveItemColumn("Start Date", nameof(Models.LiveItem.StartDate)));
                    columns.Add(new Models.LiveItemColumn("URL", nameof(Models.LiveItem.Url)));
                    LiveColumns = columns.ToArray();
                    // Notify LiveList PropertyChanged to View
                    RaisePropertyChanged(nameof(Lives));
                    RaisePropertyChanged(nameof(LiveColumns));
                    // Replace query type list
                    FavoriteQueryTargets = Alert.GetQueryTargets(_AlertSource);
                    RaisePropertyChanged(nameof(FavoriteQueryTargets));
                    SelectedFavoriteQueryTarget.Value = FavoriteQueryTargets.First();
                    // Refresh favorite
                    CollectionViewSource.GetDefaultView(Favorites).Refresh();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        public ReactiveCommand DoubleClickLiveItemCommand { get; }

        public ReactiveCommand AddFavoriteCommand { get; }

        public ReactiveCommand RemoveFavoriteCommand { get; }

        public ReactiveCommand SearchCommand { get; }

        public ReactiveCommand ClearSearchBoxCommand { get; }

        public ReadOnlyReactiveCollection<Models.LiveItem> Lives { get; private set; }

        public Models.LiveItemColumn[] LiveColumns { get; set; }

        public ReactiveProperty<string> FavoriteLabel { get; }

        public ReactiveProperty<string> FavoriteQuery { get; }

        public Dictionary<Models.LiveDescriptor, int> FavoriteQueryTargets { get; private set; }

        public ReactiveProperty<KeyValuePair<Models.LiveDescriptor, int>> SelectedFavoriteQueryTarget { get; private set; }

        public ReadOnlyReactiveCollection<Models.Favorite> Favorites { get; private set; }
    }
}