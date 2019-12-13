using Leap.Unity.Attributes;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Leap
{
    public interface IPlaybackEvent
    {
        void OnLoadPlaybackFile(string filename);
        void OnCompletePlayback(string id);
        void OnSpeechActive(bool active, string text);
    }

	public class LeapmotionPlaybackController : LeapmotionController
	{
		private LeapmotionPlaybackPlayer _playback_player;
		public LeapmotionPlaybackPlayer PlaybackPlayer
		{
			get
			{
				return this._playback_player;
			}
			private set
			{
				this._playback_player = value;
			}
		}

		private Dictionary<string, JSONArray> _playback_table;
		public Dictionary<string, JSONArray> PlaybackTable
		{
			get
			{
				return this._playback_table;
			}
			private set
			{
				this._playback_table = value;
			}
		}

        private IPlaybackEvent _playback_event;
        public IPlaybackEvent PlaybackEvent
        {
            get
            {
                return this._playback_event;
            }
            set
            {
                this._playback_event = value;
                this.PlaybackPlayer.PlaybackEvent = value;
            }
        }


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

		public event EventHandler<ConnectionEventArgs> Connect;
		public event EventHandler<ConnectionLostEventArgs> Disconnect;
		public event EventHandler<FrameEventArgs> FrameReady;
		public event EventHandler<DeviceEventArgs> Device;
		public event EventHandler<DeviceEventArgs> DeviceLost;
		public event EventHandler<ImageEventArgs> ImageReady;
		public event EventHandler<DeviceFailureEventArgs> DeviceFailure;
		public event EventHandler<LogEventArgs> LogMessage;
		public event EventHandler<PolicyEventArgs> PolicyChange;
		public event EventHandler<ConfigChangeEventArgs> ConfigChange;
		public event EventHandler<DistortionEventArgs> DistortionChange;

		public LeapmotionPlaybackController(Transform head, params string[] files)
		{
			this.PlaybackPlayer = new LeapmotionPlaybackPlayer(head);
			this.PlaybackTable = new Dictionary<string, JSONArray>();

			var loadPlaybackThread = new Thread(new ParameterizedThreadStart(this.LoadPlaybackFile));
			loadPlaybackThread.Start(files);
		}

		public byte[] Background(LeapmotionBackgroundSet id)
		{
			throw new NotImplementedException();
		}

		public void ClearPolicy(Controller.PolicyFlag policy)
		{
			
		}

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
			try
			{
				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public void Frame(Frame toFill, int history)
		{
			throw new NotImplementedException();
		}

		public void Frame(Frame toFill, bool fix = false)
		{
			try
			{
				toFill.CopyFrom(this.PlaybackPlayer.Frame);
			}
			catch(Exception)
			{}
		}

		public Frame Frame(int history = 0)
		{
			throw new NotImplementedException();
		}

		public long FrameTimestamp(int history = 0)
		{
			return 0;
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
			return 0;
		}

		public void OnFixedUpdate()
		{
			this.OnUpdate();
		}

		public void OnUpdate()
		{
			this.PlaybackPlayer.Update();
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
			//throw new NotImplementedException();
		}

		public void StartConnection()
		{
			throw new NotImplementedException();
		}

		public void StopConnection()
		{
		}

		private void LoadPlaybackFile(object filenames)
		{
			foreach (var filename in filenames as string[])
			{
				this.PlaybackTable.Add(filename, JSONArray.LoadFromCompressedFile(filename).AsArray);
                this.PlaybackEvent.OnLoadPlaybackFile(filename);
			}
		}
	}
}