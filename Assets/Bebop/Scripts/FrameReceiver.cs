using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using OpenCvSharp;
using System;
using System.Threading;

public class FrameReceiver : MonoBehaviour {

    private Material _mat;
    //private GUITexture _gui_texture;
    private Texture2D _texture;
    private Queue<Mat> _frame_queue;

    private bool _running;
    public bool Running
    {
        get
        {
            return this._running;
        }

        private set
        {
            this._running = value;
        }
    }

    private bool _idle;
    public bool Idle
    {
        get
        {
            return this._idle;
        }
        set
        {
            this._idle = value;
            if(this.Idle)
                this._frame_queue.Clear();
        }
    }

    private bool _streamingNow;
    public bool StreamingNow
    {
        get
        {
            return this._streamingNow;
        }
        private set
        {
            this._streamingNow = value;
        }
    }

	// Use this for initialization
	void Start () {

        var mesh_renderer = this.GetComponent<MeshRenderer>();
        this._mat = mesh_renderer.material;

        //this._gui_texture = this.GetComponent<GUITexture>();
        //this._gui_texture.pixelInset = new UnityEngine.Rect(0, 0, Screen.width, Screen.height);

        this._texture = new Texture2D(1024, 768, TextureFormat.RGB24, false);
        //this._gui_texture.texture = this._texture;
        this._mat.mainTexture = this._texture;

        this._frame_queue = new Queue<Mat>();

        var exists = File.Exists("./bebop.sdp");
        var capture_thread = new Thread(this.DroneVideoCapture);
        capture_thread.Start();
	}
	
	// Update is called once per frame
	void Update () {

        try
        {
            var mat = this._frame_queue.Dequeue();
            this._texture.LoadRawTextureData(mat.Data, (int)mat.Total() * (int)mat.ElemSize());
            this._texture.Apply();
        }
        catch(Exception)
        {
        }
	}

    public void DroneVideoCapture()
    {
        this.Running = true;

        var cap = new VideoCapture("./bebop.sdp");
        while(this.Running)
        {
            if(this.StreamingNow == false)
            {
                Thread.Sleep(100);
                continue;
            }

            try
            {
                var mat = new Mat();
                if(cap.Read(mat) == false)
                    throw new Exception("cannot read");

                if(mat.Empty())
                    throw new Exception("matrix is empty");

                Cv2.Resize(mat, mat, new Size(1024, 768));
                Cv2.Flip(mat, mat, FlipMode.X);
                Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2RGB);
                this._frame_queue.Enqueue(mat);
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        cap.Release();
    }

    public void OnEnable()
    {
        this.StreamingNow = true;
    }

    public void OnDisable()
    {
        this.StreamingNow = false;
    }

    public void OnDestroy()
    {
        this.Running = false;
    }
}