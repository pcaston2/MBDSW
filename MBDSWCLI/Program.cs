using MBDSW;
using System;

namespace MBDSWCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var manager = new ServerManager(logger);
            logger.Log("Minecraft Bedrock Dedicated Server Wrapper Command Line Integration");
            while (true)
            {
                logger.Log("MBDSWCLI:");
                var command = Console.ReadLine().ToLower();
                switch (command)
                {
                    case "update":
                        manager.Update();
                        break;
                    case "start":
                        manager.Start();
                        break;
                    default:
                        logger.Log("Command not recognized.");
                        break;
                }
            }
        }
    }
}
