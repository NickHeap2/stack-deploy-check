using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace StackDeployCheck
{
    class StackChecker
    {
        public string StackName { get; set; }
        //public List<TaskState> DesiredStates { get; set; }

        public bool AwaitDesiredStates()
        {
            var dockerClient = new DockerClient();
            while (true)
            {
                Console.Out.WriteLine($"Checking stack {StackName} for desired state...");
                var stackState = dockerClient.GetCurrentState(StackName);

                var allTasksComplete = true;
                foreach (var task in stackState.Tasks)
                {
                    if (!task.IsCurrent) continue;
                    if (!task.CurrentState.StartsWith("Running"))
                    {
                        allTasksComplete = false;
                    }
                    Console.Out.WriteLine($"Task: {task.Name} State: {task.CurrentState} Image: {task.Image} Error: {task.Error}");
                }
                if (allTasksComplete)
                {
                    return true;
                }
                Thread.Sleep(1000);
            }
        }
    }
}
