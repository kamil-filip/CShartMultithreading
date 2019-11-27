using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MasteringConcurrency
{
    class ClassicRaceCondition
    {
        public void RaceCondition()
        {

        

        const int iterations = 10000;
        var counter = 0;

        object lockFlag = new object();

        ThreadStart proc = () =>
        {
            for (int i = 0; i < iterations; i++)
            {
                //lock (lockFlag)
                counter++;

                Thread.SpinWait(100);
                //lock (lockFlag)
                counter--;

            }
        };

        var threads = Enumerable.Range(0, 8).Select(n => new Thread(proc)).ToArray();

            foreach (var thread in threads)
                thread.Start();


            foreach (var thread in threads)
                thread.Join();

            Console.WriteLine(counter);
        }
    }
}
