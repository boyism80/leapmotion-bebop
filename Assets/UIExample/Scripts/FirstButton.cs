using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnDebug(string message)
    {
        Debug.Log(message);
    }

    public void OnFirstActive()
    {
        this.gameObject.SetActive(true);
    }
}
