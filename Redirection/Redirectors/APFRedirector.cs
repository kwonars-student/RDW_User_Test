using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redirection;

public class APFRedirector : Redirector {
    GameObject lineObject;
    LineRenderer LineRenderer;

    // HODGSON_MIN(MAX)_CURVATURE_GAIN = 1 / redirectionManager.CURVATURE_RADIUS
    // MIN_ROTATION_GAIN = redirectionManager.MIN_ROT_GAIN
    // MAX_ROTATION_GAIN = redirectionManager.MAX_ROT_GAIN

    // Testing Parameters
    bool useBearingThresholdBasedRotationDampeningTimofey = true;
    bool dontUseDampening = false;

    // User Experience Improvement Parameters
    private const float MOVEMENT_THRESHOLD = 0.2f; // meters per second
    private const float CURVATURE_GAIN_CAP_DEGREES_PER_SECOND = 15;  // degrees per second
    private const float ROTATION_THRESHOLD = 1.5f; // degrees per second
    private const float ROTATION_GAIN_CAP_DEGREES_PER_SECOND = 30;  // degrees per second
    private const float BEARING_THRESHOLD_FOR_DAMPENING = 45f; // TIMOFEY: 45.0f; // Bearing threshold to apply dampening (degrees) MAHDI: WHERE DID THIS VALUE COME FROM?
    private const float DISTANCE_THRESHOLD_FOR_DAMPENING = 1.25f; // Distance threshold to apply dampening (meters)
    private const float SMOOTHING_FACTOR = 0.125f; // Smoothing factor for redirection rotations

    // Reference Parameters
    protected Transform currentTarget; //Where the participant  is currently directed?
    protected GameObject tmpTarget;

    // State Parameters
    public bool noTmpTarget = true;

    // Auxiliary Parameters
    private float rotationFromCurvatureGain; //Proposed curvature gain based on user speed
    private float rotationFromRotationGain; //Proposed rotation gain based on head's yaw
    private float lastRotationApplied = 0f;

    bool lineInitialized=false;

    public override void ApplyRedirection()
    {
        (Vector2 w, float t) = GetWandT();

        //Debug.DrawRay (redirectionManager.currPos, new Vector3(10000*w.x, 1f ,10000*w.y), Color.green);
        //Debug.Log("redirectionManager.currPos: "+redirectionManager.currPos);

        currentTarget = redirectionManager.trackedSpace;
        w = Utilities.RotateVector2(w,-currentTarget.rotation.eulerAngles.y);
        //Debug.Log("w:"+10000*w);
        //Vector3 currentTargetPosition = new Vector3(redirectionManager.currPos.x + w.x, redirectionManager.currPos.y, redirectionManager.currPos.z + w.y);
        

        if(!lineInitialized && redirectionManager.simulationManager.runInSimulationMode)
        {
            lineObject = new GameObject("Line");
            LineRenderer = lineObject.AddComponent<LineRenderer>();
            LineRenderer.startWidth = 0.2f;
            LineRenderer.endWidth = 0f;

            lineInitialized = true;
        }
        
        if(lineInitialized)
        {
            LineRenderer.SetPosition(0, new Vector3(redirectionManager.currPos.x, 0.5f ,redirectionManager.currPos.z));
            LineRenderer.SetPosition(1, new Vector3(10*w.x+redirectionManager.currPos.x, 0.5f ,10f*w.y+redirectionManager.currPos.z));
        }
        


        // Get Required Data
        Vector3 deltaPos = redirectionManager.deltaPos;
        float deltaDir = redirectionManager.deltaDir;

        rotationFromCurvatureGain = 0;

        
        if (deltaPos.magnitude / redirectionManager.GetDeltaTime() > MOVEMENT_THRESHOLD) //User is moving
        {
            rotationFromCurvatureGain = Mathf.Rad2Deg * (deltaPos.magnitude / redirectionManager.CURVATURE_RADIUS);

            //float t = 1 - dListMagnitude.Min()*Mathf.Abs( 1 / redirectionManager.CURVATURE_RADIUS );
            //Debug.Log("t: "+ t);
            
            rotationFromCurvatureGain = (1-t)*rotationFromCurvatureGain + t*CURVATURE_GAIN_CAP_DEGREES_PER_SECOND;
            rotationFromCurvatureGain = Mathf.Min(rotationFromCurvatureGain, CURVATURE_GAIN_CAP_DEGREES_PER_SECOND * redirectionManager.GetDeltaTime());
            // rotationFromCurvatureGain = CURVATURE_GAIN_CAP_DEGREES_PER_SECOND;
        }

        //Compute desired facing vector for redirection
        Vector3 desiredFacingDirection = new Vector3(10000*w.x, 0f, 10000*w.y);//Utilities.FlattenedPos3D(currentTarget.position) - redirectionManager.currPos;
        //Debug.Log("redirectionManager.currDir"+redirectionManager.currDir);
        int desiredSteeringDirection = (-1) * (int)Mathf.Sign(Utilities.GetSignedAngle(redirectionManager.currDir, desiredFacingDirection)); // We have to steer to the opposite direction so when the user counters this steering, she steers in right direction

        //Compute proposed rotation gain
        rotationFromRotationGain = 0;

        if (Mathf.Abs(deltaDir) / redirectionManager.GetDeltaTime() >= ROTATION_THRESHOLD)  //if User is rotating
        {
            
            //Determine if we need to rotate with or against the user
            if (deltaDir * desiredSteeringDirection < 0)
            {
                //Rotating against the user
                rotationFromRotationGain = Mathf.Min(Mathf.Abs(deltaDir * redirectionManager.MIN_ROT_GAIN), ROTATION_GAIN_CAP_DEGREES_PER_SECOND * redirectionManager.GetDeltaTime());
            }
            else
            {
                //Rotating with the user
                rotationFromRotationGain = Mathf.Min(Mathf.Abs(deltaDir * redirectionManager.MAX_ROT_GAIN), ROTATION_GAIN_CAP_DEGREES_PER_SECOND * redirectionManager.GetDeltaTime());
            }
        }

        float rotationProposed = desiredSteeringDirection * Mathf.Max(rotationFromRotationGain, rotationFromCurvatureGain);
        bool curvatureGainUsed = rotationFromCurvatureGain > rotationFromRotationGain;


        // Prevent having gains if user is stationary
        if (Mathf.Approximately(rotationProposed, 0))
            return;

        if (!dontUseDampening)
        {
            //DAMPENING METHODS
            // MAHDI: Sinusiodally scaling the rotation when the bearing is near zero
            float bearingToTarget = Vector3.Angle(redirectionManager.currDir, desiredFacingDirection);
            if (useBearingThresholdBasedRotationDampeningTimofey)
            {
                // TIMOFEY
                if (bearingToTarget <= BEARING_THRESHOLD_FOR_DAMPENING)
                    rotationProposed *= Mathf.Sin(Mathf.Deg2Rad * 90 * bearingToTarget / BEARING_THRESHOLD_FOR_DAMPENING);
            }
            else
            {
                // MAHDI
                // The algorithm first is explained to be similar to above but at the end it is explained like this. Also the BEARING_THRESHOLD_FOR_DAMPENING value was never mentioned which make me want to use the following even more.
                rotationProposed *= Mathf.Sin(Mathf.Deg2Rad * bearingToTarget);
            }


            // MAHDI: Linearly scaling the rotation when the distance is near zero
            if (desiredFacingDirection.magnitude <= DISTANCE_THRESHOLD_FOR_DAMPENING)
            {
                rotationProposed *= desiredFacingDirection.magnitude / DISTANCE_THRESHOLD_FOR_DAMPENING;
            }

        }

        // Implement additional rotation with smoothing
        float finalRotation = (1.0f - SMOOTHING_FACTOR) * lastRotationApplied + SMOOTHING_FACTOR * rotationProposed;
        // Debug.Log("finalRotation: "+finalRotation);
        lastRotationApplied = finalRotation;
        if (!curvatureGainUsed)
            InjectRotation(finalRotation);
        else
            InjectCurvature(finalRotation);
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
}
