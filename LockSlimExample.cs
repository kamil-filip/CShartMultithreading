using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace MasteringConcurrency
{
    class Program2
    {
        private const int _readersCount = 5;
        private const int _writersCount = 1;
        private const int _readPayLoad = 100;
        private const int _writePayLoad = 100;
        private const int _count = 100000;

        private static readonly Dictionary<int, string> _map =
            new Dictionary<int, string>();

        private static void ReaderProc()
        {
            string val;
            _map.TryGetValue(Environment.TickCount % _count, out val);
            Thread.SpinWait(_readPayLoad);
        }

        private static void WriteProc()
        {
            var n = Environment.TickCount % _count;

            Thread.SpinWait(_writePayLoad);
            _map[n] = n.ToString();
        }

        private static long Measure(Action reader, Action writer)
        {
            var threads = Enumerable
                .Range(0, _readersCount).Select(n => new Thread(
                    () =>
                    {
                        for (int i = 0; i < _count; i++)
                            reader();
                    })).Concat(
                Enumerable.Range(0, _writersCount)
                .Select(n => new Thread(
                    () =>
                    {
                        for (int i = 0; i < _count; i++)
                            writer();
                    }))).ToArray();

            _map.Clear();
            var sw = Stopwatch.StartNew();


            foreach (var thread in threads)
                thread.Start();

            foreach (var thread in threads)
                thread.Join();

            sw.Stop();
            return sw.ElapsedMilliseconds;

        }

        private static readonly object _simpleLockLock = new object();

        private static void SimpleLockReader()
        {
            lock (_simpleLockLock)
                ReaderProc();
        }

        private static void SimpleLockWriter()
        {
            lock (_simpleLockLock)
                WriteProc();
        }

        private static readonly ReaderWriterLock _rwLock = new ReaderWriterLock();

        private static void RWLockReader()
        {
            _rwLock.AcquireReaderLock(-1);
            try
            {
                ReaderProc();
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        private static void RWLockWriter()
        {
            _rwLock.AcquireWriterLock(-1);
            try
            {
                WriteProc();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        private static readonly ReaderWriterLockSlim _rwLockSlim =
            new ReaderWriterLockSlim();

        private static void RWLockSlimReader()
        {
            _rwLockSlim.EnterReadLock();
            try
            {
                ReaderProc();
            }
            finally
            {
                _rwLockSlim.ExitReadLock();
            }
        }

        private static void RWLockSlimWriter()
        {
            _rwLockSlim.EnterWriteLock();
            try
            {
                WriteProc();
            }
            finally
            {
                _rwLockSlim.ExitWriteLock();
            }
        }


        static void Main2(string[] args)
        {
            // Warm up    
            Measure(SimpleLockReader, SimpleLockWriter);
            // Measure    
            var simpleLockTime = Measure(SimpleLockReader, SimpleLockWriter);
            Console.WriteLine("Simple lock: {0}ms", simpleLockTime);

            // Warm up    
            Measure(RWLockReader, RWLockWriter);

            // Measure    
            var rwLockTime = Measure(RWLockReader, RWLockWriter);
            Console.WriteLine("ReaderWriterLock: {0}ms", rwLockTime);

            // Warm up    
            Measure(RWLockSlimReader, RWLockSlimWriter);

            // Measure    
            var rwLockSlimTime = Measure(RWLockSlimReader, RWLockSlimWriter);
            Console.WriteLine("ReaderWriterLockSlim: {0}ms", rwLockSlimTime);


            Console.ReadKey();
        }
    }
}
