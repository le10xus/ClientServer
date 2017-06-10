using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace FileTransfer
{
    public class Files
    {
        public string FileName { get; set; }
        public string PathName { get; set; }
        public int FileId { get; set; }
        public FileStream fileStream = null;
    }

    [Serializable]
    public class FileDetails
    {
        public long Size = 0;
        public string FileName { get; set; }
    }

    public class OpenFile
    { 
        /// <summary>
        /// Выбрать файл
        /// </summary>
        public string ChooseFile(Files file)
        {          
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt) | *.txt";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                file.FileName = Path.GetFileName(ofd.FileName);
                file.PathName = Path.GetDirectoryName(file.FileName);
                file.fileStream = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
            }
            
            return file.PathName;
        }

        /// <summary>
        /// Сохранить файл
        /// </summary>
        public Files SaveFile()
        {
            Files file = new Files();

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "txt files (*.txt) | *.txt";
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                file.PathName = sfd.FileName;
                file.fileStream = null;
            }

            return file;
        }
    }


}
