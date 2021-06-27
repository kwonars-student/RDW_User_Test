using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Redirection;

public class RFLTerrainCorrector : MonoBehaviour
{
    RedirectionManager rm;
    ResetTriggeringController rtc;

    // Start is called before the first frame update
    void Start()
    {
        // rm = GameObject.Find("Redirected User").GetComponent<RedirectionManager>();
        // rtc = GameObject.Find("Redirected User").GetComponent<ResetTriggeringController>();
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    public void CorrectMap()
    {
        //Debug.Log("difference: "+(rm.currPosReal - new Vector3(2*GeometryInfo.resetLocations[rtc.minIndex].x, 0f, 2*GeometryInfo.resetLocations[rtc.minIndex].y)));
        //this.transform.position = this.transform.position + (rm.currPosReal - new Vector3(GeometryInfo.resetLocations[rtc.minIndex].x, 0f, GeometryInfo.resetLocations[rtc.minIndex].y));
    }
}
