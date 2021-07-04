using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Redirection;
using Valve.VR;

public class ResetTriggeringController : MonoBehaviour
{
    public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");

    RedirectionManager rm;
    //TwoOneTurnResetter twoOneTurnResetter;
    //Transform vrCamTransform;
    [HideInInspector]
    List<float> distancesFromResets;

    [HideInInspector]
    public Vector3 triggeredResetPosition;

    [HideInInspector]
    public int minIndex;

    // [SerializeField]
    // bool TilingMode = false;

    // Start is calleD before the first frame update
    void Start()
    {
        rm = GameObject.Find("Redirected User").GetComponent<RedirectionManager>();

        // if(TilingMode) rm.setTilingMode();
        // //Debug.Log("Set");

        // twoOneTurnResetter = GameObject.Find("Redirected User").GetComponent<TwoOneTurnResetter>();

        //vrCamTransform = GameObject.Find("VRCamera").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {

        distancesFromResets = new List<float>();
        //Vector3 vrCam = vrCamTransform.position;
        //Debug.Log(vrCam);
        //rm.UpdateTrackedSpaceLocation(1f,5f);

        for(int i = 0; i < GeometryInfo.resetLocations.Count; i++)
        {
            //Debug.Log(rm.currPosReal);
            distancesFromResets.Add((new Vector3(GeometryInfo.resetLocations[i].x, 0f, GeometryInfo.resetLocations[i].y) - rm.currPosReal).magnitude);
        }
        minIndex = distancesFromResets.IndexOf(distancesFromResets.Min());

        if(distancesFromResets.Min() < 0.3 && !rm.inReset)
        {
            // Debug.Log("rm.resetter.state: "+rm.resetter.state);
            // Debug.Log("minIndex: "+minIndex);
            
            // Show "Ready"
            if(rm.resetter.state == 1 || rm.resetter.state == 2)
            {
                GameObject.Find("TurnReadySign").GetComponent<Canvas>().enabled = true;
            }
            else if( (rm.resetter.state == 3 || rm.resetter.state == 4) && (minIndex == 0 || minIndex == 2) )
            {
                GameObject.Find("TurnReadySign").GetComponent<Canvas>().enabled = true;   
            }
            else
            {
                GameObject.Find("TurnReadySign").GetComponent<Canvas>().enabled = false;
            }
            

            if(grabPinchAction.GetStateDown(SteamVR_Input_Sources.Any))
            {
                GameObject.Find("TurnReadySign").GetComponent<Canvas>().enabled = false;
                rm.setControllerTriggered();
                triggeredResetPosition = rm.currPosReal;
            }
        }
        else
        {
            GameObject.Find("TurnReadySign").GetComponent<Canvas>().enabled = false;
        }
        
    }
}
