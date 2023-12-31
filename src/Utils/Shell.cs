using System;
using System.Diagnostics;

namespace Keepix.SmartNodePlugin.Utils
{
    public class ShellCondition
    {
        public string content;
        public string[] answers;
    }

    public static class Shell
    {
        public static string ExecuteCommand(string command, List<ShellCondition>? conditions = null, int waitForExit = -1)
        {
            var processInfo = new ProcessStartInfo("bash", $"-c \"{command}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardInput = true, // Enable input redirection
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var process = new Process
            {
                StartInfo = processInfo
            };

            process.Start();
            bool forceExit = false;
            string output = string.Empty;
            while (!forceExit && !process.StandardOutput.EndOfStream) {
                // we just need to make sure we catch the before last line else we hang here
                string? line = process.StandardOutput.ReadLine();
                if (conditions != null && line != null) {
                    var conditionMet = conditions.FirstOrDefault(x => line.Contains(x.content));
                    if (conditionMet != null) {
                        foreach(var answer in conditionMet.answers) {
                            if (answer.Length > 0 && answer == "[STOP_PROCESS]") {
                                process.Kill();
                                forceExit = true;
                                break ;
                            }
                            else if (answer.Length > 0) process.StandardInput.WriteLine(answer);
                            else process.StandardInput.WriteLine(); // in case we just need to send an empty entry to be processed
                            Thread.Sleep(500);
                        }
                        Thread.Sleep(1500); // once we met a condition we let time to terminal for processing the next conditions
                    }
                }
                output += string.IsNullOrEmpty(output) ? line : "\n" + line;
            }

            process.WaitForExit(TimeSpan.FromMinutes(5));
            if (process.ExitCode != 0 && process.ExitCode != 137) // 137 equals forced exit.
            {
                throw new Exception(output);
            }
            return output;
        }
    }
}
