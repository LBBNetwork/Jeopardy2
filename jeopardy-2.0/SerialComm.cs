using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows;

namespace jeopardy_2._0
{
    class SerialComm
    {
        //COMPort = new SerialPort();

        public static String[] GetSerialPorts()
        {
            String[] COMPort = SerialPort.GetPortNames();

            foreach (string port in COMPort)
            {
                MessageBox.Show(port);
            }

            return COMPort;
        }
    }
}
