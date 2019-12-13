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
	public class LeapmotionPlaybackRecorder : MonoBehaviour
	{
		public enum State
		{
			Idle, Recording, Pause,
		}

		public State RecordState
		{
			get; private set;
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

		[SerializeField] [EditTimeOnly]
		private LeapServiceProvider _leapmotionProvider;
		public LeapServiceProvider LeapmotionProvider
		{
			get
			{
				return this._leapmotionProvider;
			}
		}

		[SerializeField] [EditTimeOnly]
		private Transform _head;
		public Transform Head
		{
			get
			{
				return this._head;
			}
		}


		public void Start()
		{

		}

		public void Update()
		{
			if (this.RecordState != State.Recording)
				return;

			var jframe = this.LeapmotionProvider.GetLeapController().Frame().ToJson();
			jframe["head"] = new JSONClass();
			jframe["head"]["rotation"] = this.Head.localRotation.ToJson();
            jframe["head"]["position"] = this.Head.localPosition.ToJson();

			this.Frames.Add(jframe);
		}

		public bool StartRecord()
		{
			if (this.RecordState != State.Idle)
				return false;

			this.RecordState = State.Recording;
			this.Frames = new JSONArray();
			return true;
		}

		public bool Pause()
		{
			if (this.RecordState != State.Recording)
				return false;

			this.RecordState = State.Pause;
			return true;
		}

		public bool Resume()
		{
			if (this.RecordState != State.Pause)
				return false;

			this.RecordState = State.Recording;
			return true;
		}

		public bool StopRecord(string filename)
		{
			if (this.RecordState == State.Idle)
				return false;

			if (filename == null || filename.Length == 0)
				return false;

            this.RecordState = State.Idle;

            if(filename != null)
			    this._frames.SaveToCompressedFile(filename);

			return true;
		}
	}
}