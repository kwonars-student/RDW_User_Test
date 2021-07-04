using UnityEngine;
using System.Collections;
using Redirection;

public class RFL_Resetter : Resetter {

    float overallInjectedRotation;
    
    private Transform prefabHUD = null;
    
    Transform instanceHUD;

    Vector3 virtualCenter;

    ResetTriggeringController rtc;

    int resetIndex = 0;

    public override bool IsResetRequired()
    {
        return !isUserFacingAwayFromWall();
    }

    public override void InitializeReset()
    {
        overallInjectedRotation = 0;
        if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.RoomType)
        {
            for(int i = 0; i < GameObject.Find("DoorsForRoom").transform.childCount; i++)
            {
                GameObject.Find("DoorsForRoom").transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.SquareType)
        {
            for(int i = 0; i < GameObject.Find("DoorsForSquare").transform.childCount; i++)
            {
                GameObject.Find("DoorsForSquare").transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        virtualCenter = redirectionManager.trackedSpace.position; //current Plane 위치
        rtc = GameObject.Find("Redirected User").GetComponent<ResetTriggeringController>();
        SetHUD();
    }

    public override void ApplyResetting()
    {

        //Debug.Log("currDirReal: "+ new Vector2(redirectionManager.currDirReal.x, redirectionManager.currDirReal.z).normalized + 0f*redirectionManager.currDirReal);
        if (Mathf.Abs(overallInjectedRotation) < 180)
        {
            float remainingRotation = redirectionManager.deltaDir > 0 ? 180 - overallInjectedRotation : -180 - overallInjectedRotation; // The idea is that we're gonna keep going in this direction till we reach objective
            if (Mathf.Abs(remainingRotation) < Mathf.Abs(redirectionManager.deltaDir))
            {
                InjectRotation(remainingRotation);
                redirectionManager.OnResetEnd();
                overallInjectedRotation += remainingRotation;

                if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.RoomType)
                {
                    for(int i = 0; i < GameObject.Find("DoorsForRoom").transform.childCount; i++)
                    {
                        GameObject.Find("DoorsForRoom").transform.GetChild(i).gameObject.SetActive(true);
                    }                    
                }
                else if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.SquareType)
                {
                    for(int i = 0; i < GameObject.Find("DoorsForSquare").transform.childCount; i++)
                    {
                        GameObject.Find("DoorsForSquare").transform.GetChild(i).gameObject.SetActive(true);
                    }
                }

                // Move Up
                if(rtc.minIndex == 0 && state == 1)
                {
                    state = 3;
                    resetIndex = 0;
                    if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.RoomType)
                    {
                        GameObject.Find("DoorsForRoom").transform.GetChild(1).gameObject.SetActive(false);
                        GameObject.Find("DoorsForRoom").transform.GetChild(3).gameObject.SetActive(false);
                    }
                    else if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.SquareType)
                    {
                        GameObject.Find("DoorsForSquare").transform.GetChild(1).gameObject.SetActive(false);
                        GameObject.Find("DoorsForSquare").transform.GetChild(3).gameObject.SetActive(false);
                    }
                }
                else if(rtc.minIndex == 0 && state == 2)
                {
                    state = 4;
                    resetIndex = 2;
                    if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.RoomType)
                    {
                        GameObject.Find("DoorsForRoom").transform.GetChild(1).gameObject.SetActive(false);
                        GameObject.Find("DoorsForRoom").transform.GetChild(3).gameObject.SetActive(false);
                    }
                    else if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.SquareType)
                    {
                        GameObject.Find("DoorsForSquare").transform.GetChild(1).gameObject.SetActive(false);
                        GameObject.Find("DoorsForSquare").transform.GetChild(3).gameObject.SetActive(false);
                    }
                }
                else if(rtc.minIndex == 0 && state == 3)
                {
                    state = 1;
                    resetIndex = 2;
                }
                else if(rtc.minIndex == 0 && state == 4)
                {
                    state = 2;
                    resetIndex = 0;
                }

                // Move Right
                else if(rtc.minIndex == 1 && state == 1)
                {
                    state = 2;
                    resetIndex = 1;
                }
                else if(rtc.minIndex == 1 && state == 2)
                {
                    state = 1;
                    resetIndex = 3;
                }
                else if(rtc.minIndex == 1 && state == 3)
                {
                    state = 4;
                    resetIndex = 3;
                }
                else if(rtc.minIndex == 1 && state == 4)
                {
                    state = 3;
                    resetIndex = 1;
                }

                // Move Down
                else if(rtc.minIndex == 2 && state == 1)
                {
                    state = 3;
                    resetIndex = 2;
                    if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.RoomType)
                    {
                        GameObject.Find("DoorsForRoom").transform.GetChild(1).gameObject.SetActive(false);
                        GameObject.Find("DoorsForRoom").transform.GetChild(3).gameObject.SetActive(false);
                    }
                    else if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.SquareType)
                    {
                        GameObject.Find("DoorsForSquare").transform.GetChild(1).gameObject.SetActive(false);
                        GameObject.Find("DoorsForSquare").transform.GetChild(3).gameObject.SetActive(false);
                    }
                }
                else if(rtc.minIndex == 2 && state == 2)
                {
                    state = 4;
                    resetIndex = 0;
                    if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.RoomType)
                    {
                        GameObject.Find("DoorsForRoom").transform.GetChild(1).gameObject.SetActive(false);
                        GameObject.Find("DoorsForRoom").transform.GetChild(3).gameObject.SetActive(false);
                    }
                    else if(redirectionManager.spaceShape == GeometryInfo.SpaceShape.SquareType)
                    {
                        GameObject.Find("DoorsForSquare").transform.GetChild(1).gameObject.SetActive(false);
                        GameObject.Find("DoorsForSquare").transform.GetChild(3).gameObject.SetActive(false);
                    }
                }
                else if(rtc.minIndex == 2 && state == 3)
                {
                    state = 1;
                    resetIndex = 0;
                }
                else if(rtc.minIndex == 2 && state == 4)
                {
                    state = 2;
                    resetIndex = 2;
                }

                // Move Left
                else if(rtc.minIndex == 3 && state == 1)
                {
                    state = 2;
                    resetIndex = 3;
                }
                else if(rtc.minIndex == 3 && state == 2)
                {
                    state = 1;
                    resetIndex = 1;
                }
                else if(rtc.minIndex == 3 && state == 3)
                {
                    state = 4;
                    resetIndex = 1;
                }
                else if(rtc.minIndex == 3 && state == 4)
                {
                    state = 3;
                    resetIndex = 3;
                }

                Vector3 movedVector = redirectionManager.trackedSpace.position - virtualCenter;
                Vector3 expectedVector = 2*(new Vector3(GeometryInfo.resetLocations[resetIndex].x, 0f, GeometryInfo.resetLocations[resetIndex].y));
                GameObject.Find("Terrain").gameObject.transform.position = GameObject.Find("Terrain").gameObject.transform.position - (expectedVector - movedVector);
                virtualCenter = redirectionManager.trackedSpace.position;
            }
            else
            {
                InjectRotation(redirectionManager.deltaDir);
                overallInjectedRotation += redirectionManager.deltaDir;
            }
        }
    }

    public override void FinalizeReset()
    {
        // Destroy(instanceHUD.gameObject); Original Spin In Place object
        GameObject.Find("TurnAroundSign").GetComponent<Canvas>().enabled = false;
        
    }

    public void SetHUD()
    {
        // Beloew are Spin In Place Codes

        // if (prefabHUD == null)
        //     prefabHUD = Resources.Load<Transform>("TwoOneTurnResetter HUD");
        // instanceHUD = Instantiate(prefabHUD);
        // instanceHUD.parent = redirectionManager.headTransform;
        // instanceHUD.localPosition = instanceHUD.position;
        // instanceHUD.localRotation = instanceHUD.rotation;

        GameObject.Find("TurnAroundSign").GetComponent<Canvas>().enabled = true;
    }

    public override void SimulatedWalkerUpdate()
    {
        // Act is if there's some dummy target a meter away from you requiring you to rotate
        //redirectionManager.simulatedWalker.RotateIfNecessary(180 - overallInjectedRotation, Vector3.forward);
        redirectionManager.simulatedWalker.RotateInPlace();
        //print("overallInjectedRotation: " + overallInjectedRotation);
    }

}
