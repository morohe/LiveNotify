using System;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Reactive.Bindings;
using Prism.Mvvm;

namespace LiveNotify.Models
{
    [DataContract]
    public class Favorite
        : BindableBase
    {
        /// <summary>
        /// 
        /// </summary>
        public Favorite()
        {
        }

        /// <summary>
        /// Favorite Constructor
        /// </summary>
        /// <param name="alertSource">Alert source. Empty string indicate All Alert source</param>
        /// <param name="label">Favorite label</param>
        /// <param name="query">Query</param>
        /// <param name="queryTarget">Query target</param>
        public Favorite(string alertSource, string label, string query, string queryTarget)
        {
            AlertSource = alertSource;
            Label = label;
            Query = query;
            QueryTarget = queryTarget;

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException($"{query} can not be null");
            }
            if (string.IsNullOrWhiteSpace(queryTarget))
            {
                throw new ArgumentNullException($"{queryTarget} can not be null");
            }
        }

        /// <summary>
        /// Only check querys
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if ((null == obj) || (obj.GetType() != typeof(Favorite)))
            {
                return false;
            }

            Favorite l = (Favorite)obj;
            return (l.AlertSource == AlertSource)
                && (l.Query == Query)
                && (l.QueryTarget == QueryTarget);
        }

        public override int GetHashCode()
        {
            return AlertSource.GetHashCode()
                ^ Query.GetHashCode()
                ^ QueryTarget.GetHashCode();
        }

        /// <summary>
        /// Target Alert source
        /// </summary>
        [DataMember]
        public string AlertSource { get; set; }

        /// <summary>
        /// Favorite Label
        /// </summary>
        [DataMember]
        public string Label { get; set; }

        /// <summary>
        /// Query
        /// </summary>
        [DataMember]
        public string Query { get; set; }

        /// <summary>
        /// Query target
        /// </summary>
        [DataMember]
        public string QueryTarget { get; set; }

        /// <summary>
        /// Enable/Disable notify
        /// </summary>
        [DataMember]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Live found date
        /// </summary>
        [IgnoreDataMember]
        public ReactiveProperty<DateTime> LatestFoundDate { get; } = new ReactiveProperty<DateTime>();

        /// <summary>
        /// Matched lives
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCollection<LiveItem> MatchedLives { get; } = new ReactiveCollection<LiveItem>();
    }
}
