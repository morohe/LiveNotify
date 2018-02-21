using Autofac;
using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace LiveNotify.Models
{
    /// <summary>
    /// Alert manager
    /// </summary>
    public class AlertManager
        : IDisposable
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        
        private CompositeDisposable _Disposables = new CompositeDisposable();
        private Dictionary<string, Alerts.IAlertModel> _Alerts = null;
        private TimeSpan _CheckInterval = TimeSpan.FromMinutes(10);
        private Timer _timer = null;
        private CancellationTokenSource _Cancel = null;

        /// <summary>
        /// Alert manager constructor
        /// </summary>
        public AlertManager(IContainer container)
        {
            _Alerts = new Dictionary<string, Alerts.IAlertModel>();
#if DEBUG
            _Alerts.Add(nameof(Alerts.TestAlert), container.Resolve<Alerts.TestAlert>());
#endif
            _Alerts.Add(nameof(Alerts.PixivSketch), container.Resolve<Alerts.PixivSketch>());
            
            foreach(var i in _Alerts.Values)
            {
                _Disposables.Add(i);
            }
            Favorites = new ObservableCollection<Favorite>();
            // Load from props
            Load();
        }

        public void Dispose()
        {
            _logger.Info(nameof(Dispose));
            _Cancel?.Cancel();
            _Disposables.Dispose();
        }

        private Task UpdateAlerts(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Update Alerts.
            return _Alerts.Values.ParallelForEachAsync(async i =>
            {
                try
                {
                    await i.Update();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            });
        }

        /// <summary>
        /// Check favorite live
        /// </summary>
        private Task CheckFavorites(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                try
                {
                    List<Favorite> NewMatchedFavorites = new List<Favorite>();
                    List<Favorite> RemoveMatchedFavorites = new List<Favorite>();

                    foreach (var i in _Alerts)
                    {
                        // Get site favorite
                        var favs = Favorites.Where(x => (x.AlertSource == i.Key) && x.IsEnabled);
                        var lives = i.Value.Lives;
                        Dictionary<LiveDescriptor, int> Descs = i.Value.Descriptors;

                        foreach (var j in favs)
                        {
                            LiveDescriptor QueryDesc = Descs.Keys.FirstOrDefault(x => x.DataName == j.QueryTarget);
                            Func<LiveItem, bool> func = null;
                            // fuzzy compare for string
                            if (QueryDesc.DataType == typeof(string))
                            {
                                func = x => 0 <= x.Descriptors[Descs[QueryDesc]].IndexOf(j.Query);
                            }
                            else
                            {
                                func = x => x.Descriptors[Descs[QueryDesc]].Equals(j.Query);
                            }
                            var matches = lives.Where(func);
                            var newlives = matches.Where(x => !j.MatchedLives.Contains(x));
                            // Remove Ended Live
                            var removelives = j.MatchedLives.Where(x => !matches.Contains(x)).ToArray();
                            // Add arrived live
                            if (0 < newlives?.Count())
                            {
                                j.MatchedLives.AddRange(newlives);
                                // For notify
                                NewMatchedFavorites.Add(j);
                                j.LatestFoundDate.Value = DateTime.Now;
                            }
                            // Remove ended lives.
                            if (0 < removelives?.Count())
                            {
                                removelives.All(remove => j.MatchedLives.Remove(remove));
                                if (0 >= j.MatchedLives.Count)
                                {
                                    RemoveMatchedFavorites.Add(j);
                                }
                            }
                        }
                    }
                    // Call event
                    if (0 < NewMatchedFavorites.Count)
                    {
                        FavMatchedLiveArrived?.Invoke(this, new FavMatchedLiveChangedEventArgs()
                        {
                            Matched = NewMatchedFavorites.ToArray()
                        });
                    }
                    if (0 < RemoveMatchedFavorites.Count)
                    {
                        FavMatchedLiveEnd?.Invoke(this, new FavMatchedLiveChangedEventArgs()
                        {
                            Matched = RemoveMatchedFavorites.ToArray()
                        });
                    }
                }
                catch
                {

                }
            }, cancellationToken);
        }

        /// <summary>
        /// Start Alerts
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            try
            {
                Stop();
                // Create new Cancellation token source
                if ((null == _Cancel) || _Cancel.IsCancellationRequested)
                {
                    _Cancel = new CancellationTokenSource();
                }
                _timer = new Timer(new TimerCallback(async x =>
                {
                    try
                    {
                        await UpdateAlerts(_Cancel.Token);
                        await CheckFavorites(_Cancel.Token);
                        _timer.Change(_CheckInterval, TimeSpan.Zero);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }), null, 0, Timeout.Infinite);

                _Disposables.Add(_timer);
                _Disposables.Add(_Cancel);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Stop Alerts
        /// </summary>
        public void Stop()
        {
            if (_Cancel != null)
            {
                _Disposables.Remove(_Cancel);
            }
            _Cancel?.Cancel();
            _Cancel?.Dispose();

            if (_timer != null)
            {
                _Disposables.Remove(_timer);
            }
            _timer?.Dispose();
        }

        /// <summary>
        /// save to props and JSON
        /// </summary>
        public void Save()
        {
            foreach(var i in AlertNames)
            {
                Favorite[] favs = Favorites.Where(x => x.AlertSource == i).ToArray();
                string path = SettingsStoreDirectoryManager.FavoritesPath
                    + System.IO.Path.DirectorySeparatorChar
                    + i + ".json";
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path))
                {
                    sw.Write(Codeplex.Data.DynamicJson.Serialize(favs));
                }
            }
            Properties.Settings.Default.CheckInterval = CheckInterval;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Load from props and JSON
        /// </summary>
        public void Load()
        {
            _CheckInterval = Properties.Settings.Default.CheckInterval;
            foreach (var i in AlertNames)
            {
                string path = SettingsStoreDirectoryManager.FavoritesPath
                    + System.IO.Path.DirectorySeparatorChar
                    + i + ".json";
                if (System.IO.File.Exists(path))
                {
                    using (System.IO.Stream s = System.IO.File.Open(path, System.IO.FileMode.Open))
                    {
                        Favorite[] favs = Codeplex.Data.DynamicJson.Parse(s);
                        Favorites.AddRange(favs);
                    }
                }
            }
        }

        /// <summary>
        /// Add favorite
        /// </summary>
        /// <param name="fav">fav</param>
        /// <returns>True = success/False = fail</returns>
        public Task<bool> AddFavorite(Favorite fav, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Exists.
                if (Favorites.Contains(fav))
                {
                    return Task.FromResult(false);
                }
                Favorites.Add(fav);

                return CheckFavorites(cancellationToken).ContinueWith(x =>
                {
                    return true;
                });
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Remvoe favorite
        /// </summary>
        /// <param name="fav"></param>
        public void RemoveFavorite(Favorite fav)
        {
            Favorites.Remove(fav);
        }

        /// <summary>
        /// Get alert localized name by Alert
        /// </summary>
        /// <param name="alertSource">Alert</param>
        /// <returns>localized name</returns>
        public string GetAlertLocalizedName(string alertSource)
        {
            if (!_Alerts.ContainsKey(alertSource))
            {
                return null;
            }
            return _Alerts[alertSource].Name;
        }

        /// <summary>
        /// Get live list
        /// </summary>
        /// <param name="alertSource">live list source</param>
        /// <returns>Live list </returns>
        public ObservableCollection<LiveItem> GetLiveList(string alertSource)
        {
            if (!_Alerts.ContainsKey(alertSource))
            {
                return null;
            }
            return _Alerts[alertSource].Lives;
        }

        /// <summary>
        /// Get query targets
        /// </summary>
        /// <param name="alertSource">Alert</param>
        /// <returns>Ident target list</returns>
        public Dictionary<LiveDescriptor, int> GetQueryTargets(string alertSource)
        {
            List<string> types = new List<string>();

            if (!_Alerts.ContainsKey(alertSource))
            {
                return null;
            }

            return _Alerts[alertSource].Descriptors;
        }

        /// <summary>
        /// Live arrived event
        /// </summary>
        public event EventHandler<FavMatchedLiveChangedEventArgs> FavMatchedLiveArrived;

        /// <summary>
        /// Live end event
        /// </summary>
        public event EventHandler<FavMatchedLiveChangedEventArgs> FavMatchedLiveEnd;

        /// <summary>
        /// Live check interval
        /// </summary>
        public TimeSpan CheckInterval
        {
            get
            {
                return _CheckInterval;
            }
            set
            {
                if ((value != _CheckInterval)
                    && (TimeSpan.Zero < value))
                {
                    _CheckInterval = value;
                    // Reload timer
                    if (null != _timer)
                    {
                        _timer.Change(_CheckInterval, TimeSpan.Zero);
                    }
                }
            }
        }

        /// <summary>
        /// Alert favorites
        /// </summary>
        public ObservableCollection<Favorite> Favorites { get; private set; }

        /// <summary>
        /// Alert names
        /// </summary>
        public string[] AlertNames
        {
            get
            {
                return _Alerts.Keys.ToArray();
            }
        }
    }
}
