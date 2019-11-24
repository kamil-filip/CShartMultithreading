using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MasteringConcurrency
{
    class TryEnterExamle
    {
        public void MyTryEnterExample()
        {
            const int count = 10000;
            var a = new object(); var b = new object();
            var thread1 = new Thread(() => { 
                for (int i = 0; i < count; i++) 
                    lock (a) 
                        lock (b) 
                            Thread.SpinWait(100); 
            }); 
            
            
            var thread2 = new Thread(() => 
            LockTimeout(a, b, count)); 
            
            thread1.Start(); 
            thread2.Start(); 
            thread1.Join(); 
            thread2.Join(); 
            
            Console.WriteLine("Done");
        }

        static void LockTimeout(object a, object b, int count)
        {
            bool accquiredB = false; 
            bool accquiredA = false; 
            const int waitSeconds = 5; 
            const int retryCount = 3; 
            
            for (int i = 0; i < count; i++)
            {
                int retries = 0;
                                
                while (retries < retryCount)
                {
                    try { 
                        accquiredB = Monitor.TryEnter(b, TimeSpan.FromSeconds(waitSeconds)); 
                        
                        if (accquiredB) { 
                            try {
                                accquiredA = Monitor.TryEnter(a, TimeSpan.FromSeconds(waitSeconds)); 
                                
                                if (accquiredA) 
                                { 
                                    Thread.SpinWait(100); 
                                    break; 
                                } else 
                                { 
                                    retries++; 
                                } 
                            } 
                            finally 
                            {
                                if (accquiredA) 
                                {
                                    Monitor.Exit(a); 
                                } 
                            } 
                        } 
                        else 
                        { 
                            retries++; 
                        } 
                    } finally 
                    { 
                        if (accquiredB) 
                            Monitor.Exit(b); 
                    }
                }
                if (retries >= retryCount) Console.WriteLine("could not obtain locks");
            }
        }

    }
}


