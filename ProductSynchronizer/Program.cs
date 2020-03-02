using ProductSynchronizer.Helpers;
using ProductSynchronizer.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ProductSynchronizer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Logger.Logger.InitLogger();
            var test1 = MapsHelper.GetCurrencyValue(Resource.Goat);
            var products = MySqlHelper.GetProducts();

            var test = new GoatWorker();
            var prod = products.First(x => x.Location.Contains("goat"));
            //prod.Location = @"https://www.footasylum.com/men/mens-footwear/trainers/adidas-ultraboost-20-trainer-core-black-white-4033016/?src=froogle&ir";
            var syncedProduct = test.GetSyncedData(prod);
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new ProductSynchronizer()
            //};
            //ServiceBase.Run(ServicesToRun);
            }
    }
}
