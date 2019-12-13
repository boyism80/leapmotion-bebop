using UnityEngine;

public class benny : MonoBehaviour
{
    public Rigidbody[] InterationRigidbodyList;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnHandUp()
    {
        foreach(var rigidbody in this.InterationRigidbodyList)
        {
            rigidbody.velocity += Vector3.up * 10.0f;
        }
    }

    public void OnHandSwipe()
    {
        foreach(var rigidbody in this.InterationRigidbodyList)
        {
            rigidbody.velocity += Vector3.left * 10.0f;
        }
    }

    public void OnHandRightSwipe()
    {
        foreach(var rigidbody in this.InterationRigidbodyList)
        {
            rigidbody.velocity += Vector3.right * 10.0f;
        }
    }

    public void OnShot()
    {
        foreach(var rigidbody in this.InterationRigidbodyList)
        {
            if(rigidbody.gameObject.activeSelf)
            {
                rigidbody.gameObject.SetActive(false);
                break;
            }
        }
    }

    public void OnPinch()
    {
        foreach(var rigidbody in this.InterationRigidbodyList)
        {
            if(!rigidbody.gameObject.activeSelf)
            {
                rigidbody.gameObject.SetActive(true);
                break;
            }
        }
    }
}
