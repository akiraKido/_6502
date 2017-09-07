using System;
using System.Collections.Generic;
using System.Text;

namespace _6502
{


    class Cpu
    {
        internal Cpu( Memory memory )
        {
            _memory = memory;
        }

        // Registers
        internal byte A;
        internal byte X;
        internal byte Y;
        internal byte S;
        internal byte P;
        internal byte[] Pc = new byte[2];

        // Memory
        private readonly Memory _memory;

        // Flags
        private static readonly byte Flag_Carry     = 0b00000001;
        private static readonly byte Flag_Zero      = 0b00000010;
        private static readonly byte Flag_Overflow  = 0b01000000;
        private static readonly byte Flag_Negative  = 0b10000000;

        // Flag Utils
        private void SetFlag( byte flag ) => P |= flag;
        private void ResetFlag( byte flag ) => P &= (byte) ~flag;
        private void Toggle( byte flag, bool value )
        {
            if (value) SetFlag( flag ); else ResetFlag( flag );
        }
        private bool IsNegative( byte register ) => (register & Flag_Negative) != 0;

        /// <summary>
        /// Transfer. e.g) TAX, TAY
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void T( byte from, ref byte to )
        {
            to = from;
            Toggle( Flag_Zero, to == 0 );
            Toggle( Flag_Negative, IsNegative( to ) );
        }

        private void Ld( ref byte register, byte value )
        {
            register = value;
            Toggle( Flag_Zero, register == 0 );
            Toggle( Flag_Negative, IsNegative( register ) );
        }

        private void Adc( byte value )
        {
            var result = A + value + (Flag_Carry & P);
            Toggle( Flag_Carry, result > 255 );
            Toggle( Flag_Zero, A == 0 );

            var bResult = (byte) result;
            Toggle( Flag_Overflow, IsNegative( A ) != IsNegative( bResult ) );

            A = (byte) result;
        }


        internal void Run( params byte[] code )
        {
            int index = 0;

            ushort GetShort() => (ushort) (code[++index] | code[++index] << 8);

            byte Immediate() => code[++index];
            byte Zeropage() => code[++index];
            byte ZeropageX() => (byte) (code[++index] + X);
            byte ZeropageY() => (byte) (code[++index] + Y);
            ushort Absolute() => GetShort();
            ushort AbsoluteX() => (ushort) (GetShort() + X);
            ushort AbsoluteY() => (ushort) (GetShort() + Y);
            ushort IndirectX() => _memory[(ushort) (code[++index] + X)];
            ushort IndirectY() => (ushort) (_memory[code[++index]] + Y);

            while (index < code.Length)
            {
                switch (code[index])
                {
                    // ======================================================================
                    // LDA
                    case 0xA9: Ld( ref A, Immediate() ); break;  // Immediate
                    case 0xA5: Ld( ref A, _memory[Zeropage()] ); break;   // Zeropage
                    case 0xB5: Ld( ref A, _memory[ZeropageX()] ); break;  // Zeropage, X
                    case 0xAD: Ld( ref A, _memory[Absolute()] ); break;   // Absolute
                    case 0xBD: Ld( ref A, _memory[AbsoluteX()] ); break;  // Absolute, X
                    case 0xB9: Ld( ref A, _memory[AbsoluteY()] ); break;  // Absolute, Y
                    case 0xA1: Ld( ref A, _memory[IndirectX()] ); break;  // (Indirect, X)
                    case 0xB1: Ld( ref A, _memory[IndirectY()] ); break;  // (Indirect), Y

                    // LDX
                    case 0xA2: Ld( ref X, Immediate() ); break;
                    case 0xA6: Ld( ref X, _memory[Zeropage()] ); break;
                    case 0xB6: Ld( ref X, _memory[ZeropageY()] ); break;
                    case 0xAE: Ld( ref X, _memory[Absolute()] ); break;
                    case 0xBE: Ld( ref X, _memory[AbsoluteY()] ); break;

                    // LDY
                    case 0xA0: Ld( ref Y, Immediate() ); break;
                    case 0xA4: Ld( ref Y, _memory[Zeropage()] ); break;
                    case 0xB4: Ld( ref Y, _memory[ZeropageX()] ); break;
                    case 0xAC: Ld( ref Y, _memory[Absolute()] ); break;
                    case 0xBC: Ld( ref Y, _memory[AbsoluteX()] ); break;

                    // ======================================================================
                    // STA
                    case 0x85: _memory[Zeropage()] = A; break;
                    case 0x95: _memory[ZeropageX()] = A; break;
                    case 0x8D: _memory[Absolute()] = A; break;
                    case 0x9D: _memory[AbsoluteX()] = A; break;
                    case 0x99: _memory[AbsoluteY()] = A; break;
                    case 0x81: _memory[IndirectX()] = A; break;
                    case 0x91: _memory[IndirectY()] = A; break;

                    // STX
                    case 0x86: _memory[Zeropage()] = X; break;
                    case 0x96: _memory[ZeropageY()] = X; break;
                    case 0x8E: _memory[Absolute()] = X; break;

                    // STY
                    case 0x84: _memory[Zeropage()] = Y; break;
                    case 0x94: _memory[ZeropageX()] = Y; break;
                    case 0x8C: _memory[Absolute()] = Y; break;

                    // ======================================================================
                    // Transfer
                    case 0xAA: T( A, ref X ); break;    // TAX
                    case 0xA8: T( A, ref Y ); break;    // TAY
                    case 0xBA: T( S, ref X ); break;    // TSX
                    case 0x8A: T( X, ref A ); break;    // TXA
                    case 0x9A: T( X, ref S ); break;    // TXS
                    case 0x98: T( Y, ref A ); break;    // TYA

                    // ======================================================================
                    // ADC
                    case 0x69: Adc( Immediate() ); break;
                    case 0x65: Adc( _memory[Zeropage()] ); break;
                    case 0x75: Adc( _memory[ZeropageX()] ); break;
                    case 0x6D: Adc( _memory[Absolute()] ); break;
                    case 0x7D: Adc( _memory[AbsoluteX()] ); break;
                    case 0x79: Adc( _memory[AbsoluteY()] ); break;
                    case 0x61: Adc( _memory[IndirectX()] ); break;
                    case 0x71: Adc( _memory[IndirectY()] ); break;

                    // ======================================================================
                    // Misc
                    case 0xFE:                                      // prints zeropage-memory
                        Console.WriteLine( _memory[code[++index]] ); break;
                    case 0xFF: Console.WriteLine( A ); break;       // prints accum
                }
                index++;
            }

            Console.WriteLine( A );
        }
    }

}
