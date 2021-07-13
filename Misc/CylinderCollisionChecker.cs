using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CylinderCollisionChecker : MonoBehaviour {

    [HideInInspector]
    public RedirectionManager redirectionManager;

    [HideInInspector]
    public Collider colliderObject;

    // public void Initialize()
    // {

    // }

    void OnTriggerEnter(Collider other)
    {
        if (other == colliderObject)
        {
            redirectionManager.setNeedChange();
        }
    }

    // void OnTriggerExit(Collider other)
    // {

    // }

    

}
