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

        }

        private void Send(string command)
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
            } finally
            {
                _server.Kill(true);
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
                    }
                };
                _server.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                    {
                        Write(e.Data);
                    });
                _server.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                    {
                        Write(e.Data, true);
                    });
                _server.Start();
                _writer = _server.StandardInput;
                _state = ServerState.Starting;
                
                _server.BeginOutputReadLine();
                _server.BeginErrorReadLine();
            } else
            {
                Write("No installation could be found, consider running an update");
            }
        }

        public void Write(String message, bool error = false)
        {
            if (message != null)
            {
                switch (message) {
                    case "[INFO] Server started.":
                        _state = ServerState.Running;
                        break;
                    case "[INFO] Server stop requested.":
                        _state = ServerState.StopRequested;
                        break;
                    case "[INFO] Stopping server...":
                        _state = ServerState.Stopping;
                        break;
                    case "Quit correctly":
                        _state = ServerState.Stopped;
                        break;
                    case "Saving...":
                        _backupState = BackupState.Saving;
                        break;
                    case "Changes to the level are resumed.":
                        _backupState = BackupState.Ready;
                        LastBackup = DateTime.Now;
                        break;
                }
                if (message.StartsWith("Data saved."))
                {
                    _backupState = BackupState.Hold;
                } else if (message.Contains("db/CURRENT"))
                {
                    var backupFiles = ParseBackupFilePaths(message);
                    BackupFiles(backupFiles);
                    Send("save resume");
                }
                Debug.WriteLine((error ? "[ERROR] " : "") + message);
            }
        }

        private void BackupFiles(Dictionary<string, int> backupFiles)
        {
            var backupFolder = Path.Combine(BackupFolder, DateTime.Now.ToString("yyyy_dd_MM HH_mm_ss"));
            Directory.CreateDirectory(backupFolder);
            foreach (var file in backupFiles)
            {
                bool success = false;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        var path = Path.Combine(Updater.serverPath, "worlds", file.Key);
                        var content = new byte[file.Value];
                        using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            fileStream.Read(content, 0, file.Value);
                        }
                        var newPath = Path.Combine(backupFolder, file.Key);
                        (new FileInfo(newPath)).Directory.Create();
                        File.WriteAllBytes(newPath, content);
                        success = true;
                    } catch (Exception)
                    {
                        Write("Wasn't able to back up '" + file.Key + "'... Retrying...", true);
                    }
                }
                if (!success)
                {
                    Write("Failed to write all files", true);
                    Directory.Delete(backupFolder, true);
                    return;
                }
                
            }
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
            _server.WaitForExit();
            return _server.ExitCode;
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
