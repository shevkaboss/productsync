using Newtonsoft.Json;
using ProductSynchronizer.Logger;
using System.Collections.Generic;
using System.Linq;

namespace ProductSynchronizer.Utils
{
    public static class UnsuccessfulItemsHandler
    {
        private static readonly object locker = new object();
        private static readonly List<Error> _errors = new List<Error>();

        public static void AddUnsuccessfulProduct(int id, string message)
        {
            lock (locker)
            {
                var error = new Error
                {
                    ProductId = id.ToString(),
                    Message = message
                };
                Log.WriteLog($"Adding error to error array {JsonConvert.SerializeObject(error)}");

                _errors.Add(error);
            }
        }

        public static void AddError(Error error)
        {
            lock (locker)
            {
                Log.WriteLog($"Adding error to error array {JsonConvert.SerializeObject(error)}");
                _errors.Add(error);
            }
        }

        public static List<Error> GetErrors()
        {
            Log.WriteLog($"Total errors to update number: {_errors.Where(x => x.NeedToUpdateProductInDb).Count()}");
            Log.WriteLog($"Total errors number: {_errors.Count}");
            return _errors;
        }

        public static IEnumerable<string> GetUnsuccessfulProductIdsToUpdate()
        {
            return _errors.Where(x => x.NeedToUpdateProductInDb).Select(x => x.ProductId);
        }
    }
}
