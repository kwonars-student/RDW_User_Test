using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Redirection
{
    public class GeometryInfo{

        [HideInInspector]
        public enum SpaceShape { SquareType, TType, RoomType };
        public SpaceShape spaceShape;
        public static List<Vector2> vertices; // APF용.
        public static List<Vector2> segmentedVertices; // APF용.
        public static List<Vector2> segmentNormalVectors; // APF용.
        public static List<float> segmentedEdgeLengths; // APF용.
        public float segNo = 20f; // APF용.
        
        public static List<Vector2> middleVertices; // APF-R용.
        public static List<Vector2> edgeNormalVectors; // APF-R용.
        public static List<Vector2> resetLocations; // RFL용

        public GeometryInfo(SpaceShape spaceShape)
        {
            getVertices(spaceShape);
            getSegmentedVertices();
            getSegmentNormalVectors();
            getSegmentedEdgeLengths();
            getMiddleVertices();
            getEdgeNormalVectors();
        }

        public void getSpaceShape(SpaceShape spaceShape)
        {
            this.spaceShape = spaceShape;
        }

        public void getVertices(SpaceShape spaceShape)
        {
            List<Vector2> spaceVertices = new List<Vector2>();
            List<Vector2> spaceResetLocations = new List<Vector2>();

            // Western Room
            if(spaceShape == SpaceShape.RoomType)
            {
                spaceVertices.Add(new Vector2(-2f, 1.5f));
                spaceVertices.Add(new Vector2(-0.5f, 1.5f));
                spaceVertices.Add(new Vector2(-0.5f, 2f));
                spaceVertices.Add(new Vector2(2f, 2f));
                spaceVertices.Add(new Vector2(2f, -2f));
                spaceVertices.Add(new Vector2(1f, -2f));
                spaceVertices.Add(new Vector2(1f, 0f));
                spaceVertices.Add(new Vector2(-0.5f, 0f));
                spaceVertices.Add(new Vector2(-0.5f, -2f));
                spaceVertices.Add(new Vector2(-1.5f, -2f));
                spaceVertices.Add(new Vector2(-1.5f, -0.25f));
                spaceVertices.Add(new Vector2(-2f, -0.25f));

                spaceResetLocations.Add(new Vector2(1f, 2f));
                spaceResetLocations.Add(new Vector2(2f, 0f));
                spaceResetLocations.Add(new Vector2(-1f, -2f));
                spaceResetLocations.Add(new Vector2(-2f, 0f));
            }
            // Short T Type
            else if(spaceShape == SpaceShape.TType)
            {
                spaceVertices.Add(new Vector2(-2f, 2f/3f));
                spaceVertices.Add(new Vector2(2f, 2f/3f));
                spaceVertices.Add(new Vector2(2f, -2f/3f));
                spaceVertices.Add(new Vector2(2f/3f, -2f/3f));
                spaceVertices.Add(new Vector2(2f/3f, -2f));
                spaceVertices.Add(new Vector2(-2f/3f, -2f));
                spaceVertices.Add(new Vector2(-2f/3f, -2f/3f));
                spaceVertices.Add(new Vector2(-2f, -2f/3f));

                spaceResetLocations.Add(new Vector2(0f, 2f/3f));
                spaceResetLocations.Add(new Vector2(2f, 0f));
                spaceResetLocations.Add(new Vector2(0f, -2f));
                spaceResetLocations.Add(new Vector2(-2f, 0f));
            }
            // Square Type
            else if(spaceShape == SpaceShape.SquareType)
            {
                spaceVertices.Add(new Vector2(2f, 2f));
                spaceVertices.Add(new Vector2(2f, -2f));
                spaceVertices.Add(new Vector2(-2f, -2f));
                spaceVertices.Add(new Vector2(-2f, 2f));

                spaceResetLocations.Add(new Vector2(0f, 2f));
                spaceResetLocations.Add(new Vector2(2f, 0f));
                spaceResetLocations.Add(new Vector2(0f, -2f));
                spaceResetLocations.Add(new Vector2(-2f, 0f));
            }
            
            vertices = spaceVertices;
            resetLocations = spaceResetLocations;
        }

        private void getSegmentedVertices()
        {
            float segNo = this.segNo;
            List<Vector2> spaceSegmentedVertices = new List<Vector2>();
            for(int i = 0 ; i < vertices.Count; i++)
            {
                if(vertices.Count <= i+1)
                {
                    for(int j = 1; j <= segNo; j++ )
                    {
                        float jFloat = (float) j;
                        spaceSegmentedVertices.Add((vertices[0] - vertices[vertices.Count-1])*jFloat/segNo + vertices[vertices.Count-1] - (vertices[0] - vertices[vertices.Count-1])/(2*segNo));
                    }
                }
                else
                {
                    for(int j = 1; j <= segNo; j++ )
                    {
                        float jFloat = (float) j;
                        spaceSegmentedVertices.Add((vertices[i+1] - vertices[i])*jFloat/segNo + vertices[i] - (vertices[i+1] - vertices[i])/(2*segNo));
                    }
                }
            }

            segmentedVertices = spaceSegmentedVertices;
        }
        private void getSegmentNormalVectors()
        {
            float segNo = this.segNo;
            List<Vector2> spaceSegmentNormalVectors = new List<Vector2>();
            for(int i = 0 ; i < vertices.Count; i++)
            {
                if(vertices.Count <= i+1)
                {
                    for(int j = 1; j <= segNo; j++ )
                    {
                        spaceSegmentNormalVectors.Add(  Utilities.RotateVector2( ( (vertices[0] - vertices[vertices.Count-1])/segNo ).normalized, -90)   );
                    }
                }
                else
                {
                    for(int j = 1; j <= segNo; j++ )
                    {
                        spaceSegmentNormalVectors.Add(  Utilities.RotateVector2( ( (vertices[i+1] - vertices[i])/segNo ).normalized, -90)   );
                    }
                }
            }

            segmentNormalVectors = spaceSegmentNormalVectors;
        }
        private void getSegmentedEdgeLengths()
        {
            float segNo = this.segNo;
            List<float> spaceSegmentedEdgeLengths = new List<float>();
            for(int i = 0 ; i < vertices.Count; i++)
            {
                if(vertices.Count <= i+1)
                {
                    for(int j = 1; j <= segNo; j++ )
                    {
                        spaceSegmentedEdgeLengths.Add( ((vertices[0] - vertices[vertices.Count-1])/segNo).magnitude);
                    }
                }
                else
                {
                    for(int j = 1; j <= segNo; j++ )
                    {
                        spaceSegmentedEdgeLengths.Add( ((vertices[i+1] - vertices[i])/segNo).magnitude);
                    }
                }
            }

            segmentedEdgeLengths = spaceSegmentedEdgeLengths;
        }

        private void getMiddleVertices()
        {
            List<Vector2> spaceMiddleVertices = new List<Vector2>();
            for(int i = 0 ; i < vertices.Count; i++)
            {
                if(vertices.Count <= i+1)
                {
                    spaceMiddleVertices.Add((vertices[0] - vertices[vertices.Count-1])/2f + vertices[vertices.Count-1]);
                }
                else
                {
                    spaceMiddleVertices.Add((vertices[i+1] - vertices[i])/2f + vertices[i]);
                }
            }
            middleVertices = spaceMiddleVertices;
        }
        private void getEdgeNormalVectors()
        {
            List<Vector2> spaceEdgeNormalVectors = new List<Vector2>();
            for(int i = 0 ; i < vertices.Count; i++)
            {
                if(vertices.Count <= i+1)
                {
                    spaceEdgeNormalVectors.Add(  Utilities.RotateVector2( ( (vertices[0] - vertices[vertices.Count-1]) ).normalized, -90)   );
                }
                else
                {
                    spaceEdgeNormalVectors.Add(  Utilities.RotateVector2( ( (vertices[i+1] - vertices[i]) ).normalized, -90)   );
                }
            }
            edgeNormalVectors = spaceEdgeNormalVectors;
        }

    }
}