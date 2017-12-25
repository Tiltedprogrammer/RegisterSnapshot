using System;
using System.Linq;

namespace RegisterSnapshot
{
    public class Register
    {
        public int Value;
        /* bit to to distinguish two consecutive writes,
         * even though no other process updated its register,
         * to ensure that each write changes the register value
         */
        public bool Toggle;
        /*vector of values of all registers that's what we need to return*/
        public int[] Snapshot;
        /*single-writer single-reader registers,
          which contains two atomic bits for communicatin between processes*/
        public bool[] Handshakes;
        
        public Register(int size = 4)
        {
            Value = default(int);
            Toggle = true;
            Snapshot = Enumerable.Repeat(default(int), size).ToArray();
            Handshakes = Enumerable.Repeat(default(bool), size).ToArray();
        }
        
    }
}