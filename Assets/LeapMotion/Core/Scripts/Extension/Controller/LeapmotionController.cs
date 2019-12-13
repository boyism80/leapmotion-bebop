using System.Threading;

namespace Leap
{
    public interface LeapmotionController : IController
    {
        bool IsServiceConnected { get; }
        SynchronizationContext EventContext { get; set; }


        FailedDeviceList FailedDevices();
        Frame Frame();
        void Frame(Frame toFill, int history);
        void Frame(Frame toFill, bool fix = false);
        long FrameTimestamp(int history = 0);
        void GetInterpolatedFrame(Frame toFill, long time);
        void GetInterpolatedFrameFromTime(Frame toFill, long time, long sourceTime);
        void GetInterpolatedLeftRightTransform(long time, long sourceTime, int leftId, int rightId, out LeapTransform leftTransform, out LeapTransform rightTransform);
        Image RequestImages(long frameId, Image.ImageType type);
        Image RequestImages(long frameId, Image.ImageType type, byte[] imageBuffer);
        void StartConnection();
        void StopConnection();

        byte[] Background(LeapmotionBackgroundSet id);

        void OnUpdate();
        void OnFixedUpdate();
    }
}
