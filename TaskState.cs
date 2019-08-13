using System;
using System.Collections.Generic;
using System.Text;

namespace StackDeployCheck
{
    public enum TaskState
    {
        Unknown = 0,
        New, // The task was initialized.
        Pending, // Resources for the task were allocated.
        Assigned, // Docker assigned the task to nodes.
        Accepted, // The task was accepted by a worker node. If a worker node rejects the task, the state changes to REJECTED.
        Preparing, // Docker is preparing the task.
        Starting, // Docker is starting the task.
        Running, // The task is executing.
        Complete, // The task exited without an error code.
        Failed, // The task exited with an error code.
        Shutdown, // Docker requested the task to shut down.
        Rejected, // The worker node rejected the task.
        Orphaned, // The node was down for too long.
        Remove, // The task is not terminal but the associated service was removed or scaled down.
        Ready // Desired state
    }
}
