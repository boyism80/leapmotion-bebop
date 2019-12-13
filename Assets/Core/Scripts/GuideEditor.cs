using Leap;
using Leap.Unity;
using Leap.Unity.Attributes;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuideEditor : MonoBehaviour
{
    [Header("[Controller Device]")]
    [SerializeField] [EditTimeOnly]
    public LeapmotionPlaybackRecorder _recorder;
    public LeapServiceProvider _provider;
    private LeapmotionPlaybackPlayer _playback_player;

    [Header("[Save Playback File Dialog]")]
    [SerializeField] [EditTimeOnly]
    public GameObject SavePlaybackFileDialog;
    public InputField PlaybackFileNameTextBox;

    [Header("[Add Speech Dialog]")]
    public GameObject AddSpeechTextDialog;
    public InputField SpeechTextBox;
    public InputField SpeechFrameNumberTextBox;
    public Toggle CloseSpeechCheckbox;

    [Header("[Buttons]")]
    public Button RecordButton;
    private Text _RecordButtonText;

    public Button PauseButton;
    private Text _PauseButtonText;

    public Button AddSpeechButton;

    public Button PrevFrameButton;
    public Button NextFrameButton;

    // Use this for initialization
    public void Start()
    {
        this._RecordButtonText = this.RecordButton.GetComponentInChildren<Text>();
        this._PauseButtonText = this.PauseButton.GetComponentInChildren<Text>();

        this._playback_player = (this._provider.GetLeapController() as LeapmotionPlaybackController).PlaybackPlayer;
    }

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (this._recorder.RecordState == LeapmotionPlaybackRecorder.State.Idle)
                this._recorder.StartRecord();
            else
                this._recorder.StopRecord("last.playback");

            Debug.Log(this._recorder.RecordState);
        }

        this.UpdateRecordState();
    }

    private void UpdateRecordState()
    {
        if(this._recorder.RecordState == LeapmotionPlaybackRecorder.State.Recording)
            this._RecordButtonText.text = "Stop Record";
        else
            this._RecordButtonText.text = "Start Record";

        if(this._playback_player.Running)
            this._PauseButtonText.text = "Pause";
        else
            this._PauseButtonText.text = "Resume";

        this.AddSpeechButton.enabled = !(this._playback_player.Running);
        this.PrevFrameButton.enabled = this.NextFrameButton.enabled = !(this._playback_player.Running);
    }

    public void StartRecord()
    {
        if(this._recorder.RecordState == LeapmotionPlaybackRecorder.State.Recording)
            return;

        this._recorder.StartRecord();
    }

    public void StopRecord()
    {
        if(this._recorder.RecordState != LeapmotionPlaybackRecorder.State.Recording)
            return;

        this._recorder.Pause();

        this.SavePlaybackFileDialog.SetActive(true);
    }

    public void SwitchRecord()
    {
        if(this._recorder.RecordState == LeapmotionPlaybackRecorder.State.Recording)
            this.StopRecord();
        else
            this.StartRecord();
    }

    public void SwitchPause()
    {
        this._playback_player.Running = !this._playback_player.Running;
    }

    public void SavePlaybackFile()
    {
        var filename = this.PlaybackFileNameTextBox.text;
        if(filename.Length == 0)
            return;

        this._recorder.StopRecord(filename);
        this.SavePlaybackFileDialog.SetActive(false);
    }

    public void ClosePlaybackDialog()
    {
        this._recorder.StopRecord(null);
        this.SavePlaybackFileDialog.SetActive(false);
    }

    public void OpenSpeechDialog()
    {
        this.AddSpeechTextDialog.SetActive(true);
        this.SpeechFrameNumberTextBox.text = this._playback_player.Seek.ToString();
    }

    public void AddSpeech()
    {
        var frame = this._playback_player.Now;
        if(frame == null)
            return;

        var speech_json = new JSONClass();
        if(this.CloseSpeechCheckbox.isOn)
        {
            speech_json["active"] = new JSONData(true);
            speech_json["text"] = new JSONData(this.SpeechTextBox.text);
        }
        else
        {
            speech_json["active"] = new JSONData(false);
        }
        frame["speech"] = speech_json;
        this.AddSpeechTextDialog.SetActive(false);
    }

    public void OnPrevFrame()
    {
        this._playback_player.Seek--;
    }

    public void OnNextFrame()
    {
        this._playback_player.Seek++;
    }

    public void OnApply()
    {
        this._playback_player.Frames.SaveToCompressedFile("temp.playback");
    }
}