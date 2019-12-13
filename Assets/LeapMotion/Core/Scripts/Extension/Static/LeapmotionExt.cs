using SimpleJSON;
using System;
using System.Collections.Generic;
using Leap;
using System.IO;

namespace Leap
{
    public enum LeapmotionBackgroundSet
    {
        LeftBackground = 0, RightBackground = 1,
    }

    public static class LeapmotionExt
    {
        public static JSONClass ToJson(this UnityEngine.Quaternion rotation)
        {
            var ret = new JSONClass();
            ret["x"] = new JSONData(rotation.x);
            ret["y"] = new JSONData(rotation.y);
            ret["z"] = new JSONData(rotation.z);
            ret["w"] = new JSONData(rotation.w);

            return ret;
        }

        public static JSONClass ToJson(this UnityEngine.Vector3 vector)
        {
            var ret = new JSONClass();
            ret["x"] = new JSONData(vector.x);
            ret["y"] = new JSONData(vector.y);
            ret["z"] = new JSONData(vector.z);

            return ret;
        }

        public static UnityEngine.Quaternion Quaternion(this JSONNode json)
        {
            return new UnityEngine.Quaternion(json["x"].AsFloat, json["y"].AsFloat, json["z"].AsFloat, json["w"].AsFloat);
        }

        public static UnityEngine.Vector3 Vector3(this JSONNode json)
        {
            return new UnityEngine.Vector3(json["x"].AsFloat, json["y"].AsFloat, json["z"].AsFloat);
        }
        public static Leap.Vector Vector(this JSONNode json)
        {
            return new Leap.Vector(json["x"].AsFloat, json["y"].AsFloat, json["z"].AsFloat);
        }

        public static Leap.LeapQuaternion LeapQuaternion(this JSONNode json)
        {
            return new Leap.LeapQuaternion(json["x"].AsFloat, json["y"].AsFloat, json["z"].AsFloat, json["w"].AsFloat);
        }

        public static JSONClass ToJson(this Leap.LeapQuaternion rotation)
        {
            var ret = new JSONClass();
            ret["x"] = new JSONData(rotation.x);
            ret["y"] = new JSONData(rotation.y);
            ret["z"] = new JSONData(rotation.z);
            ret["w"] = new JSONData(rotation.w);

            return ret;
        }

        public static JSONClass ToJson(this Leap.Vector vector)
        {
            var ret = new JSONClass();
            ret["x"] = new JSONData(vector.x);
            ret["y"] = new JSONData(vector.y);
            ret["z"] = new JSONData(vector.z);

            return ret;
        }

        public static JSONNode ToJson(this Controller controller)
        {
            var ret = new JSONClass();
            ret["frame timestamp"] = new JSONData((int)controller.FrameTimestamp());
            ret["now"] = new JSONData((int)controller.Now());

            return ret;
        }

        public static JSONNode ToJson(this Frame frame)
        {
            var ret = new JSONClass();
            ret["id"] = new JSONData(frame.Id);
            ret["timestamp"] = new JSONData(frame.Timestamp);
            ret["current frames per second"] = new JSONData(frame.CurrentFramesPerSecond);
            ret["hands"] = frame.Hands.ToJson();

            return ret;
        }

        public static JSONArray ToJson(this List<Hand> hands)
        {
            var ret = new JSONArray();
            foreach (var hand in hands)
            {
                var json_hand = new JSONClass();
                json_hand["frame id"] = new JSONData(hand.FrameId);
                json_hand["id"] = new JSONData(hand.Id);
                json_hand["confidence"] = new JSONData(hand.Confidence);
                json_hand["grab strength"] = new JSONData(hand.GrabStrength);
                json_hand["grab angle"] = new JSONData(hand.GrabAngle);
                json_hand["pinch strength"] = new JSONData(hand.PinchStrength);
                json_hand["pinch distance"] = new JSONData(hand.PinchDistance);
                json_hand["palm width"] = new JSONData(hand.PalmWidth);
                json_hand["is left"] = new JSONData(hand.IsLeft);
                json_hand["time visible"] = new JSONData(hand.TimeVisible);

                var fingers = new JSONArray();
                foreach (var finger in hand.Fingers)
                {
                    var json_finger = new JSONClass();
                    json_finger["hand id"] = new JSONData(finger.HandId);
                    json_finger["finger id"] = new JSONData(finger.Id);
                    json_finger["time visible"] = new JSONData(finger.TimeVisible);
                    json_finger["tip position"] = finger.TipPosition.ToJson();
                    json_finger["tip velocity"] = finger.TipVelocity.ToJson();
                    json_finger["direction"] = finger.Direction.ToJson();
                    json_finger["stabilized tip position"] = finger.StabilizedTipPosition.ToJson();
                    json_finger["width"] = new JSONData(finger.Width);
                    json_finger["length"] = new JSONData(finger.Length);
                    json_finger["is extended"] = new JSONData(finger.IsExtended);
                    json_finger["type"] = new JSONData(finger.Type.ToString());

                    var bones = new JSONArray();
                    foreach (var bone in finger.bones)
                    {
                        var json_bone = new JSONClass();
                        json_bone["prev joint"] = bone.PrevJoint.ToJson();
                        json_bone["next joint"] = bone.NextJoint.ToJson();
                        json_bone["center"] = bone.Center.ToJson();
                        json_bone["direction"] = bone.Direction.ToJson();
                        json_bone["length"] = new JSONData(bone.Length);
                        json_bone["width"] = new JSONData(bone.Width);
                        json_bone["type"] = new JSONData(bone.Type.ToString());
                        json_bone["rotation"] = bone.Rotation.ToJson();

                        bones.Add(json_bone);
                    }
                    json_finger["bones"] = bones;
                    fingers.Add(json_finger);
                }
                json_hand["fingers"] = fingers;

                if (hand.Arm != null)
                {
                    var json_arm = new JSONClass();
                    json_arm["elbow"] = hand.Arm.ElbowPosition.ToJson();
                    json_arm["wrist"] = hand.Arm.WristPosition.ToJson();
                    json_arm["center"] = hand.Arm.Center.ToJson();
                    json_arm["direction"] = hand.Arm.Direction.ToJson();
                    json_arm["length"] = new JSONData(hand.Arm.Length);
                    json_arm["width"] = new JSONData(hand.Arm.Width);
                    json_arm["rotation"] = hand.Arm.Rotation.ToJson();

                    json_hand["arm"] = json_arm;
                }

                json_hand["palm position"] = hand.PalmPosition.ToJson();
                json_hand["stabilized palm position"] = hand.StabilizedPalmPosition.ToJson();
                json_hand["palm velocity"] = hand.PalmVelocity.ToJson();
                json_hand["palm normal"] = hand.PalmNormal.ToJson();
                json_hand["palm orientation"] = hand.Rotation.ToJson();
                json_hand["direction"] = hand.Direction.ToJson();
                json_hand["wrist position"] = hand.WristPosition.ToJson();

                ret.Add(json_hand);
            }

            return ret;
        }

        public static Frame ToFrame(this JSONNode json)
        {
            var frame = new Frame(json["id"].AsInt,
                json["time stamp"].AsInt,
                json["current frames per second"].AsFloat,
                new InteractionBox(),
                json["hands"].AsArray.ToHands());

            return frame;
        }

        public static List<Hand> ToHands(this JSONArray json)
        {
            var ret = new List<Hand>();
            foreach (JSONNode hand in json)
            {
                var fingers = new List<Finger>();
                foreach (JSONNode finger in hand["fingers"].AsArray)
                {
                    var bones = new List<Bone>();
                    foreach (JSONNode bone in finger["bones"].AsArray)
                    {
                        var createdBone = new Bone(bone["prev joint"].Vector(),
                                                   bone["next joint"].Vector(),
                                                   bone["center"].Vector(),
                                                   bone["direction"].Vector(),
                                                   bone["length"].AsFloat,
                                                   bone["width"].AsFloat,
                                                   (Bone.BoneType)Enum.Parse(typeof(Bone.BoneType), bone["type"].Value),
                                                   bone["rotation"].LeapQuaternion());

                        bones.Add(createdBone);
                    }

                    var createdFinger = new Finger(hand["frame id"].AsInt,
                                                   finger["hand id"].AsInt,
                                                   finger["finger id"].AsInt,
                                                   finger["time visible"].AsFloat,
                                                   finger["tip position"].Vector(),
                                                   finger["tip velocity"].Vector(),
                                                   finger["direction"].Vector(),
                                                   finger["stabilized tip position"].Vector(),
                                                   finger["width"].AsFloat,
                                                   finger["length"].AsFloat,
                                                   finger["is extended"].AsBool,
                                                   (Finger.FingerType)Enum.Parse(typeof(Finger.FingerType), finger["type"].Value),
                                                   bones[0], bones[1], bones[2], bones[3]);

                    fingers.Add(createdFinger);
                }

                var createdArm = new Arm(hand["arm"]["elbow"].Vector(),
                                         hand["arm"]["wrist"].Vector(),
                                         hand["arm"]["center"].Vector(),
                                         hand["arm"]["direction"].Vector(),
                                         hand["arm"]["length"].AsFloat,
                                         hand["arm"]["width"].AsFloat,
                                         hand["arm"]["rotation"].LeapQuaternion());

                var createdHand = new Hand(hand["frame id"].AsInt,
                                           hand["id"].AsInt,
                                           hand["confidence"].AsFloat,
                                           hand["grab strength"].AsFloat,
                                           hand["grab angle"].AsFloat,
                                           hand["pinch strength"].AsFloat,
                                           hand["pinch distance"].AsFloat,
                                           hand["palm width"].AsFloat,
                                           hand["is left"].AsBool,
                                           hand["time visible"].AsFloat,
                                           createdArm,
                                           fingers,
                                           hand["palm position"].Vector(),
                                           hand["stabilized palm position"].Vector(),
                                           hand["palm velocity"].Vector(),
                                           hand["palm normal"].Vector(),
                                           hand["palm orientation"].LeapQuaternion(),
                                           hand["direction"].Vector(),
                                           hand["wrist position"].Vector());

                ret.Add(createdHand);
            }

            return ret;
        }

        private static void SaveToStream(System.IO.Stream stream, byte[] data, int index, int count)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(data, index, count == -1 ? data.Length : count);
                writer.Flush();
            }
        }

        private static void SaveToCompressedStream(System.IO.Stream stream, byte[] data, int index, int count)
        {
            using (var zstream = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(stream))
            {
                zstream.IsStreamOwner = false;
                SaveToStream(zstream, data, index, count);
                zstream.Close();
            }
        }

        public static string SaveToCompressedBase64(byte[] data, int index = 0, int count = -1)
        {
            using (var stream = new MemoryStream())
            {
                SaveToCompressedStream(stream, data, index, count);
                stream.Position = 0;
                return System.Convert.ToBase64String(stream.ToArray());
            }
        }

        public static string SaveToCompressedBase64(this Leap.Image source)
        {
            return SaveToCompressedBase64(source.Data);
        }

        private static byte[] LoadFromStream(System.IO.Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                using (var ms = new MemoryStream())
                {
                    byte[] buffer = new byte[4096];
                    int count = 0;
                    while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        ms.Write(buffer, 0, count);
                    }

                    return ms.ToArray();
                }
            }
        }

        private static byte[] LoadFromCompressedStream(System.IO.Stream stream)
        {
            using (var zstream = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(stream))
            {
                return LoadFromStream(zstream);
            }
        }

        public static byte[] LoadFromCompressedBase64(string source)
        {
            var bytes = System.Convert.FromBase64String(source);

            using (var stream = new MemoryStream(bytes))
            {
                stream.Position = 0;
                return LoadFromCompressedStream(stream);
            }
        }
    }
}
