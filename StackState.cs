using System.Collections.Generic;

namespace StackDeployCheck
{
    public class StackState
    {
        public string StackName { get; set; }

        public List<DockerTask> Tasks { get; set; }

        public StackState()
        {
            Tasks = new List<DockerTask>();
        }
    }
}