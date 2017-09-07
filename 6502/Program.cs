using System;
using System.Collections;
using System.Collections.Generic;

namespace _6502
{
    class Program
    {
        static void Main( string[] args )
        {
            byte[] code = {0xA9, 0x10, 0x95, 0x10};

            byte a = 200;
            byte m = 55;
            byte c = 1;
            int result = ( a + m + c );

            Console.WriteLine( Convert.ToString( result ), 2 );

            //var memory = new Memory();
            //var cpu = new Cpu( memory );
            //cpu.Run( code );
        }
    }

    class Memory
    {
        internal byte[] _memory = new byte[2048];

        internal byte this[byte index]
        {
            get => _memory[index];
            set => _memory[index] = value;
        }

        internal byte this[ushort index]
        {
            get => _memory[index];
            set => _memory[index] = value;
        }
    }

}
