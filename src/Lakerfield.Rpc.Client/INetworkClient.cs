using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lakerfield.Rpc
{
  internal interface INetworkClient
  {
    Task<T> Execute<T>(RpcMessage message) where T : RpcMessage;
    IObservable<T> ExecuteObservable<T>(RpcMessage message);
  }
}
