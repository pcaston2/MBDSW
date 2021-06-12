using MBDSW;
using System;

namespace MBDSWCLI
{
    public class ConsoleLogger : ILog
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}