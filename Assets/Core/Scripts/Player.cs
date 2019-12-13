using Leap;
using Leap.Unity;
using ParrotBebop2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public enum DroneControlDirection
    {
        Unknown = 0x0000000, Vertical = 0x00000001, Horizontal = 0x00000002
    }

    public enum DroneDirection
    {
        Unknown, Forward, Backward,
    }

    public Transform _head;

    public LeapProvider _provider;

    public Bebop _bebop;

    public StaticUIWidgets _widgets;

    public MeshRenderer ControlLockIcon;
    private Material _control_lock_icon_mat;

    private float _begin_pinch = 0.0f;
    private bool _control_lock;
    public bool ControlLock
    {
        get
        {
            return this._control_lock;
        }
        private set
        {
            this._control_lock = value;
            if(this._control_lock == false)
            {
                this._widgets.Showing = false;
                this.Rotating = false;
            }
        }
    }


    private bool _rotating = false;
    public bool Rotating
    {
        get
        {
            return this.ControlLock == false && this._rotating;
        }
        set
        {
            if(this.ControlLock)
                return;

            if(this._provider.LeftHand == null)
                return;

            this._rotating = value;
            if(this._rotating)
            {
                var angle = Vector3.Angle(this._provider.LeftHand.PalmNormal.ToVector3(), this._head.transform.up);
                this.CapturedAngle = angle;
            }
        }
    }

    private float _captured_angle = 0.0f;
    public float CapturedAngle
    {
        get
        {
            if(this.Rotating == false)
                throw new Exception("not rotation");

            if(this._provider.LeftHand == null)
                throw new Exception("no left hand");

            return _captured_angle;
        }
        private set
        {
            if(this.Rotating == false)
                throw new Exception("not rotation");

            if(this._provider.LeftHand == null)
                throw new Exception("no left hand");

            this._captured_angle = value;
        }
    }

    public Text DebugText;


    private float _begin_grab_time = 0.0f;

    public DroneDirection ControlForwardDirection { get; private set; }

    public DroneControlDirection ControlDirection { get; private set; }
    private BebopCommandSet.PCMD _pcmd;
    private static BebopCommandSet.PCMD EMPTY_PCMD;




    // Use this for initialization
    void Start()
    {
        this.ControlLock = true;

        this._control_lock_icon_mat = this.ControlLockIcon.material;
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            this._control_lock_icon_mat.color = Color.Lerp(this._control_lock_icon_mat.color, this.ControlLock ? new Color32(0, 0, 0, 0) : new Color32(255, 255, 255, 0), Time.deltaTime);

            foreach(var hand in this._provider.CurrentFrame.Hands)
            {
                var angle = Vector3.Angle(hand.PalmNormal.ToVector3(), this._head.transform.up);
                var cross = Vector3.Cross(hand.PalmNormal.ToVector3(), this._head.transform.up);

                this.OnHandRotation(hand.IsRight, angle * (cross.z > 0 ? 1 : -1));

                this.OnHandPositionChange(hand.IsRight, hand.PalmPosition.y);
            }

            lock(this._bebop)
            {
                this._bebop.pcmd = EMPTY_PCMD;
                if (this.ControlLock || this._provider.RightHand == null)
                    return;

                if(this.ControlForwardDirection == DroneDirection.Forward)
                {
                    this._pcmd.pitch = 2;
                }
                else if(this.ControlForwardDirection == DroneDirection.Backward)
                {
                    this._pcmd.pitch = -2;
                }
                else
                {
                    this._pcmd.pitch = 0;
                }

                if ((this.ControlDirection & DroneControlDirection.Horizontal) == DroneControlDirection.Horizontal || (this.ControlForwardDirection != DroneDirection.Unknown))
                {
                    this._bebop.pcmd.flag = 1;
                    this._bebop.pcmd.roll = this._pcmd.roll;
                    this._bebop.pcmd.pitch = this._pcmd.pitch;
                    //Debug.Log(string.Format("move horizon : {0}, {1}", this._pcmd.roll, this._pcmd.pitch));
                }

                else if ((this.ControlDirection & DroneControlDirection.Vertical) == DroneControlDirection.Vertical)
                {
                    this._bebop.pcmd.flag = 0;
                    this._bebop.pcmd.gaz = this._pcmd.gaz;
                    //Debug.Log(string.Format("move vertical : {0}", this._pcmd.gaz));
                }

                
                else
                {
                    this._bebop.pcmd = EMPTY_PCMD;
                    //Debug.Log(string.Format("no move"));
                }
            }
            Debug.Log(string.Format("{0}, {1}, {2}, {3}, {4}", (this.ControlDirection & DroneControlDirection.Horizontal) == DroneControlDirection.Horizontal ? 1 : 0, this._pcmd.roll, this._pcmd.pitch, this._pcmd.yaw, this._pcmd.gaz));
        }
        catch (Exception e)
        {
            Debug.Log(e.StackTrace);
        }
    }

    public void OnDebug(string text)
    {
        Debug.Log(text);
    }

    private void OnHandRotation(bool isRight, float angle)
    {
        if(isRight)
        {
            if (angle > 90 && angle < 140)                  // left
            {
                this._pcmd.roll = -2;
                this.ControlDirection |= DroneControlDirection.Horizontal;
            }
            else if (angle > -125.0f && angle < -70.0f)     // right
            {
                this._pcmd.roll = 2;
                this.ControlDirection |= DroneControlDirection.Horizontal;
            }
            else                                            // none
            {
                this._pcmd.roll = 0;
                this.ControlDirection &= ~DroneControlDirection.Horizontal;
            }
        }
        else
        {
            try
            {
                if (this.Rotating)
                {
                    var difference = Mathf.Abs(angle) - this.CapturedAngle;
                    
                    if(difference > 10.0f)
                        Debug.Log("turn left");
                    else if(difference < -10.0f)
                        Debug.Log("turn right");
                    else
                        Debug.Log("no turn");
                }
            }
            catch(Exception)
            { }
        }
    }

    private void OnHandPositionChange(bool isRight, float value)
    {
        if(isRight)
        {
            var offset = (value - this._head.localPosition.y);
            if (offset > 1.05f)
            {
                this._pcmd.gaz = 7;
                this.ControlDirection |= DroneControlDirection.Vertical;
            }
            else if (offset > 0.8f)
            {
                this._pcmd.gaz = 0;
                this.ControlDirection &= ~DroneControlDirection.Vertical;
            }
            else
            {
                this._pcmd.gaz = -7;
                this.ControlDirection |= DroneControlDirection.Vertical;
            }
        }
        else
        {

        }
    }

    public void OnPinchActive()
    {
        this._begin_pinch = Time.time;
    }

    public void OnPinchDeactive()
    {
        var elapsed = Time.time - this._begin_pinch;
        if (elapsed < 0.25f)
        {
            this.ControlLock = !this.ControlLock;
            if(this._control_lock_icon_mat != null)
                this._control_lock_icon_mat.color = this.ControlLock ?  new Color32(0, 0, 0, 255) : new Color32(255, 255, 255, 255);
        }
    }

    public void OnVisibilityUI(bool visible)
    {
        if (this.ControlLock == false)
        {
            Debug.Log("control lock is active");
            return;
        }

        this._widgets.Showing = visible;
    }

    public void OnForwardDrone()
    {
        this.ControlForwardDirection = DroneDirection.Forward;
    }

    public void OnBackwardDrone()
    {
        this.ControlForwardDirection = DroneDirection.Backward;
    }

    public void OnUnknownDirectionDrone()
    {
        this.ControlForwardDirection = DroneDirection.Unknown;
    }

    public void OnGrabLeftHand()
    {
        this._begin_grab_time = Time.time;
    }

    public void OnExtendAllLeftHand()
    {
        var elapsed_time = Time.time - this._begin_grab_time;
        if(elapsed_time < 0.25f)
            this.Rotating = !this.Rotating;
    }
}