using System;
using System.Threading.Tasks;
using Lakerfield.Rpc.Models;

namespace Lakerfield.Rpc;

public partial interface ISampleRpcService
{

    Task<Mailbox> DomainFindById(Guid id);
    Task<Mailbox[]> DomainFindAll();
    Task<Mailbox> DomainSave(Mailbox entity);
    Task DomainDelete(Mailbox entity);

}
