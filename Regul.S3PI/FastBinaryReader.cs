using System;
using System.IO;

namespace Regul.S3PI;

public class FastBinaryReader : IDisposable
{
    private static byte[] buffer = new byte[50];
    
    public Stream BaseStream { get; private set; }

    public FastBinaryReader(Stream input)
    {
        BaseStream = input;
    }

    public byte ReadByte() => (byte)BaseStream.ReadByte();

    public unsafe short ReadInt16()
    {
        BaseStream.Read(buffer, 0, 2);
        
        fixed (byte* numRef = &buffer[0])
            return *(short*)numRef;
    }

    public unsafe int ReadInt32()
    {
        BaseStream.Read(buffer, 0, 4);
        
        fixed (byte* numRef = &buffer[0])
            return *(int*)numRef;
    }
    
    public unsafe uint ReadUInt32()
    {
        BaseStream.Read(buffer, 0, 4);
        
        fixed (byte* numRef = &buffer[0])
            return *(uint*)numRef;
    }

    public unsafe ulong ReadUInt64()
    {
        BaseStream.Read(buffer, 0, 8);
        
        fixed (byte* numRef = &buffer[0])
            return *(ulong*)numRef;
    }

    public unsafe byte[] ReadBytes(int count)
    {
        byte[] arr = new byte[count];
        BaseStream.Read(arr, 0, count);

        return arr;
    }

    public void Dispose()
    {
        BaseStream?.Dispose();
    }
}