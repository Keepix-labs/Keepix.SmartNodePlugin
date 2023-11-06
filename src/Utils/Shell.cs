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
        public static string ExecuteCommand(string command, List<ShellCondition>? conditions = null)
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
            string output = string.Empty;
            if (conditions != null && conditions.Count > 0)
            {
                foreach (var condition in conditions)
                {
                    var retry = 0;
                    output += process.StandardOutput.ReadLine();
                    while (!output.Contains(condition.content) && retry < 100)
                    {
                        foreach (var answer in condition.answers) {

                            if (answer.Length > 0) {
                                process.StandardInput.WriteLine(answer);
                            }
                            else
                            {
                                process.StandardInput.WriteLine(); // in case we just need to send an empty entry to be processed
                            }
 
                            Thread.Sleep(500);
                        }

                        Thread.Sleep(1000);
                        retry++;
                    }
                }
                // After sending tasks, we add a delay to ensure the input is processed
                Thread.Sleep(1000);
            }

            output += process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception(output);
            }

            return output;
        }
    }
}
