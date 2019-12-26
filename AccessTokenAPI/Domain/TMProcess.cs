using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ephemeral.AccessTokenAPI.Exceptions;

namespace Ephemeral.AccessTokenAPI.Domain
{
    public class TMProcess
    {
        public string ProcessName { get; }
        public int ProcessId { get; }

        public Pipe Pipe { get; }

        public TMProcess(Process process, Pipe pipe = null)
        {
            this.ProcessName = process.ProcessName;
            this.ProcessId = process.Id;
            this.Pipe = pipe;
        }

        private TMProcess(string processName, int pid, Pipe pipe = null)
        {
            this.ProcessName = processName;
            this.ProcessId = pid;
            this.Pipe = pipe;
        }

        public static List<TMProcess> GetProcessByName(string name)
        {
            Process[] processes = Process.GetProcessesByName(name);
            return processes.Select(x => new TMProcess(x)).ToList();
        }

        public static TMProcess GetProcessById(int pid)
        {
            Process p = Process.GetProcessById(pid);
            if (p == null)
                throw new ProcessNotFoundException();

            return new TMProcess(p);
        }

        public static List<TMProcess> GetAllProcesses()
        {
            List<Process> processes = new List<Process>(Process.GetProcesses());
            return processes.Select(x => new TMProcess(x)).ToList();
        }

        public static TMProcess FromValues(string processName, int pid, Pipe pipe = null)
        {
            return new TMProcess(processName, pid, pipe);
        }
    }
}
