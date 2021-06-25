using UnityEngine;
using System.Collections;

public class RFL_Resetter : Resetter {

    float overallInjectedRotation;
    
    private Transform prefabHUD = null;
    
    Transform instanceHUD;

    public override bool IsResetRequired()
    {
        return !isUserFacingAwayFromWall();
    }

    public override void InitializeReset()
    {
        overallInjectedRotation = 0;
        for(int i = 0; i < GameObject.Find("Doors").transform.childCount; i++)
        {
            GameObject.Find("Doors").transform.GetChild(i).gameObject.SetActive(false);
        }
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
                for(int i = 0; i < GameObject.Find("Doors").transform.childCount; i++)
                {
                    GameObject.Find("Doors").transform.GetChild(i).gameObject.SetActive(true);
                }
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
        GameObject.Find("TurnBackSign").GetComponent<Canvas>().enabled = false;
        
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

        GameObject.Find("TurnBackSign").GetComponent<Canvas>().enabled = true;
    }

    public override void SimulatedWalkerUpdate()
    {
        // Act is if there's some dummy target a meter away from you requiring you to rotate
        //redirectionManager.simulatedWalker.RotateIfNecessary(180 - overallInjectedRotation, Vector3.forward);
        redirectionManager.simulatedWalker.RotateInPlace();
        //print("overallInjectedRotation: " + overallInjectedRotation);
    }

}
