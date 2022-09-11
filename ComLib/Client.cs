using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ComLib
{
    public abstract class ClientBehavior
    {
        private TcpClient client;
        private NetworkStream network_stream;

        protected Config config = new Config();

        protected void Connect()
        {
            client = new TcpClient();
            client.BeginConnect(config.ip,config.port,_Connect,null);
        }

        private void _Connect(IAsyncResult asyncResult)
        {
            client.ReceiveBufferSize = config.buffersize;
            client.SendBufferSize = config.buffersize;
            network_stream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            network_stream.BeginRead(buffer, 0, buffer.Length, _Receive, buffer);
        }

        private void _Receive(IAsyncResult asyncResult)
        {
            int len = network_stream.EndRead(asyncResult);
            if (len <= 0) { Disconnect(); return; }
            byte[] buffer = asyncResult.AsyncState as byte[];
            byte[] databuffer = new byte[len];
            Array.Copy(buffer, databuffer, len);
            //print
            buffer = new byte[client.ReceiveBufferSize];
            network_stream.BeginRead(buffer, 0, buffer.Length, _Receive, buffer);
        }
        protected void Send(byte[] data)
        {
            network_stream.Write(data, 0, data.Length);
            Thread.Sleep(config.delay);
            //print
        }

        protected void Disconnect()
        {
            client.Close();
            client = null;
            network_stream.Close();
            network_stream = null;
        }



        protected class Config
        {
            public string ip = "127.0.0.1";
            public int port = 1234;
            public int buffersize = 1024;
            public int delay = 1;
        }
    }
}
