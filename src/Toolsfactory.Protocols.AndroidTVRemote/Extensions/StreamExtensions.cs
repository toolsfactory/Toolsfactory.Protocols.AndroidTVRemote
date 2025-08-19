using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace Toolsfactory.Protocols.AndroidTVRemote.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<byte> ReadByteAsync(
            this Stream stream,
            CancellationToken token)
        {
            byte[] bytes = new byte[1];
            int r = await stream.ReadAsync(bytes, 0, 1, token);
            if (r != 1)
                throw new IOException("Read Error");
            
            return bytes[0];
        }

        public static async Task<byte[]> ReadBytesAsync(
            this Stream stream,
            int count,
            CancellationToken token)
        {
            var bytes = new byte[count];
            int readCount = 0;

            while (readCount < count)
            {
                int left = count - readCount;
                int r = await stream.ReadAsync(bytes, readCount, left, token);

                if (r == 0)
                    throw new IOException("Read Error");

                readCount += r;
            }

            return bytes;
        }
        
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        public static async Task WriteProtoBufMessageAsync(this Stream stream, IMessage message, CancellationToken token, ILogger logger = null!)
        {
            if (logger != null)
                logger.LogDebug($"Sent: {message}  -  {message.ToByteArray().ToHex()}");
            
            var data = message.ToByteArray();
            var len = ((ulong)data.Length).ToVarIntBytes();
            await stream.WriteAsync(len, 0, len.Length, token);
            await stream.WriteAsync(data, 0, data.Length, token);
            await stream.FlushAsync(token);
        }

        public static async Task WriteMessageAsync(this Stream stream, byte[] message, CancellationToken token, ILogger logger = null!)
        {
            if (logger != null)
                logger.LogDebug($"Sent: {message.ToHex()}");
            
            await stream.WriteAsync([(byte) message.Length], 0, 1, token);
            await stream.WriteAsync(message, 0, message.Length, token); 
            await stream.FlushAsync(token);
        }
    }
}
