using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MBDSW
{
    public class ServerManager : IDisposable
    {
        private Process _server;
        private StreamWriter _writer;
        private AutoResetEvent _res = new AutoResetEvent(false);
        private ServerState _state = ServerState.NeverStarted;
        private BackupState _backupState = BackupState.Ready;
        public const string BackupFolder = "backups";
        private ILog _logger;

        public ServerManager(ILog logger = null)
        {
            if (logger == null)
            {
                _logger = new DebugLogger();
            } else
            {
                _logger = logger;
            }
        }
        public enum ServerState
        {
            NeverStarted,
            Starting,
            Running,
            Stopped,
            StopRequested,
            Stopping,
            Killed,
        }

        public bool runningState => !(_state == ServerState.NeverStarted || _state == ServerState.Stopped || _state == ServerState.Killed);

        public bool isRunning => _server != null && _server.HasExited == false && runningState;

        public enum BackupState 
        { 
            Ready,
            BackupRequested,
            Saving,
            Hold,
        }

        public DateTime? LastBackup = null;


        public void Backup()
        {
            if (_state == ServerState.Running)
            {
                if (_backupState == BackupState.Ready)
                {
                    _backupState = BackupState.BackupRequested;
                    Send("save hold");
                    if (WaitForBackup(BackupState.Saving, 5000))
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Send("save query");
                            var result = WaitForBackup(BackupState.Hold, 1000);
                            if (result == true)
                            {
                                return;
                            }
                        }
                    };
                } else
                {
                    Write("Backup status is in the wrong state: " + Enum.GetName(typeof(BackupState), _backupState));
                }
            } else
            {
                Write("Can't back up when not running. Server is in wrong state: " + Enum.GetName(typeof(ServerState), _state));
            }
        }

        public List<string> ListBackups()
        {
            return Directory.EnumerateDirectories("backups")
                .OrderByDescending(s => s)
                .ToList();
        }

        public void Restore()
        {
            if (!isRunning)
            {
                var restoreName = ListBackups().FirstOrDefault();
                if (restoreName == null)
                {
                    Write("No backups available to restore");
                } else
                {
                    Write("Restoring from backup '" + restoreName + "'");
                    var files = Directory.GetFiles(restoreName, "*", SearchOption.AllDirectories).ToList();
                    files = files.Select(s => s.Replace(restoreName, String.Empty)).ToList();
                    try
                    {                    
                        foreach (var file in files)
                        {
                            Write("Restoring file '" + file + "'");
                            var oldPath = Path.Join(restoreName, file);
                            var newPath = Path.Join("bds", "worlds", file);
                            (new FileInfo(newPath)).Directory.Create();
                            File.Copy(oldPath, newPath, true);
                        }
                    } catch (Exception ex)
                    {
                        Write("Failed to restore from backup: " + ex.Message);
                    }
                }
            } else
            {
                Write("Can't restore while running. Server is in wrong state: " + Enum.GetName(typeof(ServerState), _state));
            }
        }

        public void KillAll()
        {
            var count = Updater.KillAllServers();
            if (count == 0)
            {
                Write("No servers appear to be running.");
            }
            else
            {
                Write("Killed " + count + " running servers.");
            }
        }

        public void Send(string command)
        {
            if (_state == ServerState.Running)
            {
                Write("Sending command: " + command);
                _writer.WriteLine(command);
            } else
            {
                Write("Command '" + command + "' was not executed, the server is in the wrong state: " + Enum.GetName(typeof(ServerState), _state), true);
            }
        }

        public void Stop()
        {
            if (isRunning)
            {
                try
                {
                    Send("stop");
                    var stopped = WaitUntil(ServerState.Stopped, 5000);
                    if (!stopped)
                    {
                        Write("Server couldn't be stopped in time, killing process", true);
                        if (_state != ServerState.Stopped)
                        {
                            _server.Kill(true);
                            _state = ServerState.Killed;
                        }
                    }
                }
                finally
                {
                    _server.Kill(true);
                }
            } else
            {
                Write("Can't stop server, it is not running");
            }
        }


        public ServerState State => _state;
        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            if (Updater.IsInstalled())
            {
                if (!isRunning)
                {
                    _server = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            UseShellExecute = false,
                            FileName = Path.Combine(Updater.serverPath, "bedrock_server.exe"),
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Maximized,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                        },
                        EnableRaisingEvents = true,
                    };
                    _server.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                        {
                            if (e.Data != null)
                            {
                                Write("(SERVER) " + e.Data);
                            }
                        });
                    _server.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                        {
                            if (e.Data != null)
                            {
                                Write("(SERVER) " + e.Data, true);
                            }
                        });
                    _server.Exited += new EventHandler((sender, e) =>
                        {
                            Write("Server process has exited with code " + _server.ExitCode);
                            _state = ServerState.Stopped;
                        });
                    _server.Start();
                    _writer = _server.StandardInput;
                    _state = ServerState.Starting;

                    _server.BeginOutputReadLine();
                    _server.BeginErrorReadLine();
                } else
                {
                    Write("Server is in wrong state: " + Enum.GetName(typeof(ServerState), _state));
                }
            } else
            {
                Write("No installation could be found, consider running an update");
            }
        }

        public void Update()
        {
            if (isRunning)
            {
                Write("The server is running, stop it before updating");
            } else
            {
                if (Updater.NeedsUpdate())
                {
                    var urlToDownload = Updater.GetNewFilename();
                    Write("Found new version: " + urlToDownload);
                    var download = Updater.GetUpdate(urlToDownload);
                    Write("Update downloaded " + download.Length + " bytes");
                    Write("Extracting files, this make take a while...");
                    Updater.Extract(download);
                    Updater.SetCurrentFilename(urlToDownload);
                    Write("Update complete");
                } else
                {
                    Write("No updates required");
                }
            }
        }

        public void Write(String message, bool error = false)
        {
            if (message != null)
            {

                if (_logger != null)
                {
                    _logger.Log((error ? "[ERROR] " : "") + message);
                }
                switch (message) {
                    case "(SERVER) [INFO] Server started.":
                        _state = ServerState.Running;
                        break;
                    case "(SERVER) [INFO] Server stop requested.":
                        _state = ServerState.StopRequested;
                        break;
                    case "(SERVER) [INFO] Stopping server...":
                        _state = ServerState.Stopping;
                        break;
                    case "(SERVER) Quit correctly":
                        _state = ServerState.Stopped;
                        break;
                    case "(SERVER) Saving...":
                        if (_backupState == BackupState.BackupRequested)
                        {
                            _backupState = BackupState.Saving;
                        }
                        break;
                    case "(SERVER) Changes to the level are resumed.":
                        _backupState = BackupState.Ready;
                        break;
                }
                if (message.StartsWith("(SERVER) Data saved."))
                {
                    if (_backupState == BackupState.Saving)
                    {
                        _backupState = BackupState.Hold;
                    }
                } else if (message.StartsWith("(SERVER)") && message.Contains("db/CURRENT"))
                {
                    if (_backupState == BackupState.Hold)
                    {
                        var backupFiles = ParseBackupFilePaths(message.Replace("(SERVER) ", ""));
                        var backupSuccess = BackupFiles(backupFiles);
                        Send("save resume");
                        if (backupSuccess)
                        {
                            Write("Backup was successful");
                            LastBackup = DateTime.Now;
                        }
                        else
                        {
                            Write("Backup failed", true);
                        }
                    }
                }
            }
        }

        private bool BackupFiles(Dictionary<string, int> backupFiles)
        {
            var backupFolder = Path.Combine(BackupFolder, DateTime.Now.ToString("yyyy_dd_MM HH_mm_ss"));
            Directory.CreateDirectory(backupFolder);
            int tries = 3;
            foreach (var file in backupFiles)
            {
                Write("Backing up '" + file.Key + "'");
                bool success = true;
                for (int i = 0; i < tries; i++)
                {
                    try
                    {
                        var path = Path.Combine(Updater.serverPath, "worlds", file.Key);
                        var content = new byte[file.Value];
                        //if (file.Value > 0)
                        //{
                        //    using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        //    {
                        //        fileStream.Read(content, 0, file.Value);
                        //    }
                        //}
                        var newPath = Path.Combine(backupFolder, file.Key);
                        (new FileInfo(newPath)).Directory.Create();
                        File.Copy(path, newPath);
                        using (var fileStream = new FileStream(newPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            fileStream.Read(content, 0, file.Value);
                        }
                        File.WriteAllBytes(newPath, content);
                        break;
                    } catch (Exception ex)
                    {
                        success = false;
                        Write("Wasn't able to back up '" + file.Key + "': " + ex.Message, true);
                        Sleep(1000);
                        Write("Retrying (" + (i + 1) + "/" + tries + ")...");
                    }
                }
                if (!success)
                {
                    Write("Failed to backup '" + file.Key + "'", true);
                    Directory.Delete(backupFolder, true);
                    return false;
                }
            }
            return true;
        }

        private Dictionary<string, int> ParseBackupFilePaths(string message)
        {
            var result = new Dictionary<string, int>();
            var files = message.Split(", ");
            foreach (var file in files)
            {
                var namesAndLength = file.Split(":");
                var name = namesAndLength.First();
                var length = int.Parse(namesAndLength.Last());
                result.Add(name, length);
            }
            return result;
        }

        public int Wait()
        {
            if (isRunning)
            {
                _server.WaitForExit();
                return _server.ExitCode;
            } else
            {
                return 0;
            }
        }

        public bool WaitWhile(ServerState state, int? timeout)
        {
            Stopwatch sw = null;
            if (timeout != null)
            {
                sw = new Stopwatch();
                sw.Start();
            }
            while (_state == state)
            {                
                if (sw != null)
                {
                    if (sw.ElapsedMilliseconds > timeout)
                    {
                        return false;
                    }
                }
                Sleep();

            }
            return true;
        }
        public bool WaitUntil(ServerState state, int? timeout = null)
        {
            Stopwatch sw = null;
            if (timeout != null)
            {
                sw = new Stopwatch();
                sw.Start();
            }
            while (_state != state)
            {
                if (sw != null)
                {
                    if (sw.ElapsedMilliseconds > timeout)
                    {
                        return false;
                    }
                }
                Sleep();

            }
            return true;
        }

        public bool WaitForBackup(BackupState backupState = BackupState.Ready, int? timeout = null)
        {
            Stopwatch sw = null;
            if (timeout != null)
            {
                sw = new Stopwatch();
                sw.Start();
            }
            while (_backupState != backupState)
            {
                if (sw != null)
                {
                    if (sw.ElapsedMilliseconds > timeout)
                    {
                        return false;
                    }
                }
                Sleep();
            }
            return true;
        }

        private void Sleep(int miliseconds = 100)
        {
            System.Threading.Thread.Sleep(miliseconds);
        }
    }
}
