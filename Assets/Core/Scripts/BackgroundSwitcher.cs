using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSwitcher : MonoBehaviour {

    [Serializable]
    public class BackgroundStateGroup
    {
        public string _name;
        public string _hand_name;
        public MeshRenderer _background;
    }

    public BackgroundStateGroup[] _background_states;
    private string _background_state;
    public string BackgroundState
    {
        get
        {
            return this._background_state;
        }
        set
        {
            this._background_state = value;
            BackgroundStateGroup target = null;
            foreach(var state in this._background_states)
            {
                if(state._name.Equals(this._background_state))
                {
                    target = state;
                    continue;
                }

                if(state._background != null)
                    state._background.gameObject.SetActive(false);

                this._hand_pool.DisableGroup(state._hand_name);
            }

            if(target != null)
            {
                if(target._background != null)
                    target._background.gameObject.SetActive(true);

                this._hand_pool.EnableGroup(target._hand_name);
            }
        }
    }

    public HandPool _hand_pool;

	// Use this for initialization
	public void Start () {

        this.BackgroundState = "vr";
    }
	
	// Update is called once per frame
	public void Update () {
	
	}
}