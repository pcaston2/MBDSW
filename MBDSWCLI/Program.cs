using MBDSW;
using System;
using System.Collections.Generic;

namespace MBDSWCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var manager = new ServerManager(logger);
            logger.Log("Minecraft Bedrock Dedicated Server Wrapper Command Line Integration");
            logger.Log("Type 'help' for a list of commands");
            while (true)
            {
                var command = (Console.ReadLine() ?? String.Empty).ToLower();

                switch (command)
                {
                    case "update":
                        manager.Update();
                        break;
                    case "start":
                        manager.Start();
                        break;
                    case "stop":
                        manager.Stop();
                        break;
                    case "backup":
                        manager.Backup();
                        break;
                    case "restore":
                        manager.Restore();
                        break;
                    case "help":
                        var commands = new List<string>() {
                            "update",
                            "start",
                            "stop",
                            "send",
                            "backup",
                            "restore",
                            "listbackups",
                            "kill",
                            "help",
                            "exit",
                        };
                        logger.Log("List of commands:");
                        logger.Log(String.Join(Environment.NewLine, commands));
                        break;
                    case "kill":
                        manager.KillAll();
                        break;
                    case "exit":
                        manager.Stop();
                        manager.Wait();
                        return;
                    case "listbackups":
                        var backups = manager.ListBackups();
                        logger.Log("Backups:");
                        backups.ForEach(b => logger.Log(b));
                        break;
                    default:
                        if (command.StartsWith("send"))
                        {
                            if (command == "send")
                            {

                                logger.Log("You must provide more parameters to send");
                            } else
                            {
                                var serverMessage = command.Replace("send ", "");
                                manager.Send(serverMessage);
                            }
                        } else
                        {
                            logger.Log("Command not recognized. Type 'help' for a list of valid commands.");
                        }
                        break;
                }
                logger.Log("MBDSW command complete.");
            }
        }
    }
}
