using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SendSocket
{
    class Program
    {
        static int starTime = 0;
        static int endTime = 0;

        static int count = 0;
        static bool falge = false;

        static int SocketCount = 4000;

        static List<Socket> _clients = new List<Socket>();

        static void Main(string[] args)
        {
           
             //OneSocket();
            ManySocket();
            Console.WriteLine("连接完成");
            Console.Read();
            
        }

        private static void ManySocket()
        {
            for (int i = 0; i < SocketCount;i++ )
            {
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("127.0.0.1", 1234);
                Console.WriteLine("连接成功 {0}", i);
                _clients.Add(client);
            }

        }


        private static void OneSocket()
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            string testIP1 = "127.0.0.1";
            string testIP2 = "172.16.128.148";

            string desIp = testIP2;
            client.Connect(desIp, 1234);

            if (client.Connected)
            {
                Console.WriteLine("connect susscue");
            }

            //byte[] some = Encoding.ASCII.GetBytes("this my hellothis my hellothis my hellothis my hellothis my hellothis my hello /1this my hellothis my hellothis my hellothis my hellothis my hello /1this my hellothis my hellothis my hellothis my hello/");

            int size = 1024;

            byte[] some = new byte[size];

            for (int i = 0; i < size; i++)
            {
                some[i] = (byte)'a';

            }
           

            Console.WriteLine("buff size {0}  begin send", some.Length);


            while(true)
            {
                try
                {
                    client.Send(some);
                    //client.BeginSend(some, 0, some.Length, 0, new AsyncCallback(SendCallback), client);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                count++;
                if (count == 1)
                {
                    starTime = Environment.TickCount;
                }
                if (count >= 10000 && falge == false)
                {
                    falge = true;
                    endTime = Environment.TickCount;
                    Console.WriteLine(endTime - starTime);
                }
                // Retrieve the socket from the state object.     
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.     
                int bytesSent = handler.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Read();
            }
        }
    }
}
