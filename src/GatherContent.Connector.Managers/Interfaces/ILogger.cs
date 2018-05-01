using System;

namespace GatherContent.Connector.Managers.Interfaces
{
    public interface ILogger
    {
        void Warn(string message, object sender);

        void Error(string message, Exception exception);
    }

    internal class NullLogger : ILogger
    {
        public void Warn(string message, object sender)
        {
            Console.WriteLine("WARN: " + sender.GetType().FullName + " : " + message);
        }

        public void Error(string message, Exception exception)
        {
            Console.WriteLine("ERROR: " + message);
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);
        }
    }
}