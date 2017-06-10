using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class ServerForm : Form
    {
        Server conn;
        public ServerForm()
        {
            InitializeComponent();
        
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            conn = new Server();

            conn.Port = Int32.Parse(tbPort.Text);
            conn.HostName = tbIpAddress.Text;

            conn.Connection();

        }
    }
}
