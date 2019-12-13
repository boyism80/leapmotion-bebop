using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Leap
{
	class LeapRemoteController : LeapmotionReceiver, LeapmotionController
	{
		private Frame _frame;
		private byte[] _background_left, _background_right;
		private long _controller_timestamp, _controller_now;
		private LeapmotionBackgroundSet _current_background_set = LeapmotionBackgroundSet.LeftBackground;

		public event EventHandler<DeviceEventArgs> Device;
		public event EventHandler<LeapEventArgs> Init;
		public event EventHandler<ConnectionEventArgs> Connect;
		public event EventHandler<ConnectionLostEventArgs> Disconnect;
		public event EventHandler<FrameEventArgs> FrameReady;
		public event EventHandler<DistortionEventArgs> DistortionChange;
		public event EventHandler<DeviceEventArgs> DeviceLost;
		public event EventHandler<ImageEventArgs> ImageReady;
		public event EventHandler<ImageRequestFailedEventArgs> ImageRequestFailed;
		public event EventHandler<DeviceFailureEventArgs> DeviceFailure;
		public event EventHandler<LogEventArgs> LogMessage;
		public event EventHandler<PolicyEventArgs> PolicyChange;
		public event EventHandler<ConfigChangeEventArgs> ConfigChange;
		public event EventHandler<InternalFrameEventArgs> InternalFrameReady;

		public bool IsServiceConnected
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsConnected { get { return true; } }

		public Config Config { get { return null; } }

		public DeviceList Devices { get { return null; } }

		public SynchronizationContext EventContext { get { return null; } set { } }

		public LeapRemoteController(string host, short port_tcp, short port_udp) : base(host, port_tcp, port_udp)
		{ }

		public void ClearPolicy(Controller.PolicyFlag policy)
		{ }

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public FailedDeviceList FailedDevices()
		{
			throw new NotImplementedException();
		}

		public Frame Frame()
		{
			throw new NotImplementedException();
		}

		public Frame Frame(int history)
		{
			throw new NotImplementedException();
		}

		public void Frame(Frame toFill, int history)
		{
			Debug.Log("??");
		}

		public void Frame(Frame toFill, bool fix = false)
		{
			if(this._frame == null)
				return;

			toFill.CopyFrom(this._frame);
		}

		public long FrameTimestamp(int history = 0)
		{
			return this._controller_timestamp;
		}

		public void GetInterpolatedFrame(Frame toFill, long time)
		{
			throw new NotImplementedException();
		}

		public Frame GetInterpolatedFrame(long time)
		{
			throw new NotImplementedException();
		}

		public void GetInterpolatedFrameFromTime(Frame toFill, long time, long sourceTime)
		{
			throw new NotImplementedException();
		}

		public void GetInterpolatedLeftRightTransform(long time, long sourceTime, int leftId, int rightId, out LeapTransform leftTransform, out LeapTransform rightTransform)
		{
			throw new NotImplementedException();
		}

		public Frame GetTransformedFrame(LeapTransform trs, int history = 0)
		{
			throw new NotImplementedException();
		}

		public bool IsPolicySet(Controller.PolicyFlag policy)
		{
			throw new NotImplementedException();
		}

		public long Now()
		{
			return this._controller_now;
		}

		public Image RequestImages(long frameId, Image.ImageType type)
		{
			throw new NotImplementedException();
		}

		public Image RequestImages(long frameId, Image.ImageType type, byte[] imageBuffer)
		{
			throw new NotImplementedException();
		}

		public void SetPolicy(Controller.PolicyFlag policy)
		{
			throw new NotImplementedException();
		}

		public void StartConnection()
		{
		}

		public void StopConnection()
		{
		}

		protected override void OnReceiveBuffer(Socket socket, byte[] bytes)
		{
		}

		protected override void OnReceiveJson(Socket socket, JSONNode json)
		{
			try
			{
				switch (json["method"])
				{
					case "get frame":
						Debug.Log("on receive frame");
						this._frame = json["frame"].ToFrame();
						json.Remove("frame");

						this._controller_timestamp = json["controller"]["frame timestamp"].AsInt;
						this._controller_now = json["controller"]["now"].AsInt;
						json.Remove("controller");
						
						//base.Send(json, socket.ProtocolType);
						break;

					case "get image":
						//if(json["left"].Value != string.Empty)
						//	this._background_left = LeapmotionExt.LoadFromCompressedBase64(json["left"].Value);

						//if(json["right"].Value != string.Empty)
						//	this._background_right = LeapmotionExt.LoadFromCompressedBase64(json["right"].Value);

						//json["left"] = new JSONData(this._current_background_set == LeapmotionBackgroundSet.LeftBackground);
						//json["right"] = new JSONData(this._current_background_set == LeapmotionBackgroundSet.RightBackground);
						//base.Send(json, socket.ProtocolType);
						break;
				}
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}

		protected override void OnConnect()
		{
			//var json = new JSONClass();
			//json["method"] = "get frame";
			//base.Send(json, ProtocolType.Udp);

			//json = new JSONClass();
			//json["method"] = "get image";
			//json["left"] = new JSONData(this._current_background_set == LeapmotionBackgroundSet.LeftBackground);
			//json["right"] = new JSONData(this._current_background_set == LeapmotionBackgroundSet.RightBackground);
			//base.Send(json, ProtocolType.Udp);
		}

		protected override void OnDisconnect()
		{
			base.OnDisconnect();
		}

		public void OnUpdate()
		{
		}

		public void OnFixedUpdate()
		{
		}

		public byte[] Background(LeapmotionBackgroundSet id)
		{
			this._current_background_set = id;
			switch(id)
			{
				case LeapmotionBackgroundSet.LeftBackground:
					return this._background_left;

				case LeapmotionBackgroundSet.RightBackground:
					return this._background_right;

				default:
					return null;
			}
		}
	}
}