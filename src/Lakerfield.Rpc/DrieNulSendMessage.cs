using System.IO;
using System.Threading;
using Lakerfield.Bson.IO;
using Lakerfield.Bson.Serialization;

namespace Lakerfield.Rpc
{
  public class DrieNulSendMessage<T> where T : class
  {
    // private static fields
    private static int __lastRequestId = 0;

    // private fields
    private readonly BsonBinaryWriterSettings _writerSettings;
    private int _messageStartPosition = -1; // start position in buffer for backpatching messageLength
    private int _messageLength;
    private int _requestId;
    private int _responseTo;
    private MessageOpcode _opcode;
    private MessageFlags _flags;
    private int _observableId;
    private T _message;



    // constructor
    public DrieNulSendMessage(BsonBinaryWriterSettings writerSettings = null)
    {
      if (writerSettings == null)
      {
        writerSettings = BsonBinaryWriterSettings.Defaults;
        //throw new ArgumentNullException("writerSettings");
      }

      _writerSettings = writerSettings;
      RequestId = Interlocked.Increment(ref __lastRequestId);
    }



    // public properties
    public int MessageLength
    {
      get { return _messageLength; }
      set { _messageLength = value; }
    }

    public int RequestId
    {
      get { return _requestId; }
      set { _requestId = value; }
    }

    public int ResponseTo
    {
      get { return _responseTo; }
      set { _responseTo = value; }
    }

    public MessageOpcode Opcode
    {
      get { return _opcode; }
      set { _opcode = value; }
    }

    public MessageFlags Flags
    {
      get { return _flags; }
      set { _flags = value; }
    }

    public int ObservableId
    {
      get { return _observableId; }
      set { _observableId = value; }
    }

    public BsonBinaryWriterSettings WriterSettings
    {
      get { return _writerSettings; }
    }

    public T Message
    {
      get { return _message; }
      set { _message = value; }
    }



    // internal methods
    public void WriteTo(Stream stream)
    {
      if (_messageStartPosition != -1) return;

      var streamWriter = new BsonBinaryWriter(stream);
      _messageStartPosition = (int)stream.Position;
      WriteMessageHeaderTo(streamWriter);
      var objectCount = WriteBodyTo(streamWriter);
      BackpatchMessageLength(stream);
      BackpatchObjectCount(stream, objectCount);
    }

    // protected methods
    protected void WriteMessageHeaderTo(BsonBinaryWriter streamWriter)
    {
      streamWriter.BsonStream.WriteInt32(0); // messageLength will be backpatched later
      streamWriter.BsonStream.WriteInt32(RequestId);
      streamWriter.BsonStream.WriteInt32(ResponseTo);
      streamWriter.BsonStream.WriteInt32((int)Opcode);
      streamWriter.BsonStream.WriteInt32((int)Flags);
      streamWriter.BsonStream.WriteInt32(ObservableId);
      streamWriter.BsonStream.WriteInt32(0); // messageObjectCount will be backpatched later
    }

    protected int WriteBodyTo(BsonBinaryWriter streamWriter)
    {
      if (_message == null)
        return 0;

      var objectCount = 0;
      using (var bsonWriter = new BsonBinaryWriter(streamWriter.BaseStream, _writerSettings))
      {
        var message = _message;
        //foreach (var obj in Objects)
        {
          //bsonWriter.CheckElementNames = false;
          BsonSerializer.Serialize(bsonWriter, typeof(T), message);//, b => b.SerializeAsNominalType = true); // b.SerializeIdFirst = true
          objectCount++;
        }
      }
      return objectCount;
    }

    protected void BackpatchMessageLength(Stream stream)
    {
      MessageLength = (int)(stream.Position - _messageStartPosition);
      Backpatch(stream, _messageStartPosition, MessageLength);
    }

    protected void BackpatchObjectCount(Stream stream, int objectCount)
    {
      MessageLength = (int)(stream.Position - _messageStartPosition);
      Backpatch(stream, _messageStartPosition + 4 + 4 + 4 + 4 + 4 + 4, objectCount);
    }

    // private methods
    private void Backpatch(Stream stream, int position, int value)
    {
      var streamWriter = new BsonBinaryWriter(stream);
      var currentPosition = stream.Position;
      stream.Position = position;
      streamWriter.BsonStream.WriteInt32(value);
      stream.Position = currentPosition;
    }

  }
}
