using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RegisterSnapshot
{
    public class BoundedSRMR
    {
        private readonly int RegCount;
        private bool[,] HandshakeMatrix;
        private Register[] Registers;

        private readonly Dictionary<TimeSpan, int>[] logWriter;
        private readonly Dictionary<TimeSpan, int[]> logReader = new Dictionary<TimeSpan, int[]>();

        /* clock for logging */
        private readonly Stopwatch timer = new Stopwatch();

        public BoundedSRMR(int regcount)
        {
            RegCount = regcount;
            Registers = new Register[RegCount];

            HandshakeMatrix = new bool[RegCount, RegCount];

            logWriter = new Dictionary<TimeSpan, int>[RegCount];

            for (var i = 0; i < RegCount; i++)
            {
                Registers[i] = new Register(regcount);
                logWriter[i] = new Dictionary<TimeSpan, int>();
            }

            timer.Start();
        }

        public void Update(int id, int value)
        {
            /* Collect handshake values */
            var f = new bool[RegCount];
            for (var j = 0; j < RegCount; j++)
            {
                f[j] = !HandshakeMatrix[j, id];
            }

            var snapshot = Scan(id);

            Registers[id].Snapshot = snapshot;
            Registers[id].Value = value;
            Registers[id].Handshakes = f;
            Registers[id].Toggle = !Registers[id].Toggle;

            logWriter[id].Add(timer.Elapsed, value);
        }

        public int[] Scan(int id, bool isFromMain = false)
        {


            var moved = new int[RegCount];

            while (true)
            {

                for (var j = 0; j < RegCount; j++)
                {
                    HandshakeMatrix[id, j] = Registers[j].Handshakes[id];
                }

                /* collect */
                var a = (Register[]) Registers.Clone();
                var b = (Register[]) Registers.Clone();
                var nobodyMoved = true;

                for (var j = 0; j < RegCount; j++)
                    nobodyMoved &= a[j].Handshakes[id] == b[j].Handshakes[id] &&
                                   a[j].Handshakes[id] == HandshakeMatrix[id, j] &&
                                   a[j].Toggle == b[j].Toggle;
                if (nobodyMoved)
                {
                    var result = new int[RegCount];
                    for (var j = 0; j < RegCount; j++)
                        result[j] = b[j].Value;
                    if(isFromMain)logReader.Add(timer.Elapsed, result);
                    return result;
                }

                for (var j = 0; j < RegCount; j++)
                {
                    if (HandshakeMatrix[id, j] != a[j].Handshakes[id] || HandshakeMatrix[id, j] != 
                        b[j].Handshakes[id] || a[j].Toggle != b[j].Toggle)
                    {
                        if (moved[j] == 1)
                        {
                            if(isFromMain)logReader.Add(timer.Elapsed, b[j].Snapshot);
                            return  b[j].Snapshot;
                        }
                        moved[j]++;
                    }
                }
            }
        }
        
        public void PrintLogs()
        {
            for (var i = 0; i < RegCount; i++)
            {
                Console.WriteLine($"Register[{i}] and it's writing log:");
                Console.Write($"({Registers[i].Value}, ");
                
                var bitMaskString = new StringBuilder("");
                bitMaskString.Append("[");
                for (var j = 0; j < RegCount - 1; j++)
                {
                    bitMaskString.Append(Registers[i].Handshakes[j]);
                    bitMaskString.Append(", ");
                }
                bitMaskString.Append(Registers[i].Handshakes[RegCount - 1]);
                bitMaskString.Append("]");
                
                Console.WriteLine($"{bitMaskString}, " +
                                  $"{Registers[i].Toggle}, " +
                                  $"[{string.Join(",", Registers[i].Snapshot)}]");
                foreach (var update in logWriter[i])
                {
                    Console.WriteLine(update);
                }
                Console.WriteLine("----------------------------");
            }
            Console.WriteLine("Read log:");
            foreach (var scan in logReader)
            {
                Console.WriteLine($"<values = ({string.Join(", ", scan.Value), 8}) " +
                                  $"time = {scan.Key}>");
            }
            Console.WriteLine("=============================");
        }
    }
}    

