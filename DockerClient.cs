using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            GetStackServices(stackName);

            foreach (var service in currentStackState.Services)
            {
                GetServiceStates(service.Name);
            }


            return currentStackState;
        }

        private void GetServiceStates(string serviceName)
        {
            var formatString = "{{.Spec.Name}} " +
                "{{if not .UpdateStatus}}" +
                  "NotUpdated" +
                "{{else}}" +
                  "{{if eq .UpdateStatus.State \\\"rollback_started\\\"}}" +
                    "RollbackStarted" +
                  "{{else if eq .UpdateStatus.State \\\"rollback_completed\\\"}}" +
                    "RollbackCompleted" +
                  "{{else}}" +
                    "{{.UpdateStatus.State}}" +
                  "{{end}}" +
                "{{end}}";
            ProcessStartInfo processStartInfo = new ProcessStartInfo("docker", $"service inspect {serviceName} --format \"{formatString}\"")
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
                process.OutputDataReceived += Process_OutputServiceStateDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                //process.Exited += Process_Exited;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                //process.WaitForExit(waitTimeout);
                process.WaitForExit();
            }
        }

        private void Process_OutputServiceStateDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == String.Empty
                || e.Data == null)
            {
                return;
            }

            string currentLine = e.Data;

            if (currentLine.StartsWith("Status: Template parsing error", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var serviceStateElements = currentLine.Split(" ");
            if (serviceStateElements.Length < 2)
            {
                return;
            }

            // "{{.Spec.Name}} {{.UpdateStatus.State}} {{.UpdateStatus.CompletedAt}} {{.UpdateStatus.Message}}";

            string name = serviceStateElements[0];
            string state = serviceStateElements[1];

            if (state.Equals("completed_rollback"))
            {
                state = "RolledBack";
            }

            var service = currentStackState.Services.FirstOrDefault<DockerService>(ds => ds.Name == name);
            if (service != null)
            {
                UpdateState updateState;
                if (Enum.TryParse<UpdateState>(state, true, out updateState))
                {
                    service.UpdateState = updateState;
                }
                else
                {
                    Console.Error.WriteLine($"ERROR: unable to parse service update state {state}");
                    service.UpdateState = UpdateState.Unknown;
                }
            }
        }

        public void GetStackTasks(string stackName)
        {
            var formatString = "{{.ID}}{{\\\"\\x00\\\"}}{{.Name}}{{\\\"\\x00\\\"}}{{.Image}}{{\\\"\\x00\\\"}}{{.Node}}{{\\\"\\x00\\\"}}{{.DesiredState}}{{\\\"\\x00\\\"}}{{.CurrentState}}{{\\\"\\x00\\\"}}{{.Error}}";
            ProcessStartInfo processStartInfo = new ProcessStartInfo("docker", $"stack ps {stackName} --no-trunc --format \"{formatString}\"")
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
                process.OutputDataReceived += Process_OutputTasksDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                //process.Exited += Process_Exited;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                //process.WaitForExit(waitTimeout);
                process.WaitForExit();
            }
        }

        private void Process_OutputTasksDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == String.Empty
                || e.Data == null)
            {
                return;
            }

            var taskElements = e.Data.Split('\x00');
            if (taskElements.Length < 7)
            {
                return;
            }

            var dockerTask = new DockerTask()
            {
                Id = taskElements[0],
                Name = taskElements[1],
                Image = taskElements[2],
                Node = taskElements[3],
                Error = taskElements[6]
            };
            dockerTask.SetDesiredState(taskElements[4]);
            dockerTask.SetCurrentState(taskElements[5]);

            currentStackState.Tasks.Add(dockerTask);

            //if (e.Data.StartsWith("ID "))
            //{
            //    //headerLine = e.Data;
            //    return;
            //}
            //string currentLine = e.Data;

            //var currentStart = currentLine.IndexOf("[") + 1;
            //var currentEnd = currentLine.IndexOf("]");
            //var errorStart = currentLine.IndexOf("[", currentEnd) + 1;
            //var errorEnd = currentLine.LastIndexOf("]");

            //var taskElements = e.Data.Split(" ");
            //if (taskElements.Length < 5)
            //{
            //    return;
            //}

            //var dockerTask = new DockerTask()
            //{
            //    Id = taskElements[0],
            //    Name = taskElements[1],
            //    Image = taskElements[2],
            //    Node = taskElements[3],
            //    Error = currentLine.Substring(errorStart, errorEnd - errorStart)
            //};
            //dockerTask.SetDesiredState(taskElements[4]);
            //dockerTask.SetCurrentState(currentLine.Substring(currentStart, currentEnd - currentStart));

            //currentStackState.Tasks.Add(dockerTask);

            //Console.Error.WriteLineAsync($"OUT: {e.Data}");
            //Console.Out.WriteLine(dockerTask.Id);
            //Console.Out.WriteLine($"{dockerTask.IsCurrent} {dockerTask.Name}");
            //Console.Out.WriteLine(dockerTask.Image);
            //Console.Out.WriteLine(dockerTask.Node);
            //Console.Out.WriteLine(dockerTask.DesiredState);
            //Console.Out.WriteLine(dockerTask.CurrentState);
            //Console.Out.WriteLine(dockerTask.Error);
            //Console.Out.WriteLine("--------------------------------------------------");
        }
        public void GetStackServices(string stackName)
        {
            var formatString = "{{.ID}}{{\\\"\\x00\\\"}}{{.Name}}{{\\\"\\x00\\\"}}{{.Mode}}{{\\\"\\x00\\\"}}{{.Replicas}}{{\\\"\\x00\\\"}}{{.Image}}{{\\\"\\x00\\\"}}{{.Ports}}";
            ProcessStartInfo processStartInfo = new ProcessStartInfo("docker", $"stack services {stackName} --format \"{formatString}\"")
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
                process.OutputDataReceived += Process_OutputServicesDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                //process.Exited += Process_Exited;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                //process.WaitForExit(waitTimeout);
                process.WaitForExit();
            }

            //
        }

        private void Process_OutputServicesDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == String.Empty
                || e.Data == null)
            {
                return;
            }
            string currentLine = e.Data;
            var serviceElements = currentLine.Split("\x00");
            if (serviceElements.Length < 6)
            {
                return;
            }

            var dockerService = new DockerService()
            {
                Id = serviceElements[0],
                Name = serviceElements[1],
                Mode = serviceElements[2],
                Image = serviceElements[4],
                Ports = serviceElements[5]
            };
            dockerService.SetReplicas(serviceElements[3]);
            currentStackState.Services.Add(dockerService);
        }
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                // don't log if no tasks found
                if (e.Data.StartsWith("nothing found in stack", StringComparison.InvariantCultureIgnoreCase)
                    || e.Data.StartsWith("Status: Template parsing error: template: :1:30: executing", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
                Console.Out.WriteLineAsync($"ERROR FROM DOCKER: {e.Data}");
            }
        }


    }
}
