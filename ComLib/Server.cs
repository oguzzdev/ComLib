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
    public abstract class ServerBehavior
    {
        private TcpListener tcp_listener;

        protected Client[] clients;

        protected Config config = new Config();
        protected static ServerBehavior server_behavior;

        protected virtual void Started() { }
        protected virtual void Stoped() { }
        protected virtual void Joined(Client client) { }
        protected virtual void Disconnected(Client client) { }
        protected virtual void Sent(byte[] data,Client client) { }
        protected virtual void Read(byte[] data,Client client) { }
        public ServerBehavior()
        {
            server_behavior = this;
        }

        protected void Start()
        {
            tcp_listener = new TcpListener(IPAddress.Parse(config.ip), config.port);
            tcp_listener.Start();
            clients = new Client[config.client_capacity];
            for (int i = 0; i < clients.Length; i++)
                clients[i] = new Client(i);
            tcp_listener.BeginAcceptTcpClient(Accept, null);
            Started();
        }

        protected void Stop()
        {
            for (int i = 0; i < clients.Length; i++)
                clients[i].Disconnect();
            clients = null;
            tcp_listener.Stop();
            tcp_listener = null;
            Stop();
        }

        private void Accept(IAsyncResult asyncResult)
        {
            TcpClient socket = tcp_listener.EndAcceptTcpClient(asyncResult);
            for (int i = 0; i < config.client_capacity; i++)
            {
                if (!clients[i].is_working)
                {
                    clients[i].Track(socket);
                    tcp_listener.BeginAcceptTcpClient(Accept, null);
                    return;
                }
            }

            socket.Close();
            tcp_listener.BeginAcceptTcpClient(Accept, null);
            return;
        }

        protected class Client
        {
            private TcpClient client;
            private NetworkStream network_stream;

            public bool is_working { get { return client != null; } }
            public int id;

            public Client(int id)
            {
                this.id = id;
            }

            public void Track(TcpClient client)
            {
                this.client = client;
                client.ReceiveBufferSize = server_behavior.config.buffersize;
                client.SendBufferSize = server_behavior.config.buffersize;
                network_stream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];
                network_stream.BeginRead(buffer, 0, buffer.Length, Receive, buffer);
                server_behavior.Joined(this);
            }

            private void Receive(IAsyncResult asyncResult)
            {
                int len = network_stream.EndRead(asyncResult);
                if (len <= 0) { Disconnect(); return; }
                byte[] buffer = asyncResult.AsyncState as byte[];
                byte[] databuffer = new byte[len];
                Array.Copy(buffer, databuffer, len);
                server_behavior.Read(databuffer, this);
                buffer = new byte[client.ReceiveBufferSize];
                network_stream.BeginRead(buffer, 0, buffer.Length, Receive, buffer);
            }

            public void Send(byte[] data)
            {
                network_stream.Write(data, 0, data.Length);
                Thread.Sleep(server_behavior.config.delay);
                server_behavior.Sent(data, this);
            }

            public void Disconnect()
            {
                client.Close();
                network_stream.Close();
                Reset();
                server_behavior.Disconnected(this);
            }

            public void Reset()
            {
                client = null;
                network_stream = null;
            }
        }


        protected class Config
        {
            public string ip = "127.0.0.1";
            public int port = 1234;
            public int client_capacity = 24;
            public int buffersize = 1024;
            public int delay=1;
        }
    }
}
