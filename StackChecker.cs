using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace StackDeployCheck
{
    class StackChecker
    {
        public string StackName { get; set; }
        //public List<TaskState> DesiredStates { get; set; }

        public bool AwaitDesiredStates2(int stableTime)
        {
            var stableTimeSpan = TimeSpan.FromMilliseconds(stableTime);
            var dockerClient = new DockerClient();
            while (true)
            {
                Console.Out.WriteLine($"Checking stack {StackName} for desired state...");
                var stackState = dockerClient.GetCurrentState(StackName);

                //stackState.TotalTasks = 0;
                //stackState.CompleteTasks = 0;
                //stackState.StableTasks = 0;
                foreach (var service in stackState.Services)
                {
                    stackState.TotalServices++;

                    var serviceLine = $"  Service [{service.Name}] [{service.ReplicasDesc}] in update state [{service.UpdateState}] for image [{service.Image}]";
                    Console.Out.WriteLine(serviceLine);

                    if (service.UpdateState == UpdateState.Completed
                        || service.UpdateState == UpdateState.NotUpdated
                        || service.UpdateState == UpdateState.RollbackCompleted
                        || service.UpdateState == UpdateState.Paused)
                    {
                        stackState.UpdatedServices++;
                    }

                    //service.TotalTasks = 0;
                    //service.StableTasks = 0;
                    //service.CompleteTasks = 0;
                    foreach (var task in stackState.Tasks.Where(t => t.Name.StartsWith($"{service.Name}.")))
                    {
                        if (!task.IsCurrent) continue;

                        service.TotalTasks++;
                        if (task.CurrentState == TaskState.Running)
                        {
                            service.CompleteTasks++;
                        }
                        if (task.TimeInState >= stableTimeSpan)
                        {
                            service.StableTasks++;
                        }

                        var statusLine = $"    Task [{task.Name}] on node [{task.Node}] in state [{task.CurrentState}] for [{task.TimeInStateDesc}] for image [{task.Image}]";
                        Console.Out.WriteLine(statusLine);
                        if (!string.IsNullOrEmpty(task.Error))
                        {
                            var errorLine = $"      Error: [{task.Error}]";
                            Console.Out.WriteLine(errorLine);
                        }
                    }
                    if (service.TotalTasks == 0)
                    {
                        Console.Out.WriteLine($"    No tasks found for service!");
                    }
                    else
                    {
                        Console.Out.WriteLine($"    [{service.CompleteTasks} of {service.TotalTasks}] tasks are running, [{service.StableTasks} of {service.TotalTasks}] tasks are stable.");
                    }
                    stackState.TotalTasks += service.TotalTasks;
                    stackState.CompleteTasks += service.CompleteTasks;
                    stackState.StableTasks += service.StableTasks;

                    if ((service.StableTasks >= service.TotalTasks
                        && service.CompleteTasks < service.TotalTasks)
                        || service.UpdateState == UpdateState.RollbackCompleted
                        || service.UpdateState == UpdateState.Paused)
                    {
                        stackState.FailedServices++;
                    }
                }
                if (stackState.TotalServices == 0)
                {
                    Console.Out.WriteLine($"  No services found for stack!");
                }
                else
                {
                    Console.Out.WriteLine($"  [{stackState.UpdatedServices} of {stackState.TotalServices}] services are updated, [{stackState.FailedServices} of {stackState.TotalServices}] services failed.");
                    //if (failedServices > 0)
                    //{
                    //    Console.Out.WriteLine($"  [{failedServices} of {totalServices}] services failed.");
                    //}
                }


                if (stackState.TotalTasks > 0
                    && stackState.TotalServices > 0
                    //&& stackState.CompleteTasks >= stackTotalTasks
                    && stackState.StableTasks >= stackState.TotalTasks
                    && stackState.UpdatedServices >= stackState.TotalServices)
                {
                    if (stackState.CompleteTasks < stackState.TotalTasks
                        || stackState.FailedServices > 0)
                    {
                        return false;
                    }
                    return true;
                }

                Console.Out.WriteLine();
                Thread.Sleep(5000);
            }
        }

        //public bool AwaitDesiredStates(int stableTime)
        //{
        //    var stableTimeSpan = TimeSpan.FromMilliseconds(stableTime);
        //    var dockerClient = new DockerClient();
        //    while (true)
        //    {
        //        Console.Out.WriteLine($"Checking stack {StackName} for desired state...");
        //        var stackState = dockerClient.GetCurrentState(StackName);

        //        var totalTasks = 0;
        //        var completeTasks = 0;
        //        var stableTasks = 0;
        //        foreach (var task in stackState.Tasks)
        //        {
        //            if (!task.IsCurrent) continue;

        //            totalTasks++;
        //            if (task.CurrentState == TaskState.Running)
        //            {
        //                completeTasks++;
        //                if (task.TimeInState >= stableTimeSpan)
        //                {
        //                    stableTasks++;
        //                }
        //            }

        //            var statusLine = $"  Task [{task.Name}] on node [{task.Node}] in state [{task.CurrentState}] for [{task.TimeInStateDesc}] for image [{task.Image}]";
        //            Console.Out.WriteLine(statusLine);
        //            if (!string.IsNullOrEmpty(task.Error))
        //            {
        //                var errorLine = $"      Error: [{task.Error}]";
        //                Console.Out.WriteLine(errorLine);
        //            }
        //        }
        //        if (totalTasks == 0)
        //        {
        //            Console.Out.WriteLine($"No tasks found for stack!");
        //        }
        //        else
        //        {
        //            Console.Out.WriteLine($"{completeTasks} of {totalTasks} tasks are running, {stableTasks} of {totalTasks} tasks are stable.");
        //        }

        //        var totalServices = 0;
        //        var updatedServices = 0;
        //        var failedServices = 0;
        //        foreach (var service in stackState.Services)
        //        {
        //            totalServices++;

        //            var serviceLine = $"  Service [{service.Name}] [{service.ReplicasDesc}] in update state [{service.UpdateState}] for image [{service.Image}]";
        //            Console.Out.WriteLine(serviceLine);

        //            Console.Out.WriteLine(service.UpdateState);
        //            Console.Out.WriteLine(UpdateState.RollbackCompleted);
        //            if (service.UpdateState == UpdateState.Completed
        //                || service.UpdateState == UpdateState.NotUpdated
        //                || service.UpdateState == UpdateState.RollbackCompleted)
        //            {
        //                updatedServices++;
        //            }
        //            if (service.UpdateState == UpdateState.RollbackCompleted)
        //            {
        //                failedServices++;
        //            }
        //        }
        //        if (totalServices == 0)
        //        {
        //            Console.Out.WriteLine($"No services found for stack!");
        //        }
        //        else
        //        {
        //            Console.Out.WriteLine($"{updatedServices} of {totalServices} services are updated.");
        //            if (failedServices > 0)
        //            {
        //                Console.Out.WriteLine($"{failedServices} of {totalServices} services failed.");
        //            }
        //        }

        //        if (totalTasks > 0
        //            && totalServices > 0
        //            && completeTasks >= totalTasks
        //            && stableTasks >= totalTasks
        //            && updatedServices >= totalServices)
        //        {
        //            if (failedServices > 0)
        //            {
        //                return false;
        //            }
        //            return true;
        //        }

        //        Thread.Sleep(5000);
        //    }
        //}
    }
}
