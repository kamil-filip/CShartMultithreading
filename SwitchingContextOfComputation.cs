using System;
using System.Collections.Generic;
using System.Text;

namespace MasteringConcurrency
{
    class SwitchingContextOfComputation
    {
        public void SwitchingContext()
        {
            var arg = 0;
            var result = "";
            var counter = 0;
            var lockHandle = new object();

            var calcThread = new Thread(() =>
            {
                while (true)
                {
                    lock (lockHandle)
                    {
                        counter++;
                        result = arg.ToString();
                        Thread.Sleep(4000);

                        Monitor.Pulse(lockHandle);
                        Monitor.Wait(lockHandle);

                    }
                }

            })
            {
                IsBackground = true
            };

            lock (lockHandle)
            {
                calcThread.Start();
                Thread.Sleep(100);
                Console.WriteLine($"counter = {counter}, result = {result}");


                arg = 123;
                Monitor.Pulse(lockHandle);
                Monitor.Wait(lockHandle);
                Console.WriteLine($"counter = {counter}, result = {result}");

                arg = 321;
                Monitor.Pulse(lockHandle);
                Monitor.Wait(lockHandle);
                Console.WriteLine($"counter = {counter}, result = {result}");
            }

        }
    }
}
