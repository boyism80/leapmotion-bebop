using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repositioning : MonoBehaviour
{
    private Vector3 _init_position;
    private Rigidbody _rigidbody;
    private InteractionBehaviour _interaction;

    // Use this for initialization
    void Start()
    {
        this._init_position = this.transform.localPosition;
        this._rigidbody = this.GetComponent<Rigidbody>();
        this._interaction = this.GetComponent<InteractionBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        if(this._interaction.isGrasped)
            return;

        this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this._init_position, Time.deltaTime * 5.0f);
        this._rigidbody.velocity = Vector3.Lerp(this._rigidbody.velocity, Vector3.zero, Time.deltaTime * 5.0f);
    }

    public void OnActive()
    {
        Debug.Log("active");
    }
}
