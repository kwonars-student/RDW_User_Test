using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionChecker : MonoBehaviour {

    [HideInInspector]
    public RedirectionManager redirectionManager;
    [HideInInspector]
    public Collider colliderObject;

    public void Initialize()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other == colliderObject && Vector3.Dot(this.transform.up, redirectionManager.currDir) < 0)
        {
            redirectionManager.OnResetTrigger();
            Debug.Log(this.transform.name);
            //Debug.Log();
        }
    }

    void OnTriggerExit(Collider other)
    {

    }

    

}
