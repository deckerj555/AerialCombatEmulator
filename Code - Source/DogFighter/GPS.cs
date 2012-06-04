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
    class GPS
    {
        // Declarations
        private SerialPort physicalSerialPort;
        private const int rate = 38400; //default for venus gps = 9600, //when venus set for 10hz, 38400 is used.
        private byte[] bigBuffer = new byte[512];   //OG line
        private int tailIndex = 0;
        private byte[] completeLine = new byte[59];

        

        // Constructor
        public GPS()
        {
            physicalSerialPort = new SerialPort("COM2", rate, Parity.None, 8, StopBits.One);
            physicalSerialPort.DataReceived += new SerialDataReceivedEventHandler(physicalSerialPort_DataReceived);
            physicalSerialPort.Open();
        }

        // Events Setup
        public delegate void LineGPSDelegate(LineGPS shitIn);
        public event LineGPSDelegate ReceivedLineGPS;

        // DataRecieved Event Handler "Method"
        void physicalSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesPutIn = physicalSerialPort.Read(bigBuffer, tailIndex, bigBuffer.Length - tailIndex);
            tailIndex += bytesPutIn;
            byte[] header = {0xA0, 0xA1, 0x00, 0x3B, 0xA8};

            int headerFound = 0;

            for (int i = 0; i < tailIndex - 4; i++)
            {
                if (bigBuffer[i] == header[0] && bigBuffer[i+4] == header[4])
                { // See if we're lucky and have a whole header, if so, declare full header and go to next step.
                    if (i != 0) // keeps you from copying if you're lucky enough to have the header start at the start of bigBuffer
                    {
                        Array.Copy(bigBuffer, i, bigBuffer, 0, tailIndex);
                        tailIndex = tailIndex - i;
                    }

                    headerFound = 1;
                }

                else if (bigBuffer[i] == header[0] && i > 0)
                { // If not lucky, search for start of header and copy to the begining, then break to go get more bytesRead
                    Array.Copy(bigBuffer, i, bigBuffer, 0, tailIndex);
                    tailIndex = tailIndex - i;
                    break;
                }

                if (headerFound == 1 && tailIndex > 58)
                { // If you have a full header, and enough data to expect a full line, assign the line
                    Array.Copy(bigBuffer, 4, completeLine, 0, 59); // 4 used so that message will start with A8, the Message ID
                    Array.Copy(bigBuffer, 59, bigBuffer, 0, tailIndex);
                    tailIndex = tailIndex - 59;

                    LineGPS line = new LineGPS(completeLine);
                    if (this.ReceivedLineGPS != null)
                    {
                        this.ReceivedLineGPS(line);
                    }
                }
                else if (headerFound == 1)
                { // if you have a full header, but not enough data for a full line, stop the loop and go get more data.
                    break;
                }     
            }
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
