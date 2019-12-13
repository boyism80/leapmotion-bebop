using Leap;
using Leap.Unity;
using Leap.Unity.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Guide : MonoBehaviour, IPlaybackEvent
{
    [Header("[Leapmotion Controller]")]
    [SerializeField] [EditTimeOnly]
    private LeapServiceProvider _provider;

    public LeapServiceProvider LeapServiceProvider
    {
        get
        {
            return this._provider;
        }
        private set
        {
            this._provider = value;
        }
    }

    private LeapmotionPlaybackController _controller;
    public LeapmotionPlaybackController Controller
    {
        get
        {
            return this._controller;
        }
        private set
        {
            this._controller = value;
        }
    }

    public UnityEvent OnCompleteLastPlayback;

    [Header("[Speech UI]")]
    [SerializeField] [EditTimeOnly]
    public GameObject SpeechBox;
    public Text SpeechText;


    private Queue<string> _next_playback_queue;
    public Queue<string> NextPlaybackQueue
    {
        get
        {
            return this._next_playback_queue;
        }
        private set
        {
            this._next_playback_queue = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        this.NextPlaybackQueue = new Queue<string>();
        this.Controller = this.LeapServiceProvider.GetLeapController() as LeapmotionPlaybackController;
        this.Controller.PlaybackEvent = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnCompletePlayback(string id)
    {
        try
        {
            Debug.Log("complete : " + id);
            if(id.Equals("use ui.playback"))
            {
                this.gameObject.SetActive(false);
                this.OnCompleteLastPlayback.Invoke();
                return;
            }

            var next = this.NextPlaybackQueue.Dequeue();
            this.Controller.PlaybackPlayer.SetPlayback(next, this.Controller.PlaybackTable[next]);
            Debug.Log(id);
        }
        catch (Exception)
        {
            this.Controller.PlaybackPlayer.Reset();
        }
    }

    public void OnLoadPlaybackFile(string filename)
    {
        this.NextPlaybackQueue.Enqueue(filename);
        if (this.Controller.PlaybackPlayer.Frames == null)
        {
            this.Controller.PlaybackPlayer.SetPlayback(filename, this.Controller.PlaybackTable[filename]);
            this.Controller.PlaybackPlayer.Running = true;
        }

        Debug.Log("load complete : " + filename);
    }

    public void OnSpeechActive(bool active, string text)
    {
        this.SpeechBox.SetActive(active);
        this.SpeechText.text = text;
    }
}