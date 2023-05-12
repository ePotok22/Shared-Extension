using System;

namespace FFF.Shared.Utilities
{
    public sealed class EnvironmentUtilities
    {
        public static string GetEnvironmentVariable(string variableName, EnvironmentVariableTarget variableTarget)
        {
            switch (variableName?.ToLower().Trim())
            {
                case "tickcount": return Environment.TickCount.ToString();
                case "exitcode": return Environment.ExitCode.ToString();
                case "commandline": return Environment.CommandLine;
                case "currentlirectory": return Environment.CurrentDirectory;
                case "systemdirectory": return Environment.SystemDirectory;
                case "machinename": return Environment.MachineName;
                case "processorcount": return Environment.ProcessorCount.ToString();
                case "systempagesize": return Environment.SystemPageSize.ToString();
                case "newline": return Environment.NewLine;
                case "version": return Environment.Version.ToString();
                case "workingSst": return Environment.WorkingSet.ToString();
                case "osversion": return Environment.OSVersion.ToString();
                case "stacktrace": return Environment.StackTrace;
                case "is64bitprocess": return Environment.Is64BitProcess.ToString();
                case "is64bitoperatingsystem": return Environment.Is64BitOperatingSystem.ToString();
                case "hasshutdownstarted": return Environment.HasShutdownStarted.ToString();
                case "username": return Environment.UserName;
                case "userinteractive": return Environment.UserInteractive.ToString();
                case "userdomainname": return Environment.UserDomainName;
                case "currentmanagedthreadid": return Environment.CurrentManagedThreadId.ToString();
                default: return Environment.GetEnvironmentVariable(variableName, variableTarget);
            }
        }
    }
}
