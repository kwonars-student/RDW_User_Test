using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Redirection;

public class APF_R_Resetter : Resetter 
{
    float overallInjectedRotation;
    Vector2 curPos;
    Vector2 curFwd;

    Vector2 obtainedW;
    float targetAngle;
    float rotationGainMinusOne;
    bool isTurnLeft = false;
    public override bool IsResetRequired()
    {
        return true;
        // return !isUserFacingAwayFromWall();
    }

    public override void InitializeReset()
    {
        getCurInfo();
        
        // // Using old W:
        // GetOldW();

        // Using new W:
        (Vector2 w, float t) = GetWandT();
        obtainedW = w;

        targetAngle = Vector2.SignedAngle(curFwd, obtainedW);
        isTurnLeft = targetAngle < 0 ? true : false;

        targetAngle = 360 - Mathf.Abs(targetAngle);

        // if(targetAngle > 0)
        // {
        //     targetAngle = targetAngle - 360;        // 이러면 목표 각도는 항상 음수로 표현됨.
        // }

        // if(Mathf.Abs(targetAngle)<180)
        // {
        //     targetAngle = 360 + targetAngle; //  회전해야 할 크기를 표현. 항상 양수.
        //     isTurnLeft = true;
        // }
        // else
        // {
        //     targetAngle = - targetAngle; //  회전해야 할 크기를 표현. 항상 양수.
        //     isTurnLeft = false;
        // }

        overallInjectedRotation = 0;
        //Debug.Log("RotationGain: "+Mathf.Abs((360-Mathf.Abs(targetAngle))/(targetAngle)));
        rotationGainMinusOne = (360/targetAngle) - 1;
        SetHUD();
    }

    private void GetOldW()
    {

        // define some variables for redirection
        // Vector2 curPos = new Vector2(redirectionManager.currPosReal.x, redirectionManager.currPosReal.z);
        // Vector2 curFwd = new Vector2(redirectionManager.currDirReal.x, redirectionManager.currDirReal.z).normalized;
        
        Vector2 userPosition = curPos - 0.02f*curFwd;
        Vector2 userDirection = curFwd;

        List<Vector2> dList = new List<Vector2>();
        List<float> inverseDList = new List<float>();
        float dAbsSum = 0;

        Vector2 w = Vector2.zero;
        Vector2 middleToUser = Vector2.zero;

        // dAbsSum과 dList, inverseDList를 우선 구함
        for(int i=0; i < GeometryInfo.middleVertices.Count; i++)
        {
            middleToUser = userPosition - GeometryInfo.middleVertices[i];
            inverseDList.Add(1/Vector2.Dot(GeometryInfo.edgeNormalVectors[i],middleToUser));
            dList.Add(Vector2.Dot(GeometryInfo.edgeNormalVectors[i],middleToUser)*(GeometryInfo.edgeNormalVectors[i]));

            if(Vector2.Dot(middleToUser, GeometryInfo.edgeNormalVectors[i]) > 0 )
            {
                dAbsSum += Vector2.Dot(GeometryInfo.edgeNormalVectors[i],middleToUser);
            }
        }

        // w계산
        for(int i=0; i < GeometryInfo.middleVertices.Count; i++)
        {
            middleToUser = userPosition - GeometryInfo.middleVertices[i];
            if(Vector2.Dot(middleToUser, GeometryInfo.edgeNormalVectors[i]) > 0 )
            {
                w += dList[i]*inverseDList[i]*inverseDList[i]*dAbsSum;
            }
            else
            {
                ;// Do Nothing
            }
        }
        this.obtainedW = w;
    }

    private (Vector2, float) GetWandT()
    {
        const float C = 0.00897f;
        const float lambda = 2.656f;
        const float r = 7.5f;
        const float gamma = 3.091f;
        const float M = 15f;

        // define some variables for redirection
        Vector2 userPosition = new Vector2(redirectionManager.currPosReal.x, redirectionManager.currPosReal.z);

        List<Vector2> dList = new List<Vector2>();
        List<float> dListMagnitude = new List<float>();
        List<Vector2> dNormalizedList = new List<Vector2>();
        List<float> inverseDList = new List<float>();

        for(int i=0; i < GeometryInfo.segmentedVertices.Count; i++)
        {
            dList.Add(userPosition - GeometryInfo.segmentedVertices[i]);
        }

        for(int i=0; i < dList.Count; i++)
        {
            dListMagnitude.Add(dList[i].magnitude);
        }

        for(int i=0; i < dList.Count; i++)
        {
            dNormalizedList.Add(dList[i].normalized);
        }

        for(int i=0; i < dList.Count; i++)
        {
            inverseDList.Add(Mathf.Pow(1/dList[i].magnitude, lambda));
        }

        Vector2 w = Vector2.zero;
        for(int i=0; i < GeometryInfo.segmentedVertices.Count; i++)
        {
            if(Vector2.Dot(GeometryInfo.segmentNormalVectors[i], dNormalizedList[i]) > 0)
            {
                // Debug.Log("GeometryInfo.segmentedVertices["+i+"]: "+GeometryInfo.segmentedVertices[i]);
                // Debug.Log("GeometryInfo.segmentNormalVectors["+i+"]: "+GeometryInfo.segmentNormalVectors[i]);
                w += C*GeometryInfo.segmentedEdgeLengths[i]*dNormalizedList[i]*inverseDList[i];
            }
            else
            {
                ;// Do Nothing
            }
        }

        return (w, 1 - dListMagnitude.Min()*Mathf.Abs(1/r));
    }

    private void getCurInfo()
    {
        curPos = new Vector2(redirectionManager.currPosReal.x, redirectionManager.currPosReal.z);
        curFwd = new Vector2(redirectionManager.currDirReal.x, redirectionManager.currDirReal.z).normalized;
    }

    public override void ApplyResetting()
    {
        float remainingRotation = targetAngle - overallInjectedRotation;
        //Debug.Log("obtainedW: "+obtainedW);
        if (remainingRotation < Mathf.Abs(redirectionManager.deltaDir))
        {
            // Debug.Log("redirectionManager.deltaDir: "+redirectionManager.deltaDir);
            // Debug.Log("remainingRotation: "+remainingRotation);

            InjectRotation(remainingRotation);
            redirectionManager.OnResetEnd();
            overallInjectedRotation += remainingRotation;
        }
        else
        {
            // InjectRotation(redirectionManager.deltaDir); // 흰색 Body가 도는 것(deltaDir)에 더해 Plane이 이만큼 더 돈다는 뜻. deltaDir를 넣으면 2배 더 도는 것.
            InjectRotation(rotationGainMinusOne*redirectionManager.deltaDir); //theta + d theta = G theta. 따라서 d theta = (G-1)theta
            //Debug.Log("redirectionManager.deltaDir: "+redirectionManager.deltaDir); // +1.8도
            overallInjectedRotation += redirectionManager.deltaDir; // deltaDir이나 deltaDirReal이나 값이 같으므로 상관 없음.
        }
    }

    public override void FinalizeReset()
    {
        // Destroy(instanceHUD.gameObject); Original Spin In Place object
        GameObject.Find("TurnLeftSign").GetComponent<Canvas>().enabled = false;
        GameObject.Find("TurnRightSign").GetComponent<Canvas>().enabled = false;
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

        if(isTurnLeft)
        {
            GameObject.Find("TurnLeftSign").GetComponent<Canvas>().enabled = true;
        }
        else
        {
            GameObject.Find("TurnRightSign").GetComponent<Canvas>().enabled = true;
        }
    }

    public override void SimulatedWalkerUpdate()
    {
        // Act is if there's some dummy target a meter away from you requiring you to rotate
        //redirectionManager.simulatedWalker.RotateIfNecessary(180 - overallInjectedRotation, Vector3.forward);
        redirectionManager.simulatedWalker.RotateInPlace();
        //print("overallInjectedRotation: " + overallInjectedRotation);
    }

}
