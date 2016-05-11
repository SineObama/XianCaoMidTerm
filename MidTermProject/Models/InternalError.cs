using System;

namespace MidTermProject.Models
{
    class InternalError : Exception
    {
        public InternalError(string msg = "") : base("Program internal error: " + msg) { }
    }
}
