using System;

namespace MeiyounaiseSlash.Exceptions
{
    public class InteractionTimeoutException : Exception
    {
        public InteractionTimeoutException(string message) : base(message)
        {
        }
    }
}