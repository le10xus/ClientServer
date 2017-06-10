using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileTransfer
{
    public abstract class UdpTransfer
    {
        public static UdpClient udpStream;
        public static IPEndPoint endPoint = null;
        public static IPAddress remoteIPAddress = null;
        public int Port { get; set; } //номер порта
        public string HostName { get; set; } //ip адрес

        abstract public void Connection();
        abstract public FileDetails GetFileDetails();

    }
}
