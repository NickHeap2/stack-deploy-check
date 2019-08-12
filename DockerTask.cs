using System;
using System.Collections.Generic;
using System.Text;

namespace StackDeployCheck
{
    public class DockerTask
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }

        public bool IsCurrent { get; internal set; }
        public string Image { get; internal set; }
        public string Node { get; internal set; }
        public string DesiredState { get; internal set; }
        public string CurrentState { get; internal set; }
        public string Error { get; internal set; }
    }
}
