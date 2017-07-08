using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RescueMe
{
    public class RescueException : Exception
    { 
        public RescueException(string _message)
        {
            message= _message;
        }
        public override string Message => message;
        public string message { get; set; }
        public string userMessage { get; set; }
    }
}
