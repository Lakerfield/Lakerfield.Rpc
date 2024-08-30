using System;
using System.Threading.Tasks;
using Lakerfield.Rpc.Models;

namespace Lakerfield.Rpc;

public partial interface ISampleRpcService
{

    Task<Mailbox> MailboxFindById(Guid id);
    Task<Mailbox[]> MailboxFindAll();
    Task<Mailbox> MailboxSave(Mailbox entity);
    Task MailboxDelete(Mailbox entity);
    
}
