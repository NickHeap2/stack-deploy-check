using System.Collections.Generic;

namespace StackDeployCheck
{
    public enum UpdateState
    {
        Unknown,
        Completed,
        Updating,
        RollbackStarted,
        RollbackCompleted,
        Paused,
        NotUpdated,
        Rejected
    }
}