using Prism.Common;
using Prism.Regions;
using System;
using System.Collections.Specialized;

namespace LiveNotify.Behaviors
{
    class DisposeRegionBehavior
        : IRegionBehavior
    {
        public const string Key = nameof(DisposeRegionBehavior);

        public IRegion Region
        {
            get;
            set;
        }

        public void Attach()
        {
            Region.Views.CollectionChanged += Views_CollectionChanged;
        }

        private void Views_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                Action<IDisposable> DisposeCaller = d => d.Dispose();
                foreach (var i in e.OldItems)
                {
                    MvvmHelpers.ViewAndViewModelAction(i, DisposeCaller);
                }
            }
        }
    }
}
