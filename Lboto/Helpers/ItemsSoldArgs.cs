using System;
using System.Collections.Generic;
using Lboto.Helpers.CachedObjects;
using DreamPoeBot.Loki.Bot;

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
        public ItemsSoldArgs(Message message)
        {
            SoldItems = message.GetInput<List<CachedItem>>(0);
            GainedItems = message.GetInput<List<CachedItem>>(1);
        }
    }
}
