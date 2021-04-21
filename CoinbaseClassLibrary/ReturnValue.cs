using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinbaseClassLibrary
{
    public class ReturnValue<T>
    {
        public string status { get; set; }
        public string message { get; set; }
        public T value { get; set; }
    }
}
