using System;

namespace Lakerfield.Rpc;

public abstract class RpcMessage
{
}

public class RpcExceptionMessage : RpcMessage
{
    public string Message { get; set; }
    public string Stacktrace { get; set; }
}

public class RpcObservableMessage : RpcMessage
{
    public object Value { get; set; }
}