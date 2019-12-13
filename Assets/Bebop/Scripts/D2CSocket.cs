using BebopCommandSet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ParrotBebop2
{
    public class D2CSocket
    {
        public interface OnReceiveListener
        {
            void OnReceiveCommand(int type, int id, int seq, Command cmd);
        }

        public static readonly int BUFFER_SIZE = 80960;

        private Socket _socket;

        private OnReceiveListener _listener;

        private bool _running;
        public bool Running
        {
            get
            {
                return this._running;
            }
            set
            {
                this._running = value;
                if(_running == false)
                    return;

                var receive_cmd_thread = new Thread(this.Run);
                receive_cmd_thread.Start();
            }
        }

        public D2CSocket(int port, OnReceiveListener listener)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, port);
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this._socket.Bind(endpoint);

            this._listener = listener;
        }

        public D2CSocket(OnReceiveListener listener) : this(CommandSet.D2C_PORT, listener)
        {
        }

        ~D2CSocket()
        {
            this._socket.Close();
        }

        private void Run()
        {
            var buffer = new byte[BUFFER_SIZE];
            while(this.Running)
            {
                var readsize = this._socket.Receive(buffer, 0, BUFFER_SIZE, SocketFlags.None);
                using (var reader = new BinaryReader(new MemoryStream(buffer, 0, readsize)))
                {
                    var type = reader.ReadByte();
                    var id = reader.ReadByte();
                    var seq = reader.ReadByte();
                    var size = reader.ReadInt32();

                    var cmd = new Command(reader.ReadBytes(size), 0, size - 7);
                    this._listener.OnReceiveCommand(type, id, seq, cmd);
                }
            }

            Debug.Log("done");
        }
    }
}