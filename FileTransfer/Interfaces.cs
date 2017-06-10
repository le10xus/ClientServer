using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfer
{
    public interface IClientConnection
    {
        int Port { get; set; }
        string HostName { get; set; }
        void Connection();
    }

    public interface IServerConnection
    {
        int Port { get; set; }
        string HostName { get; set; }
        void Connection();
    }
}
