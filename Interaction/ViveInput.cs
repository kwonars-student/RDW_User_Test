using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ViveInput : MonoBehaviour
{
    //// Start is calleD before the first frame update
    //void Start()
    //{

    //}

    public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");

    // Update is called once per frame
    void Update()
    {
        RedirectionManager rm = GameObject.Find("Redirected User").GetComponent<RedirectionManager>();
        Vector3 vrCam = GameObject.Find("VRCamera").GetComponent<Transform>().position;

        Debug.Log(vrCam);
        //rm.UpdateTrackedSpaceLocation(1f,5f);
        if(grabPinchAction.GetStateDown(SteamVR_Input_Sources.Any))
        {
            //Debug.Log("Down");
            //terrainPos = new Vector3 (0f,0.6f,0f);
            GameObject.Find("Terrain").GetComponent<TerrainCorrector>().makeZero();
        }
        
    }
}
