using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class StaticUIWidgets : MonoBehaviour
{
    public static float MIN_HIDE_WIDTH = 2.0f;
    public static float MIN_HIDE_HEIGHT = 1.2f;
    public static float MIN_SHOW_ZLENGTH = 0.38f;
    public static float SPEED = 5.0f;

    private bool _showing = false;
    public bool Showing
    {
        get
        {
            return this._showing;
        }
        set
        {
            this._showing = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        this.transform.position = new Vector3(0.0f, MIN_HIDE_HEIGHT);
    }

    // Update is called once per frame
    void Update()
    {
        if(this._showing)
        {
            var dest = new Vector3(0, 0, MIN_SHOW_ZLENGTH);
            if((this.transform.localPosition - dest).magnitude > 0.001f)
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, new Vector3(0, 0, MIN_SHOW_ZLENGTH), Time.deltaTime * SPEED);
        }
        else
            this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, new Vector3(0.0f, MIN_HIDE_HEIGHT, MIN_SHOW_ZLENGTH), Time.deltaTime * SPEED);
    }
}