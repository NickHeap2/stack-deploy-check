using System;
using System.Diagnostics;
using System.Timers;

namespace StackDeployCheck
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("dotnet stack-deploy-check {stackName} [timeout seconds] [stableTime seconds]");
                return -2;
            }

            var stackName = args[0];
            int timeout = 10000;
            int stableTime = 10000;
            if (args.Length > 1)
            {
                if (!int.TryParse(args[1], out timeout))
                {
                    Console.Error.WriteLine("ERROR: Invalid timeout!");
                    return -3;
                }
                timeout = timeout * 1000;
            }
            if (args.Length > 2)
            {
                if (!int.TryParse(args[2], out stableTime))
                {
                    Console.Error.WriteLine("ERROR: Invalid stableTime!");
                    return -3;
                }
                stableTime = stableTime * 1000;
            }

            Timer timer = new Timer(timeout);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            var stackChecker = new StackChecker()
            {
                StackName = stackName
            };
            if (stackChecker.AwaitDesiredStates2(stableTime))
            {
                Console.Out.WriteLine("All stack tasks are running.");
            }
            else
            {
                Console.Error.WriteLine("ERROR: Not all stack tasks completed successfully.");
                return -1;
            }

            return 0;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.Error.WriteLine("ERROR: Timed out waiting for desired state!");
            Environment.Exit(-3);
        }

    }
}
