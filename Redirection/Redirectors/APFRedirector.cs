using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redirection;

public class APFRedirector : Redirector {

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

    // Realspace
    private List<Vector2> vertices;
    private List<Vector2> verticesW;
    private List<Vector2> verticesT;
    private List<Vector2> verticesS;
    private List<Vector2> segmentedVertices;
    private List<Vector2> segmentNormalVectors;
    private List<float> segmentedEdgeLengths;
    private float segNo = 50f;

    public APFRedirector()
    {
        SetVertices();
        SetSegmentedVertices(this.verticesS); // choose realspace type
        SetSegmentNormalVectors(this.verticesS);
        SetSegmentedEdgeLengths(this.verticesS);
    }

    private void SetVertices()
    {
        List<Vector2> verticesW = new List<Vector2>();
        List<Vector2> verticesT = new List<Vector2>();
        List<Vector2> verticesS = new List<Vector2>();

        // Western Room
        verticesW.Add(new Vector2(-2f, 1.5f));
        verticesW.Add(new Vector2(-0.5f, 1.5f));
        verticesW.Add(new Vector2(-0.5f, 2f));
        verticesW.Add(new Vector2(2f, 2f));
        verticesW.Add(new Vector2(2f, -2f));
        verticesW.Add(new Vector2(1f, -2f));
        verticesW.Add(new Vector2(1f, 0f));
        verticesW.Add(new Vector2(-0.5f, 0f));
        verticesW.Add(new Vector2(-0.5f, -2f));
        verticesW.Add(new Vector2(-1.5f, -2f));
        verticesW.Add(new Vector2(-1.5f, -0.25f));
        verticesW.Add(new Vector2(-2f, -0.25f));

        // Short T Type
        verticesT.Add(new Vector2(-2f, 2f/3f));
        verticesT.Add(new Vector2(2f, 2f/3f));
        verticesT.Add(new Vector2(2f, -2f/3f));
        verticesT.Add(new Vector2(2f/3f, -2f/3f));
        verticesT.Add(new Vector2(2f/3f, -2f));
        verticesT.Add(new Vector2(-2f/3f, -2f));
        verticesT.Add(new Vector2(-2f/3f, -2f/3f));
        verticesT.Add(new Vector2(-2f, -2f/3f));

        // Square Type
        verticesS.Add(new Vector2(2f, 2f));
        verticesS.Add(new Vector2(2f, -2f));
        verticesS.Add(new Vector2(-2f, -2f));
        verticesS.Add(new Vector2(-2f, 2f));

        this.verticesW = verticesW;
        this.verticesT = verticesT;
        this.verticesS = verticesS;
    }

    private void SetSegmentedVertices(List<Vector2> vertices)
    {
        float segNo = this.segNo;
        this.vertices = vertices; // Choose real space type here
        List<Vector2> segmentedVertices = new List<Vector2>();
        for(int i = 0 ; i < vertices.Count; i++)
        {
            if(vertices.Count <= i+1)
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    float jFloat = (float) j;
                    segmentedVertices.Add((vertices[0] - vertices[vertices.Count-1])*jFloat/segNo + vertices[vertices.Count-1] - (vertices[0] - vertices[vertices.Count-1])/(2*segNo));
                }
            }
            else
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    float jFloat = (float) j;
                    segmentedVertices.Add((vertices[i+1] - vertices[i])*jFloat/segNo + vertices[i] - (vertices[i+1] - vertices[i])/(2*segNo));
                }
            }
        }

        this.segmentedVertices = segmentedVertices;
    }
    private void SetSegmentNormalVectors(List<Vector2> vertices)
    {
        float segNo = this.segNo;
        this.vertices = vertices; // Choose real space type here
        List<Vector2> segmentNormalVectors = new List<Vector2>();
        for(int i = 0 ; i < vertices.Count; i++)
        {
            if(vertices.Count <= i+1)
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    segmentNormalVectors.Add(  RotateVector2( ( (vertices[0] - vertices[vertices.Count-1])/segNo ).normalized, -90)   );
                }
            }
            else
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    segmentNormalVectors.Add(  RotateVector2( ( (vertices[i+1] - vertices[i])/segNo ).normalized, -90)   );
                }
            }
        }

        this.segmentNormalVectors = segmentNormalVectors;
    }
    private void SetSegmentedEdgeLengths(List<Vector2> vertices)
    {
        float segNo = this.segNo;
        this.vertices = vertices; // Choose real space type here
        List<float> segmentedEdgeLengths = new List<float>();
        for(int i = 0 ; i < vertices.Count; i++)
        {
            if(vertices.Count <= i+1)
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    segmentedEdgeLengths.Add( ((vertices[0] - vertices[vertices.Count-1])/segNo).magnitude);
                }
            }
            else
            {
                for(int j = 1; j <= segNo; j++ )
                {
                    segmentedEdgeLengths.Add( ((vertices[i+1] - vertices[i])/segNo).magnitude);
                }
            }
        }

        this.segmentedEdgeLengths = segmentedEdgeLengths;
    }

    public override void ApplyRedirection()
    {
        (Vector2 w, float t) = GetWandT();

        currentTarget = redirectionManager.trackedSpace;
        w = RotateVector2(w,-currentTarget.rotation.eulerAngles.y);
        //Debug.Log("w:"+10000*w);
        //Vector3 currentTargetPosition = new Vector3(redirectionManager.currPos.x + w.x, redirectionManager.currPos.y, redirectionManager.currPos.z + w.y);
        
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

        List<Vector2> segmentedVertices = this.segmentedVertices;
        List<Vector2> segmentNormalVectors = this.segmentNormalVectors;
        List<float> segmentedEdgeLengths = this.segmentedEdgeLengths;

        List<Vector2> dList = new List<Vector2>();
        List<float> dListMagnitude = new List<float>();
        List<Vector2> dNormalizedList = new List<Vector2>();
        List<float> inverseDList = new List<float>();


        for(int i=0; i < segmentedVertices.Count; i++)
        {
            dList.Add(userPosition - segmentedVertices[i]);
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
        for(int i=0; i < segmentedVertices.Count; i++)
        {
            if(Vector2.Dot(segmentNormalVectors[i], dNormalizedList[i]) > 0)
            {
                w += C*segmentedEdgeLengths[i]*dNormalizedList[i]*inverseDList[i];
            }
            else
            {
                ;// Do Nothing
            }
        }

        return (w, 1 - dListMagnitude.Min()*Mathf.Abs(1/r));
    }
    public static Vector3 CastVector2Dto3D(Vector2 vec2, float height = 0) {
        int significantDigit = 5;
        float significant = Mathf.Pow(10, significantDigit);

        float xValue = Mathf.Floor(vec2.x * significant) / significant;
        float yValue = Mathf.Floor(vec2.y * significant) / significant;

        return new Vector3(xValue, height, yValue);
    }
    public static Vector2 CastVector3Dto2D(Vector3 vec3) {
        int significantDigit = 5; 
        float significant = Mathf.Pow(10, significantDigit);

        float xValue = Mathf.Floor(vec3.x * significant) / significant; // 0.6666667 이면 0.666666 으로 버림
        float zValue = Mathf.Floor(vec3.z * significant) / significant;

        return new Vector2(xValue, zValue);
    }

    public static Quaternion CastRotation2Dto3D(float degree)
    {
        return Quaternion.Euler(0, -degree, 0);
    }

    public static Vector2 RotateVector2(Vector2 vec, float degree)  // 시계 반대방향으로 회전함.
    {
        Vector2 rotated = CastVector3Dto2D(CastRotation2Dto3D(degree) * CastVector2Dto3D(vec));
        return rotated;
    }
}
