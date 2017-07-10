using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RescueMe
{
    public static class Extensions
    {
        public static TModel JsonToObject<TModel>(this object model) where TModel : class
        {
            TModel deserializeModel = null;
            RescueException exception = null;

            try
            {
                //Case 1: Exception = null
                //Case 2: Exception != null && Message == null
                exception = JsonConvert.DeserializeObject<RescueException>(model.ToString());
                deserializeModel = TryToDeserialize<TModel>(exception, model);
            }
            catch (RescueException r)
            {
                throw r;
            }
            catch (Exception)
            {
                deserializeModel = TryToDeserialize<TModel>(exception, model);
            }

            return deserializeModel;
        }

        private static TModel TryToDeserialize<TModel>(RescueException exception, object model)
        {
            TModel tModel = Activator.CreateInstance<TModel>();
            if (exception == null || exception.message == null)
            {
                try
                {
                    tModel = JsonConvert.DeserializeObject<TModel>(model.ToString());
                }
                catch (Exception e)
                {
                    //  tModel = Activator.CreateInstance<TModel>();
                    throw new Exception(e.Message);
                }

            }
            else
            {
                throw new RescueException(exception.message);
            }

            return tModel;
        }

    }
}
