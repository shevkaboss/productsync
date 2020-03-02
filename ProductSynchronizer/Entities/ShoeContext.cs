﻿namespace ProductSynchronizer.Entities
{
    public class ShoeContext : ISizeMapNode
    {
        public int Quantity { get; set; }
        public string InternalSize { get; set; }
        public string ExternalSize { get; set; }
        public string ExternalPrice { get; set; }
        public string InternalPrice { get; set; }
    }
}
