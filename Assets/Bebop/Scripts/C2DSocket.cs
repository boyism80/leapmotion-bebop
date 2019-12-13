using BebopCommandSet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace ParrotBebop2
{
    public class C2DSocket
    {
        private Socket _socket;

        private IPEndPoint _drone_endpoint;

        private int[] _seq = new int[256];

        public C2DSocket(string drone_host, int drone_port)
        {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this._drone_endpoint = new IPEndPoint(IPAddress.Parse(drone_host), drone_port);
        }

        public C2DSocket() : this(CommandSet.IP, CommandSet.C2D_PORT)
        {
        }

        private int update_sequence(int id)
        {
            this._seq[id] = (this._seq[id] + 1) % 256;
            return this._seq[id];
        }

        private byte[] encode_command(Command cmd, bool request_ack)
        {
            var id = request_ack ? CommandSet.BD_NET_CD_ACK_ID : CommandSet.BD_NET_CD_NONACK_ID;

            return this.encode_command(CommandSet.ARNETWORKAL_FRAME_TYPE_DATA,
                id,
                cmd);
        }

        private byte[] encode_command(int type, int id, Command cmd)
        {
            var buffer = new byte[cmd.size + 7];

            using (var writer = new BinaryWriter(new MemoryStream(buffer)))
            {
                writer.Write((byte)type);
                writer.Write((byte)id);
                writer.Write((byte)this.update_sequence(id));
                writer.Write(buffer.Length);
                writer.Write(cmd.cmd, 0, cmd.size);
            }

            return buffer;
        }

        public void send(Command cmd, bool request_ack = false)
        {
            try
            {
                var encoded_cmd = this.encode_command(cmd, request_ack);
                this._socket.SendTo(encoded_cmd, this._drone_endpoint);
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public void send(int type, int id, Command cmd)
        {
            try
            {
                var encoded_cmd = this.encode_command(type, id, cmd);
                this._socket.SendTo(encoded_cmd, this._drone_endpoint);
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}