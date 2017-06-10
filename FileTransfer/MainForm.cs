using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FileTransfer.Files;

namespace FileTransfer
{
    public partial class MainForm : Form
    {
        //Подключение к серверу
        ClientConnection cc = new ClientConnection();

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            cc.Port = Int32.Parse(tbPort.Text);
            cc.HostName = tbIpAddress.Text;
            //cc.Connection();

            lbStatus.Text = cc.Status;
            bool flag = true;
            FileDetails request = new FileDetails();
            List<FileDetails> detList = new List<FileDetails>();
            request.FileName = "list";
            request.Size = 0;
            cc.SendRequest(request);

            while (flag)
            {
                detList = cc.GetFileList();
                flag = false;
            }
            
            for (int i = 0; i < detList.Count; i++)
            {
                lbFileList.Items.Add(detList[i].FileName);
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            Files files = new Files();
            OpenFile of = new OpenFile();
            FileDetails fd = new FileDetails();
            of.ChooseFile(files);
            
            cc.UdpLoadFile(files, fd);
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            string value = "";
            if (lbFileList.SelectedItem != null)
            {
                value = lbFileList.SelectedItem.ToString();

                Files files = new Files();
                OpenFile of = new OpenFile();

                files = of.SaveFile();
                files.FileName = value;
                
                cc.DownloadFile(files);                
            }
            else
            {
                MessageBox.Show("Выберите файл для скачивания");
            }

        }
    }
}
