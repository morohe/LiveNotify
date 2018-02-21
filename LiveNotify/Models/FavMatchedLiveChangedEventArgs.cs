using System;
using System.Collections.Generic;

namespace LiveNotify.Models
{
    public class FavMatchedLiveChangedEventArgs
        : EventArgs
    {
        public Favorite[] Matched;
    }
}
