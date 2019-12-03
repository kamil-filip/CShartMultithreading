using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MasteringConcurrency
{
    class SpinLock
    {
        private const int _count = 10000000;

        static void Main4(string[] args)
        {
            var map = new Dictionary<double, double>();
            var r = Math.Sin(0.01);

            map.Clear();
            var prm = 0d;
            var lockFlag = new object();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < _count; i++)
            {
                lock (lockFlag)
                {
                    map.Add(prm, Math.Sin(prm));
                    prm += 0.01;
                }
            }

            sw.Stop();
            Console.WriteLine($"Lock: {sw.ElapsedMilliseconds}");

            //spinlock with memory barrier
            map.Clear();
            var spinLock = new SpinLock();
            prm = 0;
            sw = Stopwatch.StartNew();

            for (int i = 0; i < _count; i++)
            {
                var gotLock = false;
                try
                {
                    spinLock.Enter(ref gotLock);
                    map.Add(prm, Math.Sin(prm));
                    prm += 0.01;
                }
                finally
                {
                    if (gotLock)
                        spinLock.Exit(true);
                }
            }

            sw.Stop();
            Console.WriteLine($"Spinlock with memory barrier: {sw.ElapsedMilliseconds} ms");


            //spinlock without memory barrier
            map.Clear();
            prm = 0;
            sw = Stopwatch.StartNew();

            for (int i = 0; i < _count; i++)
            {
                var gotLock = false;
                try
                {
                    spinLock.Enter(ref gotLock);
                    map.Add(prm, Math.Sin(prm));
                    prm += 0.01;
                }
                finally
                {
                    if (gotLock)
                        spinLock.Exit(true);
                }
            }

            sw.Stop();
            Console.WriteLine($"Spinlock without memory barrier: {sw.ElapsedMilliseconds} ms");


            Console.ReadKey();
        }
    }
}

