using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ViveInput : MonoBehaviour
{
    public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");

    RedirectionManager rm;
    TwoOneTurnResetter twoOneTurnResetter;
    Transform vrCamTransform;

    // Start is calleD before the first frame update
    void Start()
    {
        rm = GameObject.Find("Redirected User").GetComponent<RedirectionManager>();
        rm.setTilingMode();
        //Debug.Log("Set");

        twoOneTurnResetter = GameObject.Find("Redirected User").GetComponent<TwoOneTurnResetter>();

        vrCamTransform = GameObject.Find("VRCamera").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {     
        Vector3 vrCam = vrCamTransform.position;
        //Debug.Log(vrCam);
        //rm.UpdateTrackedSpaceLocation(1f,5f);
        if(grabPinchAction.GetStateDown(SteamVR_Input_Sources.Any))
        {
            //Debug.Log("Down");
            //terrainPos = new Vector3 (0f,0.6f,0f);
            rm.setControllerTriggered();
            //GameObject.Find("Terrain").GetComponent<TerrainCorrector>().makeZero();
        }
        
    }
}
