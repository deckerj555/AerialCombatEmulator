using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;
using System.Text;

namespace DogFighter
{
    class IMU
    {
        // Declarations
        private SerialPort physicalSerialPort;
        private const int rate = 38400;
        private string bigBuffer = "";

        // Constructor
        public IMU()
        {
            physicalSerialPort = new SerialPort("COM1", rate, Parity.None, 8, StopBits.One);
            physicalSerialPort.DataReceived += new SerialDataReceivedEventHandler(physicalSerialPort_DataReceived);
            physicalSerialPort.Open();
        }

        // Events Setup
        public delegate void LineIMUDelegate(LineIMU line);
        public event LineIMUDelegate ReceivedLineIMU;

        // DataRecieved Event Handler "Method"
        void physicalSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[1024];
            int lineStop = 0;

            physicalSerialPort.Read(buffer, 0, physicalSerialPort.BytesToRead < buffer.Length ? physicalSerialPort.BytesToRead : buffer.Length);

            try
            {
                bigBuffer += new String(Encoding.UTF8.GetChars(buffer));
            }
            catch (Exception)
            {
                bigBuffer = "";
                return;
            }

            do
            {
                lineStop = bigBuffer.IndexOf('\n');
                if (lineStop != -1)
                {
                    int lineStart = bigBuffer.LastIndexOf('!', 0, lineStop);

                    if (lineStart != -1)
                    {
                        //LineIMU line = new LineIMU(bigBuffer.Substring(lineStart + 1, lineStop - lineStart - 2));
                        
                        if (this.ReceivedLineIMU != null)
                        {
                        //    this.ReceivedLineIMU(line);
                        }
                    }

                    bigBuffer = bigBuffer.Substring(lineStop + 1);
                }
                else if (bigBuffer.Length >= LineIMU.LineMaxChars() * 2)
                {
                    bigBuffer = "";
                }
            } while (lineStop != -1);
        }

        public void TerminalPrintOut(string toPrint)
        {
            // Print data out the Tx line to the terminal
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            byte[] bytesToSend = encoder.GetBytes(toPrint);
            physicalSerialPort.Write(bytesToSend, 0, bytesToSend.Length);
        }
    }
}
