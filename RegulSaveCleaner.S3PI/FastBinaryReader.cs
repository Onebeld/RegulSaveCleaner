using System;
using System.IO;

namespace RegulSaveCleaner.S3PI;

public class FastBinaryReader : IDisposable
{
    private static readonly byte[] buffer = new byte[50];
    
    public Stream BaseStream { get; }

    public FastBinaryReader(Stream input)
    {
        BaseStream = input;
    }

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

    public unsafe ulong ReadUInt64()
    {
        BaseStream.Read(buffer, 0, 8);
        
        fixed (byte* numRef = &buffer[0])
            return *(ulong*)numRef;
    }

    public byte[] ReadBytes(int count)
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