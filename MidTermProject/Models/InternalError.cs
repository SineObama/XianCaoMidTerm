using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidTermProject.Models
{
    class InternalError : Exception
    {
        public InternalError(string msg = "") : base("Program internal error: " + msg) { }
    }
}
