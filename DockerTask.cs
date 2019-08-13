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
        public TaskState DesiredState { get; private set; }
        public TaskState CurrentState { get; private set; }
        public string CurrentStateDesc { get; private set; }
        public string Error { get; internal set; }
        public string TimeInStateDesc { get; private set; }
        public TimeSpan TimeInState { get; private set; }

        public void SetCurrentState(string currentState)
        {
            CurrentStateDesc = currentState;
            CurrentState = TaskState.Unknown;
            TimeInStateDesc = "";
            TimeInState = TimeSpan.MinValue;

            var currentStateParts = currentState.Split(" ");

            if (currentStateParts.Length < 3)
            {
                Console.Error.WriteLine($"ERROR: unable to parse current state time [{currentState}]!");
                return;
            }

            TaskState state;
            if (!Enum.TryParse<TaskState>(currentStateParts[0], out state))
            {
                Console.Error.WriteLine($"ERROR: unable to parse state [{currentStateParts[0]}]!");
                return;
            }
            CurrentState = state;

            var amountAt = 1;
            if (currentStateParts[amountAt].Equals("about", StringComparison.InvariantCultureIgnoreCase))
            {
                amountAt = 2;
            }
            if (currentStateParts[amountAt].Equals("less", StringComparison.InvariantCultureIgnoreCase))
            {
                amountAt = 3;
            }

            var amountString = currentStateParts[amountAt];
            var amount = 0;
            if (amountString.StartsWith("a", StringComparison.InvariantCultureIgnoreCase))
            {
                amountString = "1";
            }
            if (!int.TryParse(amountString, out amount))
            {
                Console.Error.WriteLine($"ERROR: unable to parse amount string [{amountString}]!");
                return;
            }

            var unit = currentStateParts[amountAt + 1];

            TimeInStateDesc = $"{amount} {unit}";

            if (unit.StartsWith("day", StringComparison.InvariantCultureIgnoreCase))
            {
                TimeInState = TimeSpan.FromDays(amount);
            }
            else if (unit.StartsWith("hour", StringComparison.InvariantCultureIgnoreCase))
            {
                TimeInState = TimeSpan.FromHours(amount);
            }
            else if (unit.StartsWith("minute", StringComparison.InvariantCultureIgnoreCase))
            {
                TimeInState = TimeSpan.FromMinutes(amount);
            }
            else if (unit.StartsWith("second", StringComparison.InvariantCultureIgnoreCase))
            {
                TimeInState = TimeSpan.FromSeconds(amount);
            }
        }

        internal void SetDesiredState(string desiredState)
        {
            TaskState state;
            if (!Enum.TryParse<TaskState>(desiredState, out state))
            {
                Console.Error.WriteLine($"ERROR: unable to parse state [{desiredState}]!");
                return;
            }
            DesiredState = state;

            if (DesiredState == TaskState.Ready
                || DesiredState == TaskState.Running)
            {
                IsCurrent = true;
            }
        }
    }
}
