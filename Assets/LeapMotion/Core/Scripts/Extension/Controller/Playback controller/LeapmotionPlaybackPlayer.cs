using Leap.Unity.Attributes;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Leap
{
    public class LeapmotionPlaybackPlayer
    {
        private IPlaybackEvent _listener;
        public IPlaybackEvent PlaybackEvent
        {
            get
            {
                return this._listener;
            }
            set
            {
                this._listener = value;
            }
        }

        private string _id;
        public string Id
        {
            get
            {
                return this._id;
            }
            private set
            {
                this._id = value;
            }
        }

        private JSONArray _frames;
        public JSONArray Frames
        {
            get
            {
                return this._frames;
            }
            private set
            {
                this._frames = value;
            }
        }

        private int _seek;
        public int Seek
        {
            get
            {
                return this._seek;
            }
            set
            {
                this._seek = Mathf.Clamp(value, 0, this.Frames.Count);
            }
        }

        private bool _running = false;
        public bool Running
        {
            get
            {
                return this._running;
            }
            set
            {
                this._running = value;
            }
        }

        private float _amendment_speed = 1.5f;
        public float AmendentSpeed
        {
            get
            {
                return this._amendment_speed;
            }
            set
            {
                this._amendment_speed = value;
            }
        }

        private Transform _head;
        public Transform Head
        {
            get
            {
                return this._head;
            }
            private set
            {
                this._head = value;
            }
        }

        public JSONNode Now
        {
            get
            {
                try
                {
                    if (this.IsComplete)
                        return null;

                    return this.Frames[this.Seek];
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public Frame Frame
        {
            get
            {
                return this.Now.ToFrame();
            }
        }

        public bool IsComplete
        {
            get
            {
                return this.Frames.Count <= this.Seek;
            }
        }

        public bool Empty
        {
            get
            {
                if (this.Frames == null)
                    return true;

                return false;
            }
        }

        public LeapmotionPlaybackPlayer(Transform head)
        {
            this.Head = head;
        }

        public bool SetPlayback(string id, JSONArray frames)
        {
            if (id == null || id.Length == 0)
                return false;

            if (frames == null)
                return false;

            this.Id = id;
            this.Frames = frames;
            this.Reset();
            return true;
        }

        public void Update()
        {
            if (this.Frames == null || this.Frames.Count == 0)
                return;

            if (this.Running == false)
                return;

            if (this.IsComplete)
            {
                this._listener.OnCompletePlayback(this.Id);
                return;
            }

            var begin_rotation = this.Frames[0]["head"]["rotation"].Quaternion();
            if (this.Seek == 0 && Quaternion.Angle(this.Head.localRotation, begin_rotation) > 1.0f)
            {
                this.Head.localRotation = Quaternion.Lerp(this.Head.localRotation, begin_rotation, Time.deltaTime * this.AmendentSpeed);
            }
            else
            {
                this.Head.localRotation = this.Now["head"]["rotation"].Quaternion();
                //this.Head.localPosition = this.Now["head"]["position"].Vector3();
                if(this.Now["speech"] != null)
                {
                    var active = this.Now["speech"]["active"].AsBool;
                    this._listener.OnSpeechActive(active, active ? this.Now["speech"]["text"].Value : string.Empty);
                }
                this.Seek++;
            }
        }

        public void Reset()
        {
            this.Seek = 0;
        }

        public void Pause()
        {
            this.Running = false;
        }

        public void Resume()
        {
            this.Running = true;
        }

        public void Stop()
        {
            this.Reset();
            this.Running = false;
        }
    }
}