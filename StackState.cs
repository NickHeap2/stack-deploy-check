using System.Collections.Generic;

namespace StackDeployCheck
{
    public class StackState
    {
        public string StackName { get; set; }

        public List<DockerTask> Tasks { get; set; }
        public List<DockerService> Services { get; set; }
        public int TotalTasks { get; internal set; }
        public int StableTasks { get; internal set; }
        public int CompleteTasks { get; internal set; }

        public int TotalServices { get; internal set; }
        public int UpdatedServices { get; internal set; }
        public int FailedServices { get; internal set; }

        public StackState()
        {
            Tasks = new List<DockerTask>();
            Services = new List<DockerService>();
        }
    }
}