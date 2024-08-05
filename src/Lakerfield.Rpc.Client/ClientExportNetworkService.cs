using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lakerfield.Rpc.Helpers;

namespace Lakerfield.Rpc
{
  public class ClientExportNetworkService //: IClientExportService
  {
    private NetworkClient _networkClient;
    public Task<string> Connected { get { return _networkClient.Connected; } }
    public string ApplicationName { get { return "ClientExport.Client"; } }

    //public IEktCollection Ekt { get; private set; }

    /*
    public Task Log(LogLevel logLevel, string message, params object[] args)
    {
      return Log(LogHelper.ConvertToLogObject(null, logLevel, null, message, args));
    }

    public Task Log(LogLevel logLevel, Exception exception, string message, params object[] args)
    {
      return Log(LogHelper.ConvertToLogObject(null, logLevel, exception, message, args));
    }

    public async Task Log(Log log)
    {
      var request = new Messages.LogRequest() { Log = log };
      var response = await _networkClient.Execute<Messages.LogResponse>(request).ConfigureAwait(false);
    }*/

    private Task _pingTask;

    public ClientExportNetworkService(string hostname)
    {
      _networkClient = new NetworkClient(hostname);

      //Ekt = new Collections.EktCollection(_networkClient);

      _pingTask = StartPing();
    }

    private async Task StartPing()
    {
      try
      {
        while (true)
        {
          //await System.Ping().ConfigureAwait(false);
          await Task.Delay(60000).ConfigureAwait(false);
        }
      }
      catch (Exception)
      {
        // TODO: Set error state...
        throw;
      }
    }

  }
}
