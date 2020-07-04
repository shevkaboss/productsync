using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProductSynchronizer.Entities;

namespace ProductSynchronizer
{
    public enum Gender
    {
        Man = 60,
        Woman = 59,
        Male = 60,
        Female = 59
    }
    public class Product
    {
        public int InternalId { get; set; }
        public int ProductOptionId { get; set; }
        public double CommonPrice { get; set; } = -1;
        public string Location { get; set; }
        public List<ISizeMapNode> ShoesSizeMap { get; set; }
        public string Brand { get; set; }
        public Gender Gender { get; set; }
        public Resource Resource
        {
            get
            {
                switch (Regex.Match(Location, Constants.REGEX_FOR_DOMAIN).Value)
                {
                    case Constants.GOAT_URL:
                        {
                            return Resource.Goat;
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
