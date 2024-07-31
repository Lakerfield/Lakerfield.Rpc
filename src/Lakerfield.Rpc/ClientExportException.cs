using System;
using System.Runtime.Serialization;

namespace Lakerfield.Rpc
{
  /// <summary>
  /// Represents a DrieNul exception.
  /// </summary>
  //[Serializable]
  public class LakerfieldRpcException : Exception
  {
    // constructors
    /// <summary>
    /// Initializes a new instance of the DrieNulException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public LakerfieldRpcException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DrieNulException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LakerfieldRpcException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    ///// <summary>
    ///// Initializes a new instance of the DrieNulException class (this overload supports deserialization).
    ///// </summary>
    ///// <param name="info">The SerializationInfo.</param>
    ///// <param name="context">The StreamingContext.</param>
    //public LakerfieldRpcException(SerializationInfo info, StreamingContext context)
    //  : base(info, context)
    //{
    //}
  }
}
