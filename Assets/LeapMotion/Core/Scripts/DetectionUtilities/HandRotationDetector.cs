using Leap;
using Leap.Unity;
using Leap.Unity.Attributes;
using System;
using System.Collections;
using UnityEngine;

public class HandRotationDetector : Detector {

    public enum PalmDirection { Pitch, Yaw, Roll }
    public enum BetweenType { Positive, Negative, Both }

    private IEnumerator _watcherCoroutine;
    private float _old;
    private bool _isInitialized = false;

    [Tooltip("The interval in seconds at which to check this detector's conditions.")]
    [Units("seconds")]
    [MinValue(0)]
    public float period = .1f; //seconds

    [AutoFind(AutoFindLocations.Parents)]
    [Tooltip("The hand model to watch. Set automatically if detector is on a hand.")]
    public IHandModel handModel = null;

    [Header("Direction Settings")]
    public PalmDirection palmDirection;

    [Range(0, 180)]
    public float angleToActive = 30;
    public BetweenType detectDirection;

    public Transform head;

    public void Awake()
    {
        this._watcherCoroutine = this.handGestureWatcher();
    }

    public void OnEnable()
    {
        StartCoroutine(this._watcherCoroutine);
    }

    public void OnDisable()
    {
        StopCoroutine(this._watcherCoroutine);
        base.Deactivate();
    }

    private float angle(Hand hand)
    {
        switch (this.palmDirection)
        {
            case PalmDirection.Pitch:
                return Vector3.Angle(this.head.transform.forward, hand.PalmNormal.ToVector3());

            case PalmDirection.Yaw:
                return Vector3.Angle(this.head.transform.forward, hand.Direction.ToVector3());

            case PalmDirection.Roll:
                return Vector3.Angle(this.head.transform.up, hand.PalmNormal.ToVector3());

            default:
                return 0;
        }
    }

    IEnumerator handGestureWatcher()
    {
        while(true)
        {
            try
            {
                var hand                    = this.handModel.GetLeapHand();
                if (hand == null)
                    throw new Exception();

                var current                 = this.angle(hand);
                var difference              = current - this._old;
                this._old                   = current;
                if(this._isInitialized == false)
                {
                    this._isInitialized = true;
                    continue;
                }

                var direction               = difference > 0.0f ? BetweenType.Positive : BetweenType.Negative;
                if (Mathf.Abs(difference) > this.angleToActive)
                {
                    if (this.detectDirection == BetweenType.Both || this.detectDirection == direction)
                        base.Activate();
                }
                else
                {
                    base.Deactivate();
                }
            }
            catch(Exception)
            {
                this._isInitialized = false;
            }
            yield return new WaitForSeconds(this.period);
        }
    }
}
