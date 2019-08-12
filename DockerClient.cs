using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StackDeployCheck
{
    class DockerClient
    {
        private StackState currentStackState;
        public StackState GetCurrentState(string stackName)
        {
            currentStackState = new StackState()
            {
                StackName = stackName
            };

            GetStackTasks(stackName);

            return currentStackState;
        }

        public void GetStackTasks(string stackName)
        {
            var formatString = "{{.ID}} {{.Name}} {{.Image}} {{.Node}} {{.DesiredState}} [{{.CurrentState}}] [{{.Error}}]";
            ProcessStartInfo processStartInfo = new ProcessStartInfo("docker", $"stack ps {stackName}  --no-trunc --format \"{formatString}\"")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process process = new Process()
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            })
            {
                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                //process.Exited += Process_Exited;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                //process.WaitForExit(waitTimeout);
                process.WaitForExit();
            }
        }


        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                Console.Error.WriteLineAsync($"ERROR: {e.Data}");
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == String.Empty
                || e.Data == null)
            {
                return;
            }
            //if (e.Data.StartsWith("ID "))
            //{
            //    //headerLine = e.Data;
            //    return;
            //}
            string currentLine = e.Data;

            var currentStart = currentLine.IndexOf("[") + 1;
            var currentEnd = currentLine.IndexOf("]");
            var errorStart = currentLine.IndexOf("[", currentEnd) + 1;
            var errorEnd = currentLine.LastIndexOf("]");

            var taskElements = e.Data.Split(" ");
            var dockerTask = new DockerTask()
            {
                Id = taskElements[0],
                Name = taskElements[1],
                Image = taskElements[2],
                Node = taskElements[3],
                DesiredState = taskElements[4],
                CurrentState = currentLine.Substring(currentStart, currentEnd - currentStart),
                Error = currentLine.Substring(errorStart, errorEnd - errorStart)
            };

            if (dockerTask.DesiredState == "Ready"
                || dockerTask.DesiredState == "Running")
            {
                dockerTask.IsCurrent = true;
            }
            currentStackState.Tasks.Add(dockerTask);

            //Console.Error.WriteLineAsync($"OUT: {e.Data}");
            //Console.Out.WriteLine(dockerTask.Id);
            //Console.Out.WriteLine($"{dockerTask.IsCurrent} {dockerTask.Name}");
            //Console.Out.WriteLine(dockerTask.Image);
            //Console.Out.WriteLine(dockerTask.Node);
            //Console.Out.WriteLine(dockerTask.DesiredState);
            //Console.Out.WriteLine(dockerTask.CurrentState);
            //Console.Out.WriteLine(dockerTask.Error);
            //Console.Out.WriteLine("");
        }
    }
}
