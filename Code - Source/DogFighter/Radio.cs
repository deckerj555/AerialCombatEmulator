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
    class Radio
    {
        // Declarations
        private SerialPort physicalSerialPort;
        private const int rate = 38400;
        private byte[] attNavHeader = { 0xB0, 0xB1 };
        private byte[] attNavFooter = { 0x0D, 0x0A };
        private bool attNavHeaderFound = false;
        private byte[] completeAttNav;
        private byte[] bigBuffer = new byte[128];   //i made that number up
        private int tailIndex = 0;
        private int sizeAttNav = 0;
        private byte[] dlcHeader = { 0x44, 0x4C, 0x43 }; // this is just DLC in hex.
        private byte[] dlcFooter = { 0x0D, 0x0A };
        private bool dlcHeaderFound;
        private byte[] completeDLC;
        private int sizeDLC = 0;
        private Led deadLed;
        private GPSIMU forDebugPrintOut;

        // Events Setup
        public delegate void AttNavDelegate(AttNav positionEnemy);
        public event AttNavDelegate ReceivedAttNavEnemy;
        public delegate void DLCReceivedDelegate(DeadLedControl dlcToMe);
        public event DLCReceivedDelegate ReceivedImDeadDLC;


        // Constructor
        public Radio()
        {
            physicalSerialPort = new SerialPort("COM2", rate, Parity.None, 8, StopBits.One);
            physicalSerialPort.DataReceived += new SerialDataReceivedEventHandler(physicalSerialPort_DataReceived);
            physicalSerialPort.Open();

            sizeAttNav = AttNav.Serialize(new AttNav()).Length; // Get size of a serialized AttNav
            completeAttNav = new byte[sizeAttNav];
            sizeDLC = DeadLedControl.Serialize(new DeadLedControl()).Length;
            completeDLC = new byte[sizeDLC];
        }

        public void Initialize(PositionComputer positionComputer, FiringSolution firingSolution, Led deadLed, GPSIMU forDebugPrintOut)
        {
            this.deadLed = deadLed;
            this.forDebugPrintOut = forDebugPrintOut;
            positionComputer.AttNavCreated += new PositionComputer.AttNavDataReadyDelegate(positionComputer_AttNavCreated);
            firingSolution.DLCCreated += new FiringSolution.DLCCreatedDelegate(firingSolution_DLCCreated);
        }

        // DataRecieved Event Handler "Method"
        void physicalSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Read in data with physicalSerialPort.Read()
            int bytesFromRadio = physicalSerialPort.Read(bigBuffer, tailIndex, bigBuffer.Length - tailIndex);
            tailIndex += bytesFromRadio;
            parse();
        }

        public void HelloWorld(byte[] helloWorld)
        {
            physicalSerialPort.Write(helloWorld, 0, helloWorld.Length);

        }

        public void InsertTestBuffer(byte[] testBuffer)
        {
            Array.Copy(testBuffer, 0, bigBuffer, tailIndex, System.Math.Min((bigBuffer.Length - tailIndex), testBuffer.Length));
            tailIndex += System.Math.Min((bigBuffer.Length - tailIndex), testBuffer.Length);
            parse();
        }

        private void parse()
        {
            // The next two lines should be put in the initializer.  We don't need to calculate msgLength every time we parse, just once per instance. (LTN and JPD 5/18/2012)
            int msgLengthAttNav = attNavHeader.Length + sizeAttNav + 2 + attNavFooter.Length; //header + attnav + checksum + footer 
            int msgLengthDLC = dlcHeader.Length + sizeDLC + 2 + dlcFooter.Length; // header + dlc + checksum + footer



            //1) Identify a header, 2) enough data for a full msg of that header type 3) footer 4) checksum = 0;
            if (attNavHeaderFound == false && dlcHeaderFound == false)
            {
                for (int i = 0; i < tailIndex; i++)
                {
                    if (i <= tailIndex - dlcHeader.Length) // Just have the check for dlcHeaderLength, because it's longer, so if there are enough bytes for a dlc header, there are enough for an attnav not to overrun the index
                    {
                        if (bigBuffer[i] == attNavHeader[0] && bigBuffer[i + attNavHeader.Length - 1] == attNavHeader[attNavHeader.Length - 1])
                        {
                            tailIndex = tailIndex - i;
                            Array.Copy(bigBuffer, i, bigBuffer, 0, tailIndex);
                            attNavHeaderFound = true;
                            break;
                        }

                        if (bigBuffer[i] == dlcHeader[0] && bigBuffer[i + dlcHeader.Length - 1] == dlcHeader[dlcHeader.Length - 1])
                        {
                            tailIndex = tailIndex - i;
                            Array.Copy(bigBuffer, i, bigBuffer, 0, tailIndex);
                            dlcHeaderFound = true;
                            break;
                        }

                    }
                    else
                    {
                        if (i > 0)
                        {//if no headers found, copy what's left back to the start of bigbuffer, and wait for more data...
                            tailIndex = tailIndex - i;
                            Array.Copy(bigBuffer, i, bigBuffer, 0, tailIndex);
                            break;
                        }
                    }
                }
            }

            // Have AttNavHeader and Plenty of Data? --> Create Attnav, or throw it away if can't find a footer.
            if (attNavHeaderFound == true && tailIndex >= msgLengthAttNav)
            {
                if (bigBuffer[attNavHeader.Length + sizeAttNav + 2] == attNavFooter[0] &&
                    bigBuffer[attNavHeader.Length + sizeAttNav + 3] == attNavFooter[1])
                {
                    //if the checksum returns zero
                    byte[] forCheckSumCalc = new byte[sizeAttNav + 2];
                    Array.Copy(bigBuffer, attNavHeader.Length, forCheckSumCalc, 0, forCheckSumCalc.Length);
                    if (CheckSumCalc(forCheckSumCalc) == 0)
                    {
                        Array.Copy(forCheckSumCalc, 0, completeAttNav, 0, sizeAttNav);
                        AttNav positionEnemy = AttNav.Deserialize(completeAttNav);
                        if (this.ReceivedAttNavEnemy != null)
                        {
                            this.ReceivedAttNavEnemy(positionEnemy); // Throwing the Event
                        }
                    }
                }
                tailIndex = tailIndex - msgLengthAttNav;
                Array.Copy(bigBuffer, msgLengthAttNav, bigBuffer, 0, tailIndex);
                attNavHeaderFound = false;
            }

            // Have dlcHeader and plenty of data? --> Create a DLC, or throw it away if can't find a footer.
            if (dlcHeaderFound == true && tailIndex >= msgLengthDLC)
            {
                if (bigBuffer[dlcHeader.Length + sizeDLC + 2] == dlcFooter[0] &&
                    bigBuffer[dlcHeader.Length + sizeDLC + 3] == dlcFooter[1])
                {
                    //if checksum returns zero
                    byte[] forCheckSumCalc = new byte[sizeDLC + 2];
                    Array.Copy(bigBuffer, dlcHeader.Length, forCheckSumCalc, 0, forCheckSumCalc.Length);
                    if (CheckSumCalc(forCheckSumCalc) == 0)
                    {
                        Array.Copy(forCheckSumCalc, 0, completeDLC, 0, sizeDLC);
                        DeadLedControl dlcToMe = DeadLedControl.Deserialize(completeDLC);
                        if (this.ReceivedImDeadDLC != null)
                        {
                            this.ReceivedImDeadDLC(dlcToMe);
                        }
                        // Control the LED here, as the dlc comes in.  In future, can move this to firing solution if desired so that it runs on clockcycle.
                        if (dlcToMe.ControllingInt == 1)
                        {
                            deadLed.On(); // We could make the DLC represent how far off a shot is, and then blink accordingly.  But for now, we'll just treat it like a bool - LTN 4/1/12
                        }
                        else
                        {
                            deadLed.Off();
                        }
                    }
                }
                tailIndex = tailIndex - msgLengthDLC;
                Array.Copy(bigBuffer, msgLengthDLC, bigBuffer, 0, tailIndex); // Every now and then I get a system....outOfRange exception on this line,  because tailindex has somehow become -5.  This makes me think maybe we need to lock bigbuffer somehowwhen we're doing Array.copy functions.  All of this testing is with fakeradiostream and insertTestBuffer, so maybe that's the problem, and we won't have it with the physicalSerialPort Data Receieved event handler.  To be tested. 
                dlcHeaderFound = false;
            }
        } // end Parse

        // AttNavCreated Event Handler to send positionMe to the other plane
        void positionComputer_AttNavCreated(AttNav positionMe)
        {
            byte[] buffer = AttNav.Serialize(positionMe);
            UInt16 checkSum = CheckSumCalc(buffer);

            // Convert the uInt16 checksum to a byte[]
            byte[] checkSumBuffer = new byte[2];
            checkSumBuffer[0] = (byte)(checkSum >> 8);
            checkSumBuffer[1] = (byte)(checkSum); // Casting (byte)(checkSum) just took the least significant (second half of the 16 bits) byte and put it in byte[1] 

            physicalSerialPort.Write(attNavHeader, 0, attNavHeader.Length);
            physicalSerialPort.Write(buffer, 0, buffer.Length);
            physicalSerialPort.Write(checkSumBuffer, 0, checkSumBuffer.Length);
            physicalSerialPort.Write(attNavFooter, 0, attNavFooter.Length);
        }

        // DLCCreated Event Handler to send dlcToEnemy to other plane
        void firingSolution_DLCCreated(DeadLedControl dlcToEnemy)
        {
            byte[] dlcBuffer = DeadLedControl.Serialize(dlcToEnemy);
            UInt16 checkSum = CheckSumCalc(dlcBuffer);

            // Convert the uInt16 checksum to a byte[]
            byte[] checkSumBuffer = new byte[2];
            checkSumBuffer[0] = (byte)(checkSum >> 8);
            checkSumBuffer[1] = (byte)(checkSum);

            physicalSerialPort.Write(dlcHeader, 0, dlcHeader.Length);
            physicalSerialPort.Write(dlcBuffer, 0, dlcBuffer.Length);
            physicalSerialPort.Write(checkSumBuffer, 0, checkSumBuffer.Length);
            physicalSerialPort.Write(dlcFooter, 0, dlcFooter.Length);
        }

        public static UInt16 CheckSumCalc(byte[] toCalc)
        {
            UInt16 checkSum = 0;
            int i = 0;

            for (; i < toCalc.Length - 1; i += 2)
            {
                checkSum += (UInt16)((UInt16)toCalc[i] << 8);
                checkSum += (UInt16)toCalc[i + 1];
            }

            if (i < toCalc.Length)
            {
                checkSum += (UInt16)toCalc[i];
            }

            checkSum = (UInt16)(~checkSum + 1); // This is a 2'sCompliment

            return checkSum;
        }

        public void TerminalPrintOut2(string toPrint)
        {
            // Print data out the Tx line to the terminal
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            byte[] bytesToSend = encoder.GetBytes(toPrint);
            physicalSerialPort.Write(bytesToSend, 0, bytesToSend.Length);
        }
    }
}
