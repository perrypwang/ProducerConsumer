using System;
using System.Threading;

namespace ProducerConsumer
{
    class CellBuffer
    {
        private int item = -1;
        public void setBuffer(int val)
        {
            item = val;
        }
        public int getBuffer()
        {
            return item;
        }
    }

    public class MultiCellBuffer
    {
        private CellBuffer[] buffer;
        private Semaphore writeMutex, readMutex;
        public static Semaphore semEmpty, semFull;
        private int inIndex, outIndex;
        public MultiCellBuffer()
        {
            buffer = new CellBuffer[2];
            for(int i = 0; i< buffer.Length; i++)
            {
                buffer[i] = new CellBuffer();
            }
            semEmpty = new Semaphore(2, 2);
            semFull = new Semaphore(0, 2);
            writeMutex = new Semaphore(1, 1);
            readMutex = new Semaphore(1, 1);
            inIndex = 0;
            outIndex = 0;
        }
        public void setOneCell(int val)
        {
            semEmpty.WaitOne();
            writeMutex.WaitOne();
            buffer[inIndex].setBuffer(val);
            inIndex = (inIndex + 1) % 2;
            writeMutex.Release();
            semFull.Release();
        }
        public int getOneCell()
        {
            int val = -1;
            semFull.WaitOne();
            readMutex.WaitOne();
            val = buffer[outIndex].getBuffer();
            outIndex = (outIndex + 1) % 2;
            readMutex.Release();
            semEmpty.Release();
            return val;
        }
    }

    class PlantThread
    {
        System.Random RandNum = new System.Random();
        int myId;
        public PlantThread(int id) { myId = id; }
        public void runProducer()
        {
            int baseVal = myId * 1000;
            while(true)
            {
                int val = myMainClass.bufferCellRef.getOneCell();
                if (val != -1)   //Buffer is not empty
                {
                    Console.WriteLine($"Plant{myId} has read value {val}");
                }
            }
        }
    }

    class DealerThread
    {
        int myId, myItem;
        System.Random RandNum = new System.Random();
        public DealerThread(int id) { myId = id; }
        public void runConsumer()
        {
            for(int i = 0; i< 10; i++)
            {
                Thread.Sleep(RandNum.Next(1, 2000));
                int val = RandNum.Next(0, 100);
                myMainClass.bufferCellRef.setOneCell(val); //set a buffer cell to random number
                Console.WriteLine($"Dealer{myId} has written {val} to the buffer");
            }
        }
    }

    public class myMainClass
    {
        public static MultiCellBuffer bufferCellRef = new MultiCellBuffer();
        public static void Main()
        {
            Thread[] plants = new Thread[2];
            Thread[] dealers = new Thread[5];
            for(int i = 0; i < dealers.Length; i++)
            {
                DealerThread d = new DealerThread(i + 1);
                dealers[i] = new Thread(new ThreadStart(d.runConsumer));
            }
            for (int i = 0; i < plants.Length; i++)
            {
                PlantThread p = new PlantThread(i + 1);
                plants[i] = new Thread(new ThreadStart(p.runProducer));
            }
            for (int i = 0; i < dealers.Length; i++)
            {
                dealers[i].Start();
            }
            for (int i = 0; i < plants.Length; i++)
            {
                plants[i].Start();
            }

            Thread.Sleep(500);  //Wait for threads to start
            for (int i = 0; i < plants.Length; i++)
            {
                plants[i].Join();
            }
            for (int i = 0; i < dealers.Length; i++)
            {
                dealers[i].Join();
            }
            Console.WriteLine("main thread complete");
            Console.ReadLine();
        }
    }
}
