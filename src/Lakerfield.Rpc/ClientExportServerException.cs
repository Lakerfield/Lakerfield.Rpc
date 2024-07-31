using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Lakerfield.Rpc
{
  /// <summary>
  /// Represents a DrieNul server exception.
  /// </summary>
  //[Serializable]
  public class LakerfieldRpcServerException : LakerfieldRpcException
  {
    public const string StacktraceServerKey = @"StacktraceServer";

    // constructors
    /// <summary>
    /// Initializes a new instance of the DrieNulConnectionException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public LakerfieldRpcServerException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DrieNulConnectionException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LakerfieldRpcServerException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    ///// <summary>
    ///// Initializes a new instance of the DrieNulConnectionException class (this overload supports deserialization).
    ///// </summary>
    ///// <param name="info">The SerializationInfo.</param>
    ///// <param name="context">The StreamingContext.</param>
    //public LakerfieldRpcServerException(SerializationInfo info, StreamingContext context)
    //  : base(info, context)
    //{
    //}

    public string StackTraceServer
    {
      get
      {
        var result = Data[StacktraceServerKey];
        if (result == null)
          return null;
        return result.ToString();
      }
    }

  }
}
