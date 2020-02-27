using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProductSynchronizer
{
    public class Product
    { 
        public int InternalId { get; set; }
        public string Location { get; set; }
        public List<ISizeMapNode> ShoesSizeMap { get; set; }
        public Resource Resource
        {
            get
            {
                switch (Regex.Match(Location, Constants.REGEX_FOR_DOMAIN).Value)
                {
                    case Constants.STOCK_URL:
                        {
                            return Resource.Stock;
                        }
                    case Constants.FOOTASYLUM_URL:
                        {
                            return Resource.Footasylum;
                        }
                    case Constants.JIMMY_JAZZ_URL:
                        {
                            return Resource.JimmyJazz;
                        }
                    default:
                        {
                            return Resource.Udentified;
                        }

                }
            }
        }
    }

}
