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
        public static T JsonToObject<T>(this object model) where T : class
        {
            T deserializeModel = null;
            try
            {
                 deserializeModel = JsonConvert.DeserializeObject<T>(model.ToString());
            }catch(Exception e)
            {
                deserializeModel = null;
                throw new Exception(model.ToString());
            }
            return deserializeModel;
        }
    }
}
