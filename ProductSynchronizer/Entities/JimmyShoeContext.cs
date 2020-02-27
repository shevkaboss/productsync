namespace ProductSynchronizer
{
    class JimmyShoeContext : ISizeMapNode
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public string InternalSize { get; set; }
        public string ExternalSize { get; set; }
    }
}
