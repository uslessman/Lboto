using System;
using System.Collections.Generic;
using Lboto.Helpers.CachedObjects;

namespace Lboto.Helpers
{
    public class ItemsSoldArgs : EventArgs
    {
        public List<CachedItem> SoldItems { get; }
        public List<CachedItem> GainedItems { get; }

        public ItemsSoldArgs(List<CachedItem> sold, List<CachedItem> gained)
        {
            SoldItems = sold;
            GainedItems = gained;
        }
    }
}
