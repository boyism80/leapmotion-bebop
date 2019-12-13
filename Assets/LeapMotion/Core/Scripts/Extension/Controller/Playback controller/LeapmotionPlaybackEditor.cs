using Leap.Unity;
using Leap.Unity.Attributes;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Leap
{
    public class LeapmotionPlaybackEditor
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


        public bool EnableDialog(string text)
        {
            try
            {
                this.PlaybackPlayer.Now["dialog"] = new JSONClass();
                this.PlaybackPlayer.Now["dialog"]["active"] = new JSONData(true);
                this.PlaybackPlayer.Now["dialog"]["content"] = text;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DisableDialog()
        {
            try
            {
                this.PlaybackPlayer.Now["dialog"] = new JSONClass();
                this.PlaybackPlayer.Now["dialog"]["active"] = new JSONData(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Save(string filename)
        {
            try
            {
                this.PlaybackPlayer.Frames.SaveToCompressedFile(filename);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int FindEnabledDialogSeek(bool isBackDirection = false)
        {
            try
            {
                if (isBackDirection)
                {
                    for (var i = Mathf.Max(this.PlaybackPlayer.Seek - 1, 0); i >= 0; i--)
                    {
                        if (this.PlaybackPlayer.Frames[i]["dialog"] != null)
                            return i;
                    }
                }
                else
                {
                    for (var i = Mathf.Min(this.PlaybackPlayer.Seek + 1, this.PlaybackPlayer.Frames.Count - 1); i < this.PlaybackPlayer.Frames.Count; i++)
                    {
                        if (this.PlaybackPlayer.Frames[i]["dialog"] != null)
                            return i;
                    }
                }
            }
            catch (Exception)
            {
            }

            return -1;
        }
    }
}