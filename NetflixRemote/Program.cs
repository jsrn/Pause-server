using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace PauseServer
{
   class SynchronousSocketListener {

       [DllImport("User32.dll")]
       static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
       [DllImport("User32.dll")]
       static extern int SetForegroundWindow(IntPtr hWnd);
       [DllImport("User32.dll")]
       static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
       [DllImport("user32.dll")]
       private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
       [DllImport("kernel32.dll")]
       static extern IntPtr GetConsoleWindow();

    // Incoming data from the client.
    public static string data = null;

    public static void StartListening() {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // Establish the local endpoint for the socket.
        // Dns.GetHostName returns the name of the 
        // host running the application.
        IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[1];//.AddressList[0];
        Console.WriteLine(ipAddress);
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 8080);

        // Create a TCP/IP socket.
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp );

        // Bind the socket to the local endpoint and 
        // listen for incoming connections.
        try {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.
            while (true) {
                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.
                Socket handler = listener.Accept();
                data = null;

                // An incoming connection needs to be processed.
                while (true) {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes,0,bytesRec);
                    Console.WriteLine(data);
                    if (data.IndexOf("p") > -1) {
                        TogglePlayPause();
                        break;
                    }
                    if (data.IndexOf("u") > -1)
                    {
                        VolUp();
                        break;
                    }
                    if (data.IndexOf("d") > -1)
                    {
                        VolDown();
                        break;
                    }
                    if (data.IndexOf("r") > -1)
                    {
                        Refresh();
                        break;
                    }
                    if (data.IndexOf("o") > -1)
                    {
                        TurnOffScreen();
                        break;
                    }
                }

                // Show the data on the console.
                Console.WriteLine( "Text received : {0}", data);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            
        } catch (Exception e) {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();
        
    }

    private static void TogglePlayPause()
    {
        SendKeys.SendWait(" ");
    }

    private static void VolUp()
    {
        for (int i = 0; i < 20; i++)
        {
            keybd_event((byte)Keys.VolumeUp, 0, 0, 0);
        }
    }

    private static void VolDown()
    {
        for (int i = 0; i < 20; i++)
        {
            keybd_event((byte)Keys.VolumeDown, 0, 0, 0);
        }
    }

    private static void Refresh()
    {
        SendKeys.SendWait("{F5}");
    }

    private static void TurnOffScreen()
    {
        const int SC_MONITORPOWER = 0xF170;
        const int WM_SYSCOMMAND = 0x0112;
        const int MONITOR_OFF = 2;
        SendMessage(GetConsoleWindow(), WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)MONITOR_OFF);
    }

    public static int Main(String[] args) {
        StartListening();
        return 0;
    }
}
}