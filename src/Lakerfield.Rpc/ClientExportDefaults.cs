using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Lakerfield.Rpc
{
  /// <summary>
  /// Default values for various ClientExport settings.
  /// </summary>
  public static class ClientExportDefaults
  {
    // private static fields
    private static string __authenticationMechanism = "CLIENTEXPORT-CR";
    private static TimeSpan __connectTimeout = TimeSpan.FromSeconds(30);
    private static TimeSpan __maxConnectionIdleTime = TimeSpan.FromMinutes(10);
    private static TimeSpan __maxConnectionLifeTime = TimeSpan.FromMinutes(30);
    private static int __maxConnectionPoolSize = 10;
    private static int __maxMessageLength = 16000000; // 16MB (not 16 MiB!)
    private static int __minConnectionPoolSize = 0;
    private static TimeSpan __socketTimeout = TimeSpan.Zero; // use operating system default (presumably infinite)
    private static int __tcpReceiveBufferSize = 64 * 1024; // 64KiB (note: larger than 2MiB fails on Mac using Mono)
    private static int __tcpSendBufferSize = 64 * 1024; // 64KiB (TODO: what is the optimum value for the buffers?)
    private static UTF8Encoding __readEncoding = new UTF8Encoding(false, true);
    private static UTF8Encoding __writeEncoding = new UTF8Encoding(false, true);

    /// <summary>
    /// Gets or sets the default authentication mechanism.
    /// </summary>
    public static string AuthenticationMechanism
    {
      get { return __authenticationMechanism; }
      set { __authenticationMechanism = value; }
    }

    /// <summary>
    /// Gets or sets the connect timeout.
    /// </summary>
    public static TimeSpan ConnectTimeout
    {
      get { return __connectTimeout; }
      set { __connectTimeout = value; }
    }

    /// <summary>
    /// Gets or sets the representation to use for Guids (this is an alias for BsonDefaults.GuidRepresentation).
    /// </summary>
    public static GuidRepresentation GuidRepresentation
    {
      get { return BsonDefaults.GuidRepresentation; }
      set { BsonDefaults.GuidRepresentation = value; }
    }

    /// <summary>
    /// Gets or sets the max connection idle time.
    /// </summary>
    public static TimeSpan MaxConnectionIdleTime
    {
      get { return __maxConnectionIdleTime; }
      set { __maxConnectionIdleTime = value; }
    }

    /// <summary>
    /// Gets or sets the max connection life time.
    /// </summary>
    public static TimeSpan MaxConnectionLifeTime
    {
      get { return __maxConnectionLifeTime; }
      set { __maxConnectionLifeTime = value; }
    }


    /// <summary>
    /// Gets or sets the max connection pool size.
    /// </summary>
    public static int MaxConnectionPoolSize
    {
      get { return __maxConnectionPoolSize; }
      set { __maxConnectionPoolSize = value; }
    }

    /// <summary>
    /// Gets or sets the min connection pool size.
    /// </summary>
    public static int MinConnectionPoolSize
    {
      get { return __minConnectionPoolSize; }
      set { __minConnectionPoolSize = value; }
    }

    /// <summary>
    /// Gets or sets the max document size (this is an alias for BsonDefaults.MaxDocumentSize).
    /// </summary>
    public static int MaxDocumentSize
    {
      get { return BsonDefaults.MaxDocumentSize; }
      set { BsonDefaults.MaxDocumentSize = value; }
    }

    /// <summary>
    /// Gets or sets the max message length.
    /// </summary>
    public static int MaxMessageLength
    {
      get { return __maxMessageLength; }
      set { __maxMessageLength = value; }
    }

    /// <summary>
    /// Gets or sets the socket timeout.
    /// </summary>
    public static TimeSpan SocketTimeout
    {
      get { return __socketTimeout; }
      set { __socketTimeout = value; }
    }

    /// <summary>
    /// Gets or sets the TCP receive buffer size.
    /// </summary>
    public static int TcpReceiveBufferSize
    {
      get { return __tcpReceiveBufferSize; }
      set { __tcpReceiveBufferSize = value; }
    }

    /// <summary>
    /// Gets or sets the TCP send buffer size.
    /// </summary>
    public static int TcpSendBufferSize
    {
      get { return __tcpSendBufferSize; }
      set { __tcpSendBufferSize = value; }
    }

    /// <summary>
    /// Gets or sets the Read Encoding.
    /// </summary>
    public static UTF8Encoding ReadEncoding
    {
      get { return __readEncoding; }
      set
      {
        if (value == null)
        {
          throw new ArgumentNullException("value");
        }
        __readEncoding = value;
      }
    }

    /// <summary>
    /// Gets or sets the Write Encoding.
    /// </summary>
    public static UTF8Encoding WriteEncoding
    {
      get { return __writeEncoding; }
      set
      {
        if (value == null)
        {
          throw new ArgumentNullException("value");
        }
        __writeEncoding = value;
      }
    }





  }

}
