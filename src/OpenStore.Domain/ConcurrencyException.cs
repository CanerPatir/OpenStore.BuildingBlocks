using System;

namespace OpenStore.Domain
{
    public class ConcurrencyException : System.Exception
    {
        public ConcurrencyException(string msg, Exception innerException = null) : base(msg, innerException)
        {
        }
    }
}