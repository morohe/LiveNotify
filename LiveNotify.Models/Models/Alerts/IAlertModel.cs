using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LiveNotify.Models.Alerts
{
    /// <summary>
    /// Alert model interface
    /// </summary>
    public interface IAlertModel : IDisposable
    {
        /// <summary>
        /// Update live list from site
        /// </summary>
        Task<bool> Update(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Identifiers
        /// </summary>
        Dictionary<LiveDescriptor, int> Descriptors { get; }

        /// <summary>
        /// Live list
        /// </summary>
        ObservableCollection<LiveItem> Lives { get; }
    }
}
