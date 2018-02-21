using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LiveNotify.Models.Alerts
{
    public class TestAlert
        : IAlertModel
    {
        private readonly LiveDescriptor[] _MyDescriptors = new LiveDescriptor[]
        {
            new LiveDescriptor("title", "Title", typeof(string))
            , new LiveDescriptor("user_name", "User Name", typeof(string))
        };
        private readonly string _TestFile = "TestAlert.json";
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public TestAlert()
        {
            Lives = new ObservableCollection<LiveItem>();
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
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public Task<bool> Update(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                List<LiveItem> ProcessedLives = new List<LiveItem>();

                try
                {
                    _logger.Info("Get Data");
                    if (!System.IO.File.Exists(_TestFile))
                    {
                        return false;
                    }
                    using (System.IO.StreamReader s = new System.IO.StreamReader(_TestFile))
                    {
                        dynamic d = DynamicJson.Parse(s.ReadToEnd());
                        foreach (var i in d.streams)
                        {
                            ProcessedLives.Add(new LiveItem(i.id, DateTime.Now, new Uri(i.url), new string[] { i.title, i.user_name }));
                        }
                    }
                    // Update lives
                    // Remove old
                    Lives.Where(x => !ProcessedLives.Contains(x)).ToArray().All(x => Lives.Remove(x));
                    // Add new lives
                    Lives.AddRange(ProcessedLives.Where(x => !Lives.Contains(x)));
                    _logger.Info("JSON Parse End");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    return false;
                }
                return true;
            });
        }

        public string Name { get { return "Test Alert"; } }

        public Dictionary<LiveDescriptor, int> Descriptors { get; }

        public ObservableCollection<LiveItem> Lives { get; private set; }
    }
}
