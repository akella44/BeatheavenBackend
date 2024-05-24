using System.Collections.Concurrent;

namespace Infrastructure.Data.DbContext;

public class AudioStreamsContext
{
    private readonly ConcurrentDictionary<string, MemoryStream> audioStreams = new ConcurrentDictionary<string, MemoryStream>();

    public async Task AddBytesToAudioStream(string id, byte[] bytes)
    {
        if (audioStreams.ContainsKey(id))
        {
            await audioStreams[id].WriteAsync(bytes, 0, bytes.Length);
            return;
        }

        audioStreams[id] = new MemoryStream();
        await audioStreams[id].WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task<byte[]> GetAudioStreamBytes(string id)
    {
        audioStreams[id].Seek(0, SeekOrigin.Begin);
        byte[] buffer = new byte[audioStreams[id].Length];
        var bytesToRead = await audioStreams[id].ReadAsync(buffer, 0, buffer.Length);

        return buffer;
    }
    
    public Task ClearAudioStream(string id)
    {
        return Task.Run(() =>
        {
            if (audioStreams.TryGetValue(id, out var memStream))
            {
                memStream.Dispose();
                audioStreams.TryRemove(id, out _);
            }
        });
    }
}