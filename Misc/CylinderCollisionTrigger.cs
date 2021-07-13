using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CylinderCollisionTrigger : MonoBehaviour {

    [HideInInspector]
    public RedirectionManager redirectionManager;
    [HideInInspector]
    public Collider bodyCollider;
    [HideInInspector]
    public List<GameObject> childObjects;

    [HideInInspector]
    public List<CylinderCollisionChecker> cylinderCollisionCheckers;

    public void Initialize()
    {
        for(int i = 0; i < this.transform.childCount; i++)
        {
            childObjects.Add(this.transform.GetChild(i).gameObject);
        }
        for(int i = 0; i < childObjects.Count; i++)
        {
            childObjects[i].AddComponent<CylinderCollisionChecker>();
        }
        for(int i = 0; i < childObjects.Count; i++)
        {
            cylinderCollisionCheckers.Add(childObjects[i].GetComponent<CylinderCollisionChecker>());
        }
        for(int i = 0; i < cylinderCollisionCheckers.Count; i++)
        {
            cylinderCollisionCheckers[i].redirectionManager = redirectionManager;
            cylinderCollisionCheckers[i].colliderObject = bodyCollider;
        }
    }

    // void OnTriggerEnter(Collider other)
    // {

    // }

    // void OnTriggerExit(Collider other)
    // {

    // }

    

}
