using System;

namespace LiveNotify.Models
{
    /// <summary>
    /// Live stream model
    /// </summary>
    public class LiveItem
    {
        /// <summary>
        /// Live stream model constructor
        /// </summary>
        /// <param name="id">Stream unique ident</param>
        /// <param name="title">Stream title</param>
        /// <param name="user">User name</param>
        /// <param name="startDate">Stream start date</param>
        /// <param name="url">Stream URL</param>
        /// <param name="idents">Other identification informations</param>
        public LiveItem(string id, DateTime startDate, Uri url, string[] idents)
        {
            Id = id;
            StartDate = startDate;
            Url = url;
            Descriptors = idents;
        }

        public override bool Equals(object obj)
        {
            if ((null == obj) || (obj.GetType() != typeof(LiveItem)))
            {
                return false;
            }
            LiveItem val = (obj as LiveItem);
            return (val.Id == this.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Stream unique identification
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Stream start date
        /// </summary>
        public DateTime StartDate { get; }

        /// <summary>
        /// Stream URL
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// Other identification informations
        /// </summary>
        public string[] Descriptors { get; }
    }
}
