using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSystem.Contract.Response
{
    public class SingleResponeMessage<T>
    {
        public T Item { get; set; }
        public bool IsSuccess { get; set; }
        public string MsgString { get; set; }

    }
}
