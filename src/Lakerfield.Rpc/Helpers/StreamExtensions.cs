using System;
using System.IO;
using System.Threading.Tasks;

namespace Lakerfield.Rpc.Helpers
{
  public static class StreamExtensions
  {
    private const int BufferSize = 0x2000;

    /// <summary>Copies the contents of one stream to another, asynchronously.</summary>
    /// <param name="input">The source stream.</param>
    /// <param name="output">The destination stream.</param>
    /// <param name="length">Stops on eof or maximum length.</param>
    /// <returns>A Task that represents the completion of the asynchronous operation.</returns>
    public async static Task CopyStreamToStreamAsync(this Stream input, Stream output, int length = int.MaxValue)
    {
      if (input == null) throw new ArgumentNullException("input");
      if (output == null) throw new ArgumentNullException("output");

      // Create two buffers.  One will be used for the current read operation and one for the current
      // write operation.  We'll continually swap back and forth between them.
      byte[][] buffers = new byte[2][] { new byte[BufferSize], new byte[BufferSize] };
      int filledBufferNum = 0;
      Task writeTask = null;
      int position = 0;

      // Until there's no more data to be read
      while (position < length)
      {
        // Read from the input asynchronously
        var readLength = length - position;
        if (readLength > buffers[filledBufferNum].Length)
          readLength = buffers[filledBufferNum].Length;
        var readTask = input.ReadAsync(buffers[filledBufferNum], 0, readLength);

        // If we have no pending write operations, just yield until the read operation has
        // completed.  If we have both a pending read and a pending write, yield until both the read
        // and the write have completed.
        if (writeTask == null)
        {
          await readTask;
        }
        else
        {
          var tasks = new[] { readTask, writeTask };
          await Task.WhenAll(tasks);
        }

        // If no data was read, nothing more to do.
        if (readTask.Result <= 0) break;

        // Otherwise, write the written data out to the file
        writeTask = output.WriteAsync(buffers[filledBufferNum], 0, readTask.Result);
        position += readTask.Result;

        // Swap buffers
        filledBufferNum ^= 1;
      }
    }

  }
}
