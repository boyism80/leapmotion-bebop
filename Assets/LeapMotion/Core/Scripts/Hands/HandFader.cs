using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

public class HandFader : MonoBehaviour
{
    public float confidenceSmoothing = 10.0f;
    public AnimationCurve confidenceCurve;

    protected HandModel _handModel;
    protected float _smoothedConfidence = 0.0f;
    protected Renderer _renderer;

    private const float EPISLON = 0.005f;

    protected virtual void Awake()
    {
        _handModel = GetComponent<HandModel>();
        _renderer = GetComponentInChildren<Renderer>();
        _renderer.material.SetFloat("_Fade", 0);
    }

    protected virtual void Update()
    {
        var hand = _handModel.GetLeapHand();
        if(hand == null)
            return;

        float unsmoothedConfidence = hand.Confidence;
        _smoothedConfidence += (unsmoothedConfidence - _smoothedConfidence) / confidenceSmoothing;
        float fade = confidenceCurve.Evaluate(_smoothedConfidence);
        _renderer.enabled = (fade > EPISLON);
        _renderer.material.SetFloat("_Fade", confidenceCurve.Evaluate(_smoothedConfidence));
    }
}