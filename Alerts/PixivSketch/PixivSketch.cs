using Autofac;
using Codeplex.Data;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiveNotify.Models.Alerts
{
    /// <summary>
    /// Pixiv sketch alert
    /// </summary>
    public class PixivSketch
        : IAlertModel
    {
        private readonly LiveDescriptor[] _MyDescriptors = new LiveDescriptor[]
        {
            new LiveDescriptor("title", "Title", typeof(string))
            , new LiveDescriptor("user_name", "User Name", typeof(string))
            , new LiveDescriptor("user_id", "User ID", typeof(int))
            , new LiveDescriptor("pixiv_user_id", "Pixiv User ID", typeof(int))
            , new LiveDescriptor("unique_name", "Unique Name", typeof(string))
        };
        private readonly string LiveListUrl = "https://sketch.pixiv.net/lives/";
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private static HttpClient _client = new HttpClient();
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container">Autofac container</param>
        public PixivSketch(IContainer container)
        {
            var configulator = container.Resolve<LocalTestConfigulator>();

            Lives = new ObservableCollection<LiveItem>();
            if (configulator.IsLocalTest)
            {
                LiveListUrl = $"{configulator.Server.AbsoluteUri}pixiv_sketch/lives";
            }
            Descriptors = new Dictionary<LiveDescriptor, int>();
            for (int i = 0; i < _MyDescriptors.Length; i++)
            {
                Descriptors.Add(_MyDescriptors[i], i);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            _logger.Info(nameof(Dispose));
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        private LiveItem[] ProcessHtml(System.IO.Stream stm)
        {
            List<LiveItem> ret = new List<LiveItem>();

            try
            {
                HtmlDocument html = new HtmlDocument();
                html.Load(stm, Encoding.UTF8);
                var script = html.DocumentNode.SelectNodes(@"//script[@id=""state""]")
                    .Select(node =>
                    {
                        int JsonStart = node.InnerText.IndexOf('{');
                        string s = node.InnerText.Substring(JsonStart);
                        // Add p to "Number Only member". workaround.
                        return System.Text.RegularExpressions.Regex.Replace(s, @"""(\d*)""\s*:\s*{", @"""p$1"": {");
                    })
                    .First();
                dynamic d = DynamicJson.Parse(script);
                // Check JSON data
                if (!d.IsDefined("context"))
                {
                    throw new Exception("context not found");
                }
                if (!d.context.IsDefined("dispatcher"))
                {
                    throw new Exception("dispatcher not found");
                }
                if (!d.context.dispatcher.IsDefined("stores"))
                {
                    throw new Exception("stores not found");
                }
                if (!d.context.dispatcher.stores.IsDefined("UserStore"))
                {
                    throw new Exception("UserStore not found");
                }
                if (!d.context.dispatcher.stores.UserStore.IsDefined("users"))
                {
                    throw new Exception("users not found");
                }

                // Build User Store
                Dictionary<int, dynamic> UserStore = new Dictionary<int, dynamic>();
                foreach (var i in d.context.dispatcher.stores.UserStore.users.GetDynamicMemberNames())
                {
                    dynamic user = d.context.dispatcher.stores.UserStore.users[i];
                    UserStore.Add(int.Parse(user.id), user);
                }
                // Parse lives
                foreach (var i in d.context.dispatcher.stores.LiveStore.lives.GetDynamicMemberNames())
                {
                    dynamic live = d.context.dispatcher.stores.LiveStore.lives[i];
                    dynamic user = UserStore[int.Parse(live.owner.user_id)];
                    ret.Add(new LiveItem(
                        live.id
                        , DateTime.Parse(live.created_at)
                        , new Uri($"https://sketch.pixiv.net/@{user.unique_name}/lives/{live.id}")
                        , new string[] { live.name, user.name, user.id, user.pixiv_user_id, user.unique_name }));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return ret.ToArray();
        }

        public Task<bool> Update(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(async () =>
            {
                try
                {
                    // TODO: Implement Login to pixiv.
                    _logger.Info("Get data from " + LiveListUrl);
                    var ret = await _client.GetAsync(LiveListUrl, cancellationToken);
                    _logger.Info("Wait response");
                    if (!ret.IsSuccessStatusCode)
                    {
                        throw new Exception($"HTTP Error:{ret.StatusCode}");
                    }
                    using (var html = await ret.Content.ReadAsStreamAsync())
                    {
                        _logger.Info("Complete");
                        // Parsing HTML
                        LiveItem[] ProcessedLives = ProcessHtml(html);
                        // Update lives
                        // Remove old, Enumerate after remove operation.
                        var removed = Lives.Where(x => !ProcessedLives.Contains(x)).ToArray();
                        removed.All(x => Lives.Remove(x));
                        // Add new lives
                        var added = ProcessedLives.Where(x => !Lives.Contains(x));
                        _logger.Info($"Remove {removed.Length} Add {added.Count()}");
                        Lives.AddRange(added);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    return false;
                }
            });
        }

        public string Name { get { return "Pixiv Sketch"; } }
        
        public Dictionary<LiveDescriptor, int> Descriptors { get; }

        public ObservableCollection<LiveItem> Lives { get; private set; }
    }
}
