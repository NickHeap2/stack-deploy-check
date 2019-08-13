using System;
using System.Collections.Generic;
using System.Text;

namespace StackDeployCheck
{
    public class DockerService
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public string Mode { get; internal set; }
        public string ReplicasDesc { get; internal set; }
        public int CurrentReplicas { get; internal set; }
        public int DesiredReplicas { get; internal set; }
        public string Image { get; internal set; }
        public string Ports { get; internal set; }
        public UpdateState UpdateState { get; internal set; }
        public string LastUpdated { get; internal set; }
        public string UpdateMessage { get; internal set; }

        public int TotalTasks { get; internal set; }
        public int StableTasks { get; internal set; }
        public int CompleteTasks { get; internal set; }

        public DockerService()
        {
            UpdateState = UpdateState.NotUpdated;
        }

        public void SetReplicas(string replicasString)
        {
            ReplicasDesc = replicasString;
            var parts = replicasString.Split(@"/");
            CurrentReplicas = 0;
            DesiredReplicas = 0;
            int current;
            if (!int.TryParse(parts[0], out current))
            {
                Console.Error.WriteLine($"ERROR: unable to parse current replicas {replicasString}");
                return;
            }
            CurrentReplicas = current;
            
            int desired;
            if (!int.TryParse(parts[1], out desired))
            {
                Console.Error.WriteLine($"ERROR: unable to parse desired replicas {replicasString}");
                return;
            }
            DesiredReplicas = desired;
        }
    }
}
