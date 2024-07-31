using System;
using System.Runtime.Serialization;

namespace Lakerfield.Rpc
{
  /// <summary>
  /// Represents a Lakerfield Rpc Connection Exception.
  /// </summary>
  //[Serializable]
  public class LakerfieldRpcConnectionException : LakerfieldRpcException
  {
    // constructors
    /// <summary>
    /// Initializes a new instance of the LakerfieldRpcConnectionException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public LakerfieldRpcConnectionException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the LakerfieldRpcConnectionException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LakerfieldRpcConnectionException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    ///// <summary>
    ///// Initializes a new instance of the LakerfieldRpcConnectionException class (this overload supports deserialization).
    ///// </summary>
    ///// <param name="info">The SerializationInfo.</param>
    ///// <param name="context">The StreamingContext.</param>
    //public LakerfieldRpcConnectionException(SerializationInfo info, StreamingContext context)
    //  : base(info, context)
    //{
    //}
  }
}
