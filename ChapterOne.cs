using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MasteringConcurrency
{
    class ChapterOne
    {
        static void Sample2(string[] args)
        {
            const int count = 10000;

            var a = new object();
            var b = new object();

            var thread1 = new Thread(
                () =>
                {
                    for (int i = 0; i < count; i++)
                        lock (a)
                            lock (b)
                                Thread.SpinWait(100);

                });

            var thread2 = new Thread(
                () =>
                {
                    for (int i = 0; i < count; i++)
                        lock (b)
                            lock (a)
                                Thread.SpinWait(100);

                });

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            Console.WriteLine("End");
        }

        static void Sample1()
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

        static void Sample4()
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