using SimpleJSON;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Leap
{
	public class LeapmotionReceiver
	{
		private Socket _socket_tcp, _socket_udp;
		private EndPoint _endpoint_tcp, _endpoint_udp;
		private bool _try_connect = false;
		private Mutex _mutex = new Mutex();

		public LeapmotionReceiver(string host, short port_tcp, short port_udp)
		{
            Debug.Log("hello -2");
			this._endpoint_tcp = (EndPoint)(new IPEndPoint(IPAddress.Parse(host), port_tcp));
            Debug.Log("hello -1");
			this.Connect();


            Debug.Log("hello 00");
			this._socket_udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Debug.Log("hello 01");
			this._endpoint_udp = (EndPoint)(new IPEndPoint(IPAddress.Parse(host), port_udp));
            Debug.Log("hello 02");
			this._socket_udp.Connect(this._endpoint_udp);
		}

		private void Connect()
		{
			if(this._try_connect == true)
				return;

            Debug.Log("hello 03");
			this._mutex.WaitOne();
            Debug.Log("hello 04");
			if(this._socket_tcp != null && this._socket_tcp.Connected)
				this._socket_tcp.Close();

			this._socket_tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this._socket_tcp.BeginConnect(_endpoint_tcp, new AsyncCallback(this.OnConnectTCP), this._socket_tcp);
			this._try_connect = true;
			this._mutex.ReleaseMutex();
		}

		public bool Send(JSONNode json, ProtocolType protocolType = ProtocolType.Tcp)
		{
			try
			{
				var data = LeapmotionProtocol.EncodeLeapmotioonFormat(json);
				switch (protocolType)
				{
					case ProtocolType.Tcp:
						data = this._socket_tcp.EncodeLeapmotioonFormat(json);
						this._socket_tcp.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(this.OnSendTCP), this._socket_tcp);
						break;

					case ProtocolType.Udp:
						this._socket_udp.BeginSendTo(data, 0, data.Length, SocketFlags.None, this._endpoint_udp, new AsyncCallback(this.OnSendUDP), this._endpoint_udp);
						break;

					default:
						return false;
				}

				return true;
			}
			catch(SocketException e)
			{
				switch(e.ErrorCode)
				{
					case 10054:
						this.OnDisconnect();
						break;
				}

				return false;
			}
		}

		private void OnConnectTCP(IAsyncResult ar)
		{
			this._mutex.WaitOne();
			if (this._socket_tcp.Connected)
			{
				this._socket_tcp.EndConnect(ar);
				var parameter = new LeapmotionProtocol.BufferFormat(this._socket_tcp);
				this._socket_tcp.BeginReceive(parameter.Buffer, 0, parameter.Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceiveTCP), parameter);
				this._try_connect = false;

				this.OnConnect();

			}
			else
			{
				this._try_connect = false;
				this.Connect();
			}
			this._mutex.ReleaseMutex();
		}

		private void OnSendTCP(IAsyncResult ar)
		{
			var sendsize = this._socket_tcp.EndSend(ar);
		}

		private void OnReceiveTCP(IAsyncResult ar)
		{
			try
			{
				var parameter = ar.AsyncState as LeapmotionProtocol.BufferFormat;
				var recvsize = this._socket_tcp.EndReceive(ar);
				if (recvsize == 0)
					return;

				var format = new LeapmotionProtocol.StreamFormat();
				var data = this._socket_tcp.DecodeLeapmotionFormat(recvsize, parameter, out format);
				switch (format)
				{
					case LeapmotionProtocol.StreamFormat.Json:
						this.OnReceiveJson(this._socket_tcp, data as JSONNode);
						break;

					case LeapmotionProtocol.StreamFormat.Buffer:
						this.OnReceiveBuffer(this._socket_tcp, data as byte[]);
						break;
				}

				this._socket_tcp.BeginReceive(parameter.Buffer, 0, parameter.Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceiveTCP), parameter);
			}
			catch(SocketException e)
			{
				switch(e.ErrorCode)
				{
					case 10054:
						this.OnDisconnect();
						break;
				}
			}
		}

		private void OnSendUDP(IAsyncResult ar)
		{
			try
			{
				this._socket_udp.EndSendTo(ar);

				var buffer = new byte[65536];
				this._socket_udp.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref this._endpoint_udp, new AsyncCallback(this.OnReceiveUDP), buffer);
			}
			catch(SocketException e)
			{
				switch(e.ErrorCode)
				{
					case 10054:
						this.OnDisconnect();
						break;
				}
			}
		}

		private void OnReceiveUDP(IAsyncResult ar)
		{
			try
			{
				var buffer = ar.AsyncState as byte[];
				var sender = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
				var recvsize = this._socket_udp.EndReceiveFrom(ar, ref sender);

				var format = new LeapmotionProtocol.StreamFormat();
				var data = this._socket_udp.DecodeLeapmotionFormat(recvsize, buffer, out format);
				if (data == null)
					return;

				switch (format)
				{
					case LeapmotionProtocol.StreamFormat.Json:
						this.OnReceiveJson(this._socket_udp, data as JSONNode);
						break;

					case LeapmotionProtocol.StreamFormat.Buffer:
						this.OnReceiveBuffer(this._socket_udp, data as byte[]);
						break;
				}
			}
			catch(Exception) {

			}
		}

		protected virtual void OnReceiveJson(Socket socket, JSONNode json)
		{
			switch (json["method"])
			{
				case "get image":
					//if(json["left"].Value != string.Empty)
					//{
					//	var bytes = LeapmotionExt.LoadFromCompressedBase64(json["left"].Value);
					//	var image = Cv2.ImDecode(bytes, ImreadModes.GrayScale);
					//	Cv2.ImShow("left " + socket.ProtocolType.ToString(), image);
					//}

					//if(json["right"].Value != string.Empty)
					//{
					//	var bytes = LeapmotionExt.LoadFromCompressedBase64(json["right"].Value);
					//	var image = Cv2.ImDecode(bytes, ImreadModes.GrayScale);
					//	Cv2.ImShow("right", image);
					//}
					//Cv2.WaitKey(10);

					//json = new JSONClass();
					//json["method"] = "get image";
					//json["left"] = new JSONData(true);
					//json["right"] = new JSONData(false);
					//this.Send(json, socket.ProtocolType);
					break;

				case "get hands":
					Console.WriteLine(json.ToString());
					break;

				case "get frame":
					Console.WriteLine(json.ToString());
					var frame = json["frame"].ToFrame();
					break;

				case "get controller":
					Console.WriteLine(json.ToString());
					break;
			}
		}

		protected virtual void OnReceiveBuffer(Socket socket, byte[] bytes)
		{
			Console.WriteLine("Receive buffer size : " + bytes.Length);
		}

		protected virtual void OnConnect()
		{
			var json = new JSONClass();
			json["method"] = "get image";
			json["left"] = new JSONData(true);
			json["right"] = new JSONData(false);
			this.Send(json, ProtocolType.Udp);
			this.Send(json, ProtocolType.Tcp);
		}

		protected virtual void OnDisconnect()
		{
			this.Connect();
		}
	}
}