using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResetTrigger : MonoBehaviour {

    [HideInInspector]
    public RedirectionManager redirectionManager;
    [HideInInspector]
    public Collider bodyCollider;

    [SerializeField, Range(0f, 1f)]
    public float RESET_TRIGGER_BUFFER;

    [HideInInspector]
    public List<GameObject> childObjects;

    [HideInInspector]
    public List<CollisionChecker> collisionCheckers;

    [HideInInspector]
    public List<Vector3> planeDirections;

    [HideInInspector]
    public float xLength, zLength;

    public void Initialize()
    {
        // Set Size of Collider
        float trimAmountOnEachSide = 2 * RESET_TRIGGER_BUFFER;
        this.transform.localScale = new Vector3(1 - (trimAmountOnEachSide / this.transform.parent.localScale.x), 0.2f, 1 - (trimAmountOnEachSide / this.transform.parent.localScale.z));
        xLength = this.transform.parent.localScale.x - trimAmountOnEachSide;
        zLength = this.transform.parent.localScale.z - trimAmountOnEachSide;

        for(int i = 0; i < this.transform.childCount; i++)
        {
            childObjects.Add(this.transform.GetChild(i).gameObject);
        }
        for(int i = 0; i < childObjects.Count; i++)
        {
            childObjects[i].AddComponent<CollisionChecker>();
        }
        for(int i = 0; i < childObjects.Count; i++)
        {
            collisionCheckers.Add(childObjects[i].GetComponent<CollisionChecker>());
        }


        for(int i = 0; i < collisionCheckers.Count; i++)
        {
            collisionCheckers[i].redirectionManager = redirectionManager;
            collisionCheckers[i].colliderObject = bodyCollider;
        }

    }

    void OnTriggerEnter(Collider other)
    {

    }

    void OnTriggerExit(Collider other)
    {
        //Debug.Log("You Did");
    }

    

}
