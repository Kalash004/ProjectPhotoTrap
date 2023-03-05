using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ProjectPhotoTrap
{
    internal class SerialPortConnection
    {
        private String portName;
        private int baudRate;
        private int _DATA_BITS = 8;
        private SerialPort _serialPort;
        private List<IPortMessage> subs = new List<IPortMessage>();
        private String FACE_DETECTION_STRING = "Face Detceted :";
        private String SERVER_LINK = "Use 'http:";
        private int timeout;
        private static bool isOpened = false;
        private int COOLDOWN_FOR_RETRYING_TO_CONNECT = 1000;

        // currently 115200
        public SerialPortConnection(String port, int baudeRate, int timeout)
        {
            Initiate(port, baudeRate, timeout);
        }

        public void Initiate(String port, int baudeRate, int timeout)
        {
            while (!isOpened)
            {
                _serialPort = new SerialPort();
                _serialPort.PortName = port;
                _serialPort.BaudRate = baudeRate;
                _serialPort.WriteTimeout = timeout;
                this.timeout = timeout;
                try
                {
                    _serialPort.Open();
                    isOpened = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Wasnt able to open port err: " + e.Message + "\nretrying");
                    Thread.Sleep(COOLDOWN_FOR_RETRYING_TO_CONNECT);
                    isOpened = false;
                }
            }
        }



        public void Close()
        {
            _serialPort.Close();
        }

        public void AddSub(IPortMessage obj)
        {
            this.subs.Add(obj);
        }

        public void StartReading()
        {
            Read();
        }

        public String Read()
        {
            while (true)
            {
                String str = _serialPort.ReadExisting();
                if (!IsFaceDetected(str))
                {
                    IsServerConnection(str);
                }

            }
        }

        private bool IsServerConnection(string str)
        {
            if (!str.Contains(SERVER_LINK))
            {
                return false;
            }
            foreach (IPortMessage obj in subs)
            {
                obj.DataRead(this, str, Type.ServerLink);
                Thread.Sleep(timeout);
            }
            return true;
        }

        public bool IsFaceDetected(String str)
        {
            if (!str.Contains(FACE_DETECTION_STRING))
            {
                return false;
            }
            foreach (IPortMessage obj in subs)
            {
                obj.DataRead(this, str, Type.FaceDetected);
                Thread.Sleep(timeout);
            }
            return true;
        }
    }
}
