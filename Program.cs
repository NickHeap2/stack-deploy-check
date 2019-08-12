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
                Console.Error.WriteLine("dotnet stack-deploy-check {stackName} [waitTimeout]");
                return -1;
            }

            var stackName = args[0];
            int timeout = 10000;
            if (args.Length > 1)
            {
                if (!int.TryParse(args[1], out timeout))
                {
                    Console.Error.WriteLine("ERROR: Invalid timeout!");
                    return -2;
                }
                timeout = timeout * 1000;
            }

            Timer timer = new Timer(timeout);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            var stackChecker = new StackChecker()
            {
                StackName = stackName
            };
            stackChecker.AwaitDesiredStates();
            Console.Out.WriteLine("All stack tasks are running");

            return 0;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.Error.WriteLine("ERROR: Timed out waiting for desired state!");
            Environment.Exit(-3);
        }

    }
}
