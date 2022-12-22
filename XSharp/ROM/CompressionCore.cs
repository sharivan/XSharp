using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.ROM
{
    public struct MatchPair
    {
        internal uint offset;
        internal uint length;
    }

    public static class CompressionCore
    {
        public const uint MAX_LENGTH = 63;
        public const uint MIN_LENGTH = 3;
        public const uint WINDOW_SIZE = 1023;

        public static void ComputeKMP(byte[] src, int srcOff, int[] table, int maxLength)
        {
            table[0] = -1;
            table[1] = 0;

            int i = 2;
            int j = 0;

            while (i < maxLength)
            {
                if (src[srcOff + i - 1] == src[srcOff + j])
                {
                    table[i++] = ++j;
                }
                else if (j > 0)
                {
                    j = table[j];
                }
                else
                {
                    table[i++] = 0;
                }
            }
        }

        public static MatchPair Find(byte[] src, int srcOff, uint windowStart, uint uncompStart, uint size)
        {
            MatchPair match;
            uint i, m;
            int[] table = new int[MAX_LENGTH];

            ComputeKMP(src, (int) uncompStart, table, (int) System.Math.Min(size - uncompStart, MAX_LENGTH));

            match.length = 0;
            match.offset = 0;
            m = 0;
            i = 0;

            while (m < uncompStart - windowStart)
            {

                if (src[uncompStart + i] == src[windowStart + m + i])
                {
                    ++i;

                    if (i == MAX_LENGTH)
                    {
                        match.length = MAX_LENGTH;
                        match.offset = uncompStart - windowStart - m;
                        break;
                    }
                    else if (uncompStart + i == size)
                    {
                        // special case to handle when we matched up all the way to the end of the source
                        // this must be the longest match so we can exit out
                        match.length = i;
                        match.offset = uncompStart - windowStart - m;
                        break;
                    }
                }
                else
                {
                    if (i > match.length)
                    {
                        match.length = i;
                        match.offset = uncompStart - windowStart - m;
                    }

                    m += (uint) (i - table[i]);
                    //++m;

                    i = table[i] > 0 ? (uint) table[i] : 0;
                }
            }

            return match;
        }

        public static int GFXRLE(byte[] rom, int romOff, byte[] dest, int destOff, int pointer, int size, int type, bool obj = false)
        {
            int oldPointer = pointer;

            if (type == 0)
            {
                int writeIndex = 0;

                for (int i = 0; i < size >> 3; i++)
                {
                    byte control = rom[romOff + pointer++];
                    byte data = rom[romOff + pointer++];
                    for (int j = 0; j < 8; j++)
                    {
                        dest[destOff + writeIndex++] = (control & 0x80) != 0 ? rom[romOff + pointer++] : data;
                        control <<= 1;
                    }
                }
            }
            else
            {
                // X2 and X3 use a more complicated LZ compression where chunks of uncompressed data
                // can be repeated using an encoded relative offset and length.  This is basically LZSS with
                // a snes-friendly file format.

                byte control = rom[romOff + pointer++];
                uint bitPos = 7;
                uint count = 0;
                uint writeIndex = 0;

                while ((int) count < size)
                {
                    if ((control & (1 << (int) bitPos)) != 0)
                    {
                        // length
                        byte currentByte = rom[romOff + pointer++];
                        uint length = (uint) (currentByte >> 2);
                        uint offset = 0;
                        uint baseWriteIndex = writeIndex;

                        count += length;

                        offset = (uint) (((currentByte & 0x3) << 8) + rom[romOff + pointer++]);

                        for (int i = 0; length != 0; ++i)
                        {
                            dest[destOff + writeIndex++] = dest[destOff + (baseWriteIndex - offset) + i];
                            length--;
                        }
                    }
                    else
                    {
                        dest[destOff + writeIndex++] = rom[romOff + pointer++];
                        count++;
                    }

                    if (bitPos == 0)
                    {
                        control = rom[romOff + pointer++];
                        bitPos = 7;
                    }
                    else
                    {
                        bitPos--;
                    }
                }
            }

            return pointer - oldPointer;
        }

        public static ushort GFXRLECmp(byte[] src, int srcOff, byte[] dest, int destOff, int size, int type)
        {
            int odest = destOff;

            if (type == 0)
            {
                byte maxCount = 0;
                byte[] bArr = new byte[0x100];
                byte control = 0xFF;
                for (int i = 0; i < size >> 3; i++)
                {
                    int config = destOff;
                    byte data = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        bArr[src[j]]++;
                        if (bArr[src[j]] > maxCount)
                        {
                            maxCount = bArr[src[j]];
                            data = src[j];
                        }
                    }

                    maxCount = 0;
                    Array.Clear(bArr, 0, 0x100);

                    destOff += 2;
                    for (int j = 0; j < 8; j++)
                    {
                        if (data == src[srcOff])
                            control ^= (byte) (0x80 >> j);
                        else
                            dest[destOff++] = src[srcOff];
                        srcOff++;
                    }

                    dest[config++] = control;
                    dest[config] = data;
                    control = 0xFF;
                }
            }
            else
            {
                // LZSS compression
                int control = destOff;
                byte flag = 0x80;
                uint windowIndex = 0;
                uint uncompressedIndex = 0;
                MatchPair match;

                if (size != 0)
                {
                    match = Find(src, srcOff, windowIndex, uncompressedIndex, (uint) size);
                    while ((int) uncompressedIndex < size)
                    {

                        if (flag == 0x80)
                        {
                            // write out control word
                            control = destOff++;
                            dest[control] = 0x0;
                        }

                        if ((int) match.length > size)
                        {
                            // we reached end of data so make sure to force length.  Should this even happen?  Probably should be an assert.
                            match.length = (uint) size;
                        }

                        if (match.length < MIN_LENGTH)
                        {
                            // write out just the symbol
                            dest[destOff++] = src[uncompressedIndex++];
                        }
                        else
                        {
                            byte tuple = 0;

                            // write out compressed length (top 6b), offset/distance (bottom 10b)
                            dest[control] |= flag;
                            // length + 2b of offset
                            tuple = (byte) ((match.length << 2) | (match.offset >> 8));
                            dest[destOff++] = tuple;
                            tuple = (byte) (match.offset & 0xff);
                            dest[destOff++] = tuple;

                            uncompressedIndex += match.length;
                        }

                        flag >>= 1;

                        if (flag == 0)
                        {
                            // reset for the next control word
                            flag = 0x80;
                        }

                        if (uncompressedIndex - windowIndex > WINDOW_SIZE)
                        {
                            // advance the window
                            windowIndex = uncompressedIndex - WINDOW_SIZE;
                        }

                        if ((int) uncompressedIndex < size)
                        {
                            match = Find(src, srcOff, windowIndex, uncompressedIndex, (uint) size);
                        }
                    }
                }
            }

            return (ushort) (destOff - odest);
        }

        public static int LayoutRLE(byte width, byte height, byte[] sceneUsed, int sceneUsedOff, byte[] src, int srcOff, byte[] dst, int dstOff, bool sizeOnly, bool overdrive_ostrich)
        {
            bool cType = false;
            byte counter = 0; //unsigned char byte
            short writeIndex = 3; //counts size of layout

            for (int i = 0; i < width * height;)
            {
                byte buf = src[srcOff + i++]; //i increments after
                counter++;
                bool write = false;
                do
                {
                    if (i >= width * height)
                    {
                        write = true;
                    }
                    else
                    {
                        if (!sizeOnly && sceneUsed[sceneUsedOff] < src[srcOff + i] + 1)
                            sceneUsed[sceneUsedOff] = (byte) (src[srcOff + i] + 1);

                        if (src[srcOff + i] == buf) //checks for repeating values
                        {
                            if (counter == 1)
                            {
                                cType = true;
                                counter |= 0x80; //counter =129 after
                                counter++; //counter = 130
                                i++; //buf doesn't change in the inside loop like in loadlayout. maybe do bitwise on this value instead?
                            }
                            else if (cType)
                            {
                                counter++;
                                i++;
                            }
                            else
                                write = true;
                        }
                        else if (src[srcOff + i] == buf + counter) //what is this
                        {
                            if (counter == 1)
                            {
                                cType = false;
                                counter++;
                                i++;
                            }
                            else if (!cType)
                            {
                                counter++;
                                i++;
                            }
                            else
                                write = true;
                        }
                        else
                        {
                            write = (counter & 0x7F) == 0x7E || true;
                        }
                    }

                    if (write)
                    {
                        if (!sizeOnly)
                        { //debug:i<155 will save but can't edit layout

                            if (writeIndex == 27 && buf == 0 && overdrive_ostrich) counter += 34;
                            dst[dstOff + writeIndex++] = counter;
                            dst[dstOff + writeIndex++] = buf;
                            if (writeIndex == 19 && overdrive_ostrich)
                            {
                                dst[dstOff + writeIndex++] = 254; //original values pulled from working rom
                                dst[dstOff + writeIndex++] = 0;
                                dst[dstOff + writeIndex++] = 254;
                                dst[dstOff + writeIndex++] = 0;
                                dst[dstOff + writeIndex++] = 238;
                                dst[dstOff + writeIndex++] = 0;
                                i = 517; //next non-zero value in src
                            }
                        }
                        else
                        {
                            writeIndex += 2; //debug: writeIndex += 2;
                        }

                        counter = 0; //exits the while loop
                    }
                } while (counter != 0); //do until WRITE is executed
            }

            if (!sizeOnly)
            {
                dst[dstOff + 0] = width;
                dst[dstOff + 1] = height;
                dst[dstOff + 2] = sceneUsed[sceneUsedOff];
                dst[dstOff + writeIndex++] = 0xFF; //pointer?
            }
            else
            {
                writeIndex++;
            }

            return writeIndex;
        }
    }
}
