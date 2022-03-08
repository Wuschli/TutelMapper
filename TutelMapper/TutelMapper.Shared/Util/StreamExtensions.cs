using System;
using System.IO;

namespace TutelMapper.Util;

public static class StreamExtensions
{
    public static void CopyStream(this Stream input, Stream output, int bytes, int bufferSize = 32768)
    {
        byte[] buffer = new byte[bufferSize];
        int read;
        while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
        {
            output.Write(buffer, 0, read);
            bytes -= read;
        }
        output.Flush();
    }
}