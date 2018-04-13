using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace EndianStreamTools
{
    public enum Endian
    {
        Little,    //Force little endian read/write
        Big,       //Force big endian read/write
        Platform   //Use platform endianness
    }


    static class StreamRW
    {
        private static readonly bool le = BitConverter.IsLittleEndian;

        private static byte[] SwapToEndian(byte[] input, Endian endian)
        {
            if (endian == Endian.Platform)
                return input;
            
            bool swap = (le && endian == Endian.Big) ||
                        (!le && endian == Endian.Little);
            if (!swap)
                return input;
            byte[] result = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
                result[input.Length - 1 - i] = input[i];
            return result;
        }

        public static Int32 ReadS32(Stream input, Endian endian)
        {
            byte[] buffer = new byte[4];
            input.Read(buffer, 0, 4);
            buffer = SwapToEndian(buffer, endian);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static UInt32 ReadU32(Stream input, Endian endian)
        {
            byte[] buffer = new byte[4];
            input.Read(buffer, 0, 4);
            buffer = SwapToEndian(buffer, endian);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public static Single ReadSingle(Stream input, Endian endian)
        {
            byte[] buffer = new byte[4];
            input.Read(buffer, 0, 4);
            buffer = SwapToEndian(buffer, endian);
            return BitConverter.ToSingle(buffer, 0);
        }

        public static Single ReadHalfFloat(Stream input, Endian endian)
        {
            ushort bits = StreamRW.ReadU16(input, endian);
            Half h = Half.ToHalf(bits);
            return (Single)h;
        }

        public static Int16 ReadS16(Stream input, Endian endian)
        {
            byte[] buffer = new byte[2];
            input.Read(buffer, 0, 2);
            buffer = SwapToEndian(buffer, endian);
            return BitConverter.ToInt16(buffer, 0);
        }

        public static UInt16 ReadU16(Stream input, Endian endian)
        {
            byte[] buffer = new byte[2];
            input.Read(buffer, 0, 2);
            buffer = SwapToEndian(buffer, endian);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public static Int64 ReadS64(Stream input, Endian endian)
        {
            byte[] buffer = new byte[8];
            input.Read(buffer, 0, 8);
            buffer = SwapToEndian(buffer, endian);
            return BitConverter.ToInt64(buffer, 0);
        }

        public static UInt64 ReadU64(Stream input, Endian endian)
        {
            byte[] buffer = new byte[8];
            input.Read(buffer, 0, 8);
            buffer = SwapToEndian(buffer, endian);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static Double ReadDouble(Stream input, Endian endian)
        {
            byte[] buffer = new byte[8];
            input.Read(buffer, 0, 8);
            buffer = SwapToEndian(buffer, endian);
            return BitConverter.ToDouble(buffer, 0);
        }

        public static sbyte ReadS8(Stream input)
        {
            byte[] buffer = new byte[1];
            input.Read(buffer, 0, 1);
            return (sbyte)buffer[0];
        }

        public static sbyte ReadS8(Stream input, Endian endian)
        {
            return ReadS8(input);
        }

        public static byte ReadU8(Stream input)
        {
            byte[] buffer = new byte[1];
            input.Read(buffer, 0, 1);
            return buffer[0];
        }

        public static byte ReadU8(Stream input, Endian endian)
        {
            return ReadU8(input);
        }

        public static String ReadAnsiNullTerminatedString(Stream input)
        {
            byte cid;

            String result = "";
            cid = ReadU8(input);
            while (cid != 0)
            {
                result += (char)cid;
                cid = ReadU8(input);
            }
            return result;
        }

        public static String ReadAnsiNullTerminatedString(Stream input, Endian endian)
        {
            return ReadAnsiNullTerminatedString(input);
        }

        public static String ReadUnicodeNullTerminatedString(Stream input, Endian endian)
        {
            UInt16 cid;

            String result = "";
            cid = ReadU16(input, endian);
            while (cid != 0)
            {
                result += (char)cid;
                cid = ReadU8(input);
            }
            return result;
        }

        public static String ReadAnsiFixedString(Stream input, int length)
        {
            byte cid;

            long pos = input.Position;

            String result = "";
            
            for(int i = 0; i < length; i++)
            {
                cid = ReadU8(input);
                if (cid == 0)
                    break;
                result += (char)cid;                
            }

            input.Seek(pos + length, SeekOrigin.Begin);

            return result;
        }

        public static String ReadAnsiFixedString(Stream input, int length, Endian endian)
        {
            return ReadAnsiFixedString(input, length);
        }

        public static String ReadEncodedFixedString(Stream input, int length, Encoding encoding)
        {
            List<byte> byteList = new List<byte>();
            byte cid;

            long pos = input.Position;

            for (int i = 0; i < length; i++)
            {
                cid = ReadU8(input);
                if (cid == 0)
                    break;
                byteList.Add(cid);
            }

            byte[] bytes = byteList.ToArray();

            input.Seek(pos + length, SeekOrigin.Begin);

            return encoding.GetString(bytes);
        }

        public static String ReadEncodedFixedString(Stream input, int length, Encoding encoding, Endian endiang)
        {
            return ReadEncodedFixedString(input, length, encoding);
        }

        public static String ReadEncodedNullTerminatedString(Stream input, Encoding encoding)
        {
            List<byte> byteList = new List<byte>();
            byte cid;

            cid = ReadU8(input);
            while (cid != 0)
            {
                byteList.Add(cid);
                cid = ReadU8(input);
            }

            byte[] bytes = byteList.ToArray();

            return encoding.GetString(bytes);            
        }

        public static String ReadEncodedNullTerminatedString(Stream input, Encoding encoding, Endian endian)
        {
            return ReadEncodedNullTerminatedString(input, encoding);
        }

        public static String ReadUnicodeFixedString(Stream input, int length, Endian endian)
        {
            UInt16 cid;

            long pos = input.Position;

            String result = "";

            for (int i = 0; i < length; i++)
            {
                cid = ReadU16(input, endian);
                if (cid == 0)
                    break;
                result += (char)cid;
            }

            input.Seek(pos + length * 2, SeekOrigin.Begin);

            return result;
        }


        public static void WriteS8(Stream output, sbyte value)
        {
            output.WriteByte((byte)value);           
        }

        public static void WriteS8(Stream output, sbyte value, Endian endian)
        {
            WriteS8(output, value);
        }

        public static void WriteU8(Stream output, byte value)
        {
            output.WriteByte((byte)value);           
        }

        public static void WriteU8(Stream output, byte value, Endian endian)
        {
            WriteU8(output, value);
        }

        public static void WriteAnsiNullTerminatedString(Stream output, String text)
        {
            byte cid;
            
            for(int i=0;i<text.Length;i++)
            {
                cid = (byte)text[i];
                output.WriteByte(cid);
            }
            output.WriteByte(0);
        }

        public static void WriteAnsiNullTerminatedString(Stream output, String text, Endian endian)
        {
            WriteAnsiNullTerminatedString(output,text);
        }

        public static void WriteUnicodeNullTerminatedString(Stream output, String text, Endian endian)
        {
            UInt16 cid;

            for (int i = 0; i < text.Length; i++)
            {
                cid = (UInt16)text[i];
                WriteU16(output,cid,endian);
            }
            cid = 0;
            WriteU16(output, cid, endian);
        }

        public static void WriteS32(Stream output, Int32 value, Endian endian)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            buffer = SwapToEndian(buffer, endian);
            output.Write(buffer, 0, 4);
        }

        public static void WriteU32(Stream output, UInt32 value, Endian endian)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            buffer = SwapToEndian(buffer, endian);
            output.Write(buffer, 0, 4);
        }

        public static void WriteSingle(Stream output, Single value, Endian endian)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            buffer = SwapToEndian(buffer, endian);
            output.Write(buffer, 0, 4);
        }

        public static void WriteS16(Stream output, Int16 value, Endian endian)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            buffer = SwapToEndian(buffer, endian);
            output.Write(buffer, 0, 2);
        }

        public static void WriteU16(Stream output, UInt16 value, Endian endian)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            buffer = SwapToEndian(buffer, endian);
            output.Write(buffer, 0, 2);
        }

        public static void WriteS64(Stream output, Int64 value, Endian endian)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            buffer = SwapToEndian(buffer, endian);
            output.Write(buffer, 0, 8);            
        }

        public static void WriteU64(Stream output, UInt64 value, Endian endian)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            buffer = SwapToEndian(buffer, endian);
            output.Write(buffer, 0, 8);
        }

        public static void WriteDouble(Stream output, double value, Endian endian)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            buffer = SwapToEndian(buffer, endian);
            output.Write(buffer, 0, 8);
        }

    }


    public class StreamParser
    {
        private Stream _stream;

        public Stream BaseStream
        {
            get { return _stream; }
        }

        private Endian _endian;

        public Endian Endian
        {
            get { return _endian; }
            set { _endian = value; }
        }

        public StreamParser(Stream stream, Endian endian)
        {
            _stream = stream;
            _endian = endian;
        }

        public StreamParser(Stream stream) : this(stream, Endian.Platform)
        { }

        public Int32 ReadS32()
        {
            return StreamRW.ReadS32(_stream, _endian);
        }

        public UInt32 ReadU32()
        {
            return StreamRW.ReadU32(_stream, _endian);
        }

        public Single ReadSingle()
        {
            return StreamRW.ReadSingle(_stream, _endian);
        }

        public Int16 ReadS16()
        {
            return StreamRW.ReadS16(_stream, _endian);
        }

        public UInt16 ReadU16()
        {
            return StreamRW.ReadU16(_stream, _endian);
        }

        public Int64 ReadS64()
        {
            return StreamRW.ReadS64(_stream, _endian);
        }

        public UInt64 ReadU64()
        {
            return StreamRW.ReadU64(_stream, _endian);
        }

        public Double ReadDouble()
        {
            return StreamRW.ReadDouble(_stream, _endian);
        }
        
        public sbyte ReadS8()
        {
            return StreamRW.ReadS8(_stream);
        }        

        public byte ReadU8()
        {
            return StreamRW.ReadU8(_stream);
        }

        public Single ReadHalfFloat()
        {
            return StreamRW.ReadHalfFloat(_stream, _endian);
        }

        public String ReadAnsiNullTerminatedString()
        {
            return StreamRW.ReadAnsiNullTerminatedString(_stream, _endian);
        }

        public String ReadUnicodeNullTerminatedString()
        {
            return StreamRW.ReadUnicodeNullTerminatedString(_stream, _endian);
        }

        public String ReadAnsiFixedString(int length)
        {
            return StreamRW.ReadAnsiFixedString(_stream, length, _endian);
        }

        public String ReadEncodedFixedString(int length, Encoding encoding)
        {
            return StreamRW.ReadEncodedFixedString(_stream, length, encoding, _endian);
        }

        public String ReadEncodedNullTerminatedString(Encoding encoding)
        {
            return StreamRW.ReadEncodedNullTerminatedString(_stream, encoding, _endian);
        }

        public String ReadUnicodeFixedString(int length)
        {
            return StreamRW.ReadUnicodeFixedString(_stream, length, _endian);
        }

        public void WriteS8(sbyte value)
        {
            StreamRW.WriteS8(_stream, value);
        }

        public void WriteU8(byte value)
        {
            StreamRW.WriteU8(_stream, value);
        }

        public void WriteAnsiNullTerminatedString(String text)
        {
            StreamRW.WriteAnsiNullTerminatedString(_stream, text);
        }

        public void WriteUnicodeNullTerminatedString(String text)
        {
            StreamRW.WriteUnicodeNullTerminatedString(_stream, text, _endian);
        }

        public void WriteS32(Int32 value)
        {
            StreamRW.WriteS32(_stream, value, _endian);            
        }

        public void WriteU32(UInt32 value)
        {
            StreamRW.WriteU32(_stream, value, _endian);
        }

        public void WriteSingle(Single value)
        {
            StreamRW.WriteSingle(_stream, value, _endian);
        }

        public void WriteS16(Int16 value)
        {
            StreamRW.WriteS16(_stream, value, _endian);
        }

        public void WriteU16(UInt16 value)
        {
            StreamRW.WriteU16(_stream, value, _endian);
        }

        public void WriteS64(Int64 value)
        {
            StreamRW.WriteS64(_stream, value, _endian);
        }

        public void WriteU64(UInt64 value)
        {
            StreamRW.WriteU64(_stream, value, _endian);
        }

        public void WriteDouble(double value)
        {
            StreamRW.WriteDouble(_stream, value, _endian);
        }

    }
}
