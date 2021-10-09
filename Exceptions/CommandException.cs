using System;

namespace MeiyounaiseSlash.Exceptions
{
    public class CommandException : Exception
    {
        public CommandException(string message) : base(message)
        {
            
        }
    }
}