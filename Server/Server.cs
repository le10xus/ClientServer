using FileTransfer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Server
{
    public class Server : UdpTransfer, IServerConnection
    {
        public string ServerPath { get; set; }
        
        private static FileDetails file;

        private static FileStream fs = null;
        private static Byte[] receiveBytes = new Byte[0];

        override public void Connection()
        {
            ServerPath = @"C:\server files\";
            if (Port == 0)
            {
                MessageBox.Show("Введите номер порта!");
                return;
            }
            else if (HostName == null)
            {
                MessageBox.Show("Введите ip адрес!");
                return;
            }

            remoteIPAddress = IPAddress.Parse(HostName);// Конвертируем в другой формат

            Thread thread = new Thread(ServerConnection);
            thread.IsBackground = true;
            thread.Start();
           // ServerConnection();
        }        

        public void ServerConnection()
        {
            udpStream = new UdpClient(Port);
            try
            {
                while (true)
                {
                    string message = GetFileDetails().FileName;
                    // Получить информацию 
                    if (message == "list")
                    {
                        List<FileDetails> detailsList = new List<FileDetails>();
                        string[] filesInDir = Directory.GetFiles(ServerPath);

                        for (int i = 0; i < filesInDir.Length; i++)
                        {
                            string name = Path.GetFileName(filesInDir[i]);
                            detailsList.Add(new FileDetails() { FileName = name });
                        }
                        FileInfo(detailsList);
                    }
                    else if (message.Contains("download_"))
                    {
                        Files sendFile = new Files();
                        sendFile.FileName = message.Substring(9);
                        sendFile.fileStream = new FileStream(ServerPath + sendFile.FileName, FileMode.Open, FileAccess.Read);
                        // загрузить файл
                        SendFile(sendFile);
                    }
                    else
                    {
                        //Отправить данные
                        FileInfo(null);

                        // Получить файл
                        ReceiveFile();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                udpStream.Close();
            }
        }

        /// <summary>
        /// Отправка файла
        /// </summary>
        private static void SendFile(Files file)
        {
            // Перевод потока в байты
            Byte[] bytes = new Byte[file.fileStream.Length];

            file.fileStream.Read(bytes, 0, bytes.Length);

            try
            {
                // Отправляем файл
                udpStream.Send(bytes, bytes.Length, endPoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                // Закрываем соединение и очищаем поток
                file.fileStream.Close();
            }
        }

        override public FileDetails GetFileDetails()
        {
            // получить информацию
            receiveBytes = udpStream.Receive(ref endPoint);

            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream stream = new MemoryStream();

            // чтение информации о файле
            stream.Write(receiveBytes, 0, receiveBytes.Length);
            stream.Position = 0;

            // Вызываем метод Deserialize
            file = (FileDetails)fileSerializer.Deserialize(stream);
            stream.Close();

            return file;
        }

        public void ReceiveFile()
        {
            try
            {
                // чтение
                receiveBytes = udpStream.Receive(ref endPoint);
                string path = ServerPath + file.FileName;
                FileMode mode = FileMode.Create;
                FileAccess access = FileAccess.ReadWrite;
                int offset = 0;
                int count = receiveBytes.Length;
                //Если файл необходимо докачать
                if (File.Exists(path))
                {
                    FileInfo fi = new FileInfo(path);
                    if (Convert.ToInt32(fi.Length) != receiveBytes.Length)
                    {
                        mode = FileMode.Append;
                        access = FileAccess.Write;
                        offset = Convert.ToInt32(fi.Length);//начало 
                        count = receiveBytes.Length - offset;//длина
                    }
                }
                // запись в файл
                fs = new FileStream(path, mode, access, FileShare.ReadWrite);
                fs.Write(receiveBytes, offset, count);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                fs.Close();
            }
        }

        /// <summary>
        /// Отправка данных о существующем файле
        /// </summary>
        private void FileInfo(List<FileDetails> detailsList)
        {
            FileDetails fd = new FileDetails();
            XmlSerializer fileSerializer = null;
            Byte[] bytes = null;
            MemoryStream stream = new MemoryStream();

            //Если нужно отправить список
            if (detailsList!=null)
            {
                fileSerializer = new XmlSerializer(typeof(List<FileDetails>));

                // Сериализуем объект
                fileSerializer.Serialize(stream, detailsList);

                // Поток в байтах
                stream.Position = 0;
                bytes = new Byte[stream.Length];
                stream.Read(bytes, 0, Convert.ToInt32(stream.Length));
                                
                udpStream.Send(bytes, bytes.Length, endPoint);
                stream.Close();
                return;
            }

            string path = ServerPath + file.FileName;
            long size = 0;
            string text = "null";

            if (File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                size = fi.Length;
                text = file.FileName;
            }
            fd = new FileDetails();
            fd.Size = size;
            fd.FileName = text;

            fileSerializer = new XmlSerializer(typeof(FileDetails));

            // Сериализуем объект
            fileSerializer.Serialize(stream, fd);

            // поток в байтах
            stream.Position = 0;
            bytes = new Byte[stream.Length];
            stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

            udpStream.Send(bytes, bytes.Length, endPoint);
            stream.Close();
            
        }
    }
}
