using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Leap
{
    public static class LeapmotionProtocol
    {
        public enum StreamFormat { Json = 0, Buffer = 1, Unknown = -1}

        public class BufferFormat
        {
            private Socket _socket;
            public Socket Socket { get { return this._socket; } private set { this._socket = value; } }


            private byte[] _buffer;
            public byte[] Buffer { get { return this._buffer; } private set { this._buffer = value; } }

            public BufferFormat(Socket socket) : this(socket, 4096)
            {
            }

            public BufferFormat(Socket socket, int bufsize)
            {
                this.Socket = socket;
                this.Buffer = new byte[bufsize];
            }

            public bool Resize(int size)
            {
                if(size < this.Buffer.Length)
                    return false;

                var dest = new byte[size];
                System.Buffer.BlockCopy(this._buffer, 0, dest, 0, this._buffer.Length);
                this._buffer = dest;
                return true;
            }
        }

        public static byte[] EncodeLeapmotioonFormat(this Socket socket, JSONNode json)
        {
            return LeapmotionProtocol.EncodeLeapmotioonFormat(json);
        }

        public static byte[] EncodeLeapmotioonFormat(JSONNode json)
        {
            var compressedBase64 = json.SaveToCompressedBase64();
            var compressedBytes = System.Text.Encoding.UTF8.GetBytes(compressedBase64);
            var buffer = new byte[sizeof(int) * 2 + compressedBytes.Length];
            using (var writer = new BinaryWriter(new MemoryStream(buffer)))
            {
                writer.Write((int)StreamFormat.Json);
                writer.Write(compressedBytes.Length);
                writer.Write(compressedBytes);
            }

            return buffer;
        }

        public static object DecodeLeapmotionFormat(this Socket socket, int recvsize, object param, out StreamFormat format)
        {
            var bytes = socket.ProtocolType == ProtocolType.Tcp ? (param as BufferFormat).Buffer : (param as byte[]);
            var bytes_size = 0;
            var header_size = sizeof(int) * 2;

            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                format = (LeapmotionProtocol.StreamFormat)reader.ReadInt32();
                bytes_size = reader.ReadInt32();
            }

            System.Buffer.BlockCopy(bytes, header_size, bytes, 0, bytes.Length - header_size);
            recvsize -= header_size;

            if(bytes_size != bytes.Length)
            {
                var new_bytes = new byte[bytes_size];
                System.Buffer.BlockCopy(bytes, 0, new_bytes, 0, recvsize);
                bytes = new_bytes;
            }


            if (socket.ProtocolType == ProtocolType.Tcp)
            {
                while (bytes_size > recvsize)
                {
                    var ret = socket.Receive(bytes, recvsize, bytes_size - recvsize, SocketFlags.None);
                    if (ret == 0)
                        return null;

                    recvsize += ret;
                }
            }

            switch(format)
            {
                case StreamFormat.Buffer:
                    return bytes;

                case StreamFormat.Json:
                    var compressedBase64 = System.Text.Encoding.UTF8.GetString(bytes);
                    return JSONData.LoadFromCompressedBase64(compressedBase64);

                default:
                    return null;
            }
        }
    }
}
