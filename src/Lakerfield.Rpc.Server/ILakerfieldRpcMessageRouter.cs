using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lakerfield.Rpc
{

  public interface ILakerfieldRpcMessageRouter
  {
    Task<RpcMessage> HandleMessage(RpcMessage message);
    NetworkObservable HandleObservable(RpcMessage message);
  }



}
