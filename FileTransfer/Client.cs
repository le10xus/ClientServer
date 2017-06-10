using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using static FileTransfer.Files;

namespace FileTransfer
{
    class ClientConnection : UdpTransfer, IClientConnection
    {
        public string Status { get; set; }

        /// <summary>
        /// Установка соединения
        /// </summary>
        override public void Connection()
        {
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
            //подключение клиента
            CreateConnection();

            Status = "Соединение установлено";

        }

        /// <summary>
        /// Подключить клиента
        /// </summary>
        void CreateConnection()
        {
            try
            {
                udpStream = new UdpClient();

                // IP-адрес и создаем IPEndPoint
                remoteIPAddress = IPAddress.Parse(HostName);
                endPoint = new IPEndPoint(remoteIPAddress, Port);
            }
            catch
            {
                Status = "Сервер не отвечает!";
                MessageBox.Show(Status);
                return;
            }
        }

        /// <summary>
        /// Скачать файл
        /// </summary>
        public void DownloadFile (Files file)
        {
            CreateConnection();
            
            FileStream fs = null;
            FileDetails fd = new FileDetails();
            file.FileName = "download_" + file.FileName;
            
            FileInfo(file, fd);
            try
            {
                Byte[] receiveBytes = new Byte[0];

                // чтение
                receiveBytes = udpStream.Receive(ref endPoint);
                string path = file.PathName;

                // запись в файл
                fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
                fs.Write(receiveBytes, 0, receiveBytes.Length);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                udpStream.Close();
                fs.Close();
            }

        }

        /// <summary>
        /// Загрузить файл на сервер
        /// </summary>
        public void UdpLoadFile(Files file, FileDetails fd)
        {
            CreateConnection();
            try
            {
                // Получаем путь файла и его размер (должен быть меньше 8kb)

                if (file.fileStream.Length > 8192)
                {
                    udpStream.Close();
                    file.fileStream.Close();
                    return;
                }
                
                // Отправляем информацию о файле
                FileInfo(file, fd);

                // Отправляем сам файл
                SendFile(file);

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
        /// Отправить детали о файле на сервер
        /// </summary>
        public void FileInfo(Files file, FileDetails fd)
        {
            // Получаем длину файла
            long size = 0;
            if (file.fileStream != null)
            {
                size = file.fileStream.Length;
            }
            fd.Size = size;
            fd.FileName = file.FileName;
            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream stream = new MemoryStream();

            // Сериализуем объект
            fileSerializer.Serialize(stream, fd);

            // Считываем поток в байты
            stream.Position = 0;
            Byte[] bytes = new Byte[stream.Length];
            stream.Read(bytes, 0, Convert.ToInt32(stream.Length));
                        
            // Отправляем информацию о файле
            udpStream.Send(bytes, bytes.Length, endPoint);
            stream.Close();
        }

        /// <summary>
        /// запрос информации о файле
        /// </summary>
        public void SendRequest(FileDetails fd)
        {
            CreateConnection();

            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream stream = new MemoryStream();

            // Сериализуем объект
            fileSerializer.Serialize(stream, fd);

            // Считываем поток в байты
            stream.Position = 0;
            Byte[] bytes = new Byte[stream.Length];
            stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

            // Отправляем информацию о файле
            udpStream.Send(bytes, bytes.Length, endPoint);

            stream.Close();
        }

        /// <summary>
        /// Отправка файла
        /// </summary>
        void SendFile(Files file)
        {
            //Принять информацию
            FileDetails getFile = new FileDetails();
            getFile = GetFileDetails();
            long size = 0;
            int offset = 0;
            int count = 0;

            // Перевод потока в байты
            Byte[] bytes = new Byte[file.fileStream.Length];
            count = bytes.Length;

            if (getFile.FileName != "null" && getFile.Size < count)
            {
                //докачать файл начиная с :
                size = getFile.Size; //размер файла
                offset = Convert.ToInt32(size); //начало в байтах
                count = bytes.Length - offset;// всего
            }
            else if(getFile.Size == count)
            {
                file.fileStream.Close();
                return;
            }

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

        /// <summary>
        /// прочитать информацию о файле
        /// </summary>
        override public FileDetails GetFileDetails()
        {
            FileDetails details = new FileDetails();
            // получить информацию
            byte [] receiveBytes = udpStream.Receive(ref endPoint);

            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream stream = new MemoryStream();

            // чтение информации о файле
            stream.Write(receiveBytes, 0, receiveBytes.Length);
            stream.Position = 0;

            // Deserialize
            details = (FileDetails)fileSerializer.Deserialize(stream);
            stream.Close();

            return details;
        }

        /// <summary>
        /// получить список файлов на сервере
        /// </summary>
        public List<FileDetails> GetFileList()
        {
            List<FileDetails> details = new List<FileDetails>();
            // получить информацию
            byte[] receiveBytes = udpStream.Receive(ref endPoint);

            XmlSerializer fileSerializer = new XmlSerializer(typeof(List<FileDetails>));
            MemoryStream stream = new MemoryStream();

            // чтение информации о файле
            stream.Write(receiveBytes, 0, receiveBytes.Length);
            stream.Position = 0;

            // Deserialize
            details = (List<FileDetails>)fileSerializer.Deserialize(stream);
            stream.Close();
            udpStream.Close();
            return details;
        }
    }
}
