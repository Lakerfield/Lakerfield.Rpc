using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

// TODO: https://github.com/mongodb/mongo-csharp-driver/commit/068438c60f091c9412e85870e80813b5a755c8f5

namespace Lakerfield.Rpc
{
  public class DrieNulReceiveMessage<T> where T : class
  {

    // private fields
    private readonly BsonBinaryReaderSettings _readerSettings;
    private readonly IBsonSerializer _serializer;
    private int _messageLength;
    private int _requestId;
    private int _responseTo;
    private MessageOpcode _opcode;
    private MessageFlags _flags;
    private int _observableId;
    private int _objectCount;
    private T _message;



    // constructor
    public DrieNulReceiveMessage(BsonBinaryReaderSettings readerSettings = null, IBsonSerializer serializer = null)
    {
      if (readerSettings == null)
      {
        readerSettings = BsonBinaryReaderSettings.Defaults;
        //throw new ArgumentNullException("readerSettings");
      }
      if (serializer == null)
      {
        serializer = BsonSerializer.LookupSerializer(typeof(T));
        //throw new ArgumentNullException("serializer");
      }

      _readerSettings = readerSettings;
      _serializer = serializer;
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

    public BsonBinaryReaderSettings ReaderSettings
    {
      get { return _readerSettings; }
    }

    public T Message
    {
      get { return _message; }
      set { _message = value; }
    }


    // internal methods
    public void ReadFrom(Stream stream, int messageLength = -1)
    {
      var streamReader = new BsonBinaryReader(stream);
      ReadMessageHeaderFrom(streamReader, messageLength);

      if ((Flags & MessageFlags.Exception) != 0)
      {
        //BsonDocument document;
        //using (BsonReader bsonReader = new BsonBinaryReader(stream, _readerSettings))
        //{
        //  var context = BsonDeserializationContext.CreateRoot<BsonDocument>(bsonReader, b => b.AllowDuplicateElementNames = true);
        //  document = BsonDocumentSerializer.Instance.Deserialize(context);
        //}
        //var err = document.GetValue("$err", "Unknown error.");
        //var message = string.Format("QueryFailure flag was {0} (response was {1}).", err, document.ToJson());
        //throw new MongoQueryException(message, document);
      }

      //_objects = new List<T>(_objectCount);
      for (int i = 0; i < _objectCount; i++)
      {
        using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
        {
          var context = BsonDeserializationContext.CreateRoot(bsonReader, b => b.AllowDuplicateElementNames = false);
          var obj = _serializer.Deserialize(context) as T;
          _message = obj;
          //if (obj != null)
          //  _objects.Add(obj);
        }
      }
    }



    // protected methods
    protected void ReadMessageHeaderFrom(BsonBinaryReader streamReader, int messageLength)
    {
      MessageLength = messageLength >= 0 ? messageLength : streamReader.ReadInt32();
      RequestId = streamReader.ReadInt32();
      ResponseTo = streamReader.ReadInt32();
      Opcode = (MessageOpcode)streamReader.ReadInt32();
      Flags = (MessageFlags)streamReader.ReadInt32();
      ObservableId = streamReader.ReadInt32();
      _objectCount = streamReader.ReadInt32();
    }



  }
}
