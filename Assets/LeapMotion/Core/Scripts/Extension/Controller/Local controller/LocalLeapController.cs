//using OpenCvSharp;
using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Leap
{
    public class LocalLeapmotionController : LeapmotionController
    {
        private Controller _controller;

        private byte[] _background = new byte[640*240*2];

        private bool _useInterpolation;
        protected bool UseInterpolation
        {
            get
            {
                return _useInterpolation;
            }
            set
            {
                _useInterpolation = value;
            }
        }

        protected int ExtrapolationAmount = 0;
        protected int BounceAmount = 0;

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

        public LocalLeapmotionController()
        {
            this._controller = new Controller();
        }
        public LocalLeapmotionController(int connectionKey)
        {
            this._controller = new Controller(connectionKey);
        }

        public bool IsServiceConnected { get { return this._controller.IsServiceConnected; } }
        public bool IsConnected { get { return this._controller.IsConnected; } }
        public Config Config { get { return this._controller.Config; } }
        public DeviceList Devices { get { return this._controller.Devices; } }
        public SynchronizationContext EventContext { get { return this._controller.EventContext; } set { this._controller.EventContext = value; } }

        public void ClearPolicy(Leap.Controller.PolicyFlag policy)
        {
            this._controller.ClearPolicy(policy);
        }
        public void Dispose()
        {
            this._controller.Dispose();
        }
        public FailedDeviceList FailedDevices()
        {
            return this._controller.FailedDevices();
        }
        public Frame Frame()
        {
            return this._controller.Frame();
        }
        public Frame Frame(int history)
        {
            return this._controller.Frame(history);
        }
        public void Frame(Frame toFill, int history)
        {
            this._controller.Frame(toFill, history);
        }
        public void Frame(Frame toFill, bool fix = false)
        {
            if(this.UseInterpolation)
            {
                if(fix)
                {
                    this._controller.GetInterpolatedFrame(toFill, CalculateInterpolationTime());
                }
                else
                {
                    this._controller.GetInterpolatedFrameFromTime(toFill, CalculateInterpolationTime() + (ExtrapolationAmount * 1000), CalculateInterpolationTime() - (BounceAmount * 1000));
                }
                
            }
            else
            {
                this._controller.Frame(toFill);
            }
        }
        public long FrameTimestamp(int history = 0)
        {
            return this._controller.FrameTimestamp(history);
        }
        public void GetInterpolatedFrame(Frame toFill, long time)
        {
            this._controller.GetInterpolatedFrame(toFill, time);
        }
        public Frame GetInterpolatedFrame(long time)
        {
            return this._controller.GetInterpolatedFrame(time);
        }
        public void GetInterpolatedFrameFromTime(Frame toFill, long time, long sourceTime)
        {
            this._controller.GetInterpolatedFrameFromTime(toFill, time, sourceTime);
        }
        public void GetInterpolatedLeftRightTransform(long time, long sourceTime, int leftId, int rightId, out LeapTransform leftTransform, out LeapTransform rightTransform)
        {
            this._controller.GetInterpolatedLeftRightTransform(time, sourceTime, leftId, rightId, out leftTransform, out rightTransform);
        }
        public Frame GetTransformedFrame(LeapTransform trs, int history = 0)
        {
            return this._controller.GetTransformedFrame(trs, history);
        }
        public bool IsPolicySet(Leap.Controller.PolicyFlag policy)
        {
            return this._controller.IsPolicySet(policy);
        }
        public long Now()
        {
            return this._controller.Now();
        }
        public Image RequestImages(long frameId, Image.ImageType type)
        {
            return this._controller.RequestImages(frameId, type);
        }
        public Image RequestImages(long frameId, Image.ImageType type, byte[] imageBuffer)
        {
            return this._controller.RequestImages(frameId, type, imageBuffer);
        }
        public void SetPolicy(Leap.Controller.PolicyFlag policy)
        {
            this._controller.SetPolicy(policy);
        }
        public void StartConnection()
        {
            this._controller.StartConnection();
        }
        public void StopConnection()
        {
            this._controller.StopConnection();
        }


        long CalculateInterpolationTime(bool endOfFrame = false)
        {
            return this.Now() - 16000;
        }

        public void OnUpdate()
        {
            //this.RequestImages(this._controller.Frame().Id, Image.ImageType.DEFAULT, this._background);
        }

        public void OnFixedUpdate()
        {
            //this.RequestImages(this._controller.Frame().Id, Image.ImageType.DEFAULT, this._background);
        }

        public byte[] Background(LeapmotionBackgroundSet id)
        {
            if(this._background == null)
                return null;

            var size = 640 * 240;
            using (var reader = new BinaryReader(new MemoryStream(this._background, size * (int)id, size)))
            {
                var data = reader.ReadBytes(size);
                //var mat = new MatOfByte(240, 640, data);

                byte[] ret = null;
                //Cv2.ImEncode(".jpg", mat, out ret);

                return ret;
            }
        }
    }
}