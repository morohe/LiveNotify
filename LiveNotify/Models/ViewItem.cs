using System;
using Prism.Regions;

namespace LiveNotify.Models
{
    /// <summary>
    /// View model item
    /// </summary>
    public class ViewItem
    {
        /// <summary>
        /// View user control
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="viewName">View name</param>
        /// <param name="param">Navigation parameters</param>
        public ViewItem(string label, string viewName, NavigationParameters param)
        {
            Label = label;
            ViewName = viewName;
            Parameters = param;
        }

        /// <summary>
        /// View Label
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// View object name
        /// </summary>
        public string ViewName { get; }

        /// <summary>
        /// Navigation parameter
        /// </summary>
        public NavigationParameters Parameters { get; }
    }
}
