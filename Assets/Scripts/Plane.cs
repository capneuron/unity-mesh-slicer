using UnityEngine;

namespace Slicing
{
    public enum LineXPlaneType
    {
        None, One, V1OnPlane, V2OnPlane, TwoOnPlane, 
    }
    public struct LineXPlaneResult
    {
        public LineXPlaneType type;
        public Vector3[] intersectionPoints;
        public float[] k;
        public LineXPlaneResult(LineXPlaneType type, Vector3[] intersectionPoints, float[] k)
        {
            this.type = type;
            this.intersectionPoints = intersectionPoints;
            this.k = k;
        }
        static public TriXPlaneType TriXPaneIntersection(LineXPlaneResult rab, LineXPlaneResult rac, LineXPlaneResult rbc)
        {
            if (rab.type == LineXPlaneType.None &&
                rac.type == LineXPlaneType.None &&
                rbc.type == LineXPlaneType.None)
                return TriXPlaneType.NoIntersection;
            //-----
            if (rab.type == LineXPlaneType.None &&
                rac.type == LineXPlaneType.One &&
                rbc.type == LineXPlaneType.One)
                return TriXPlaneType.Cross;
            
            if (rab.type == LineXPlaneType.One &&
                rac.type == LineXPlaneType.None &&
                rbc.type == LineXPlaneType.One)
                return TriXPlaneType.Cross;
            
            if (rab.type == LineXPlaneType.One &&
                rac.type == LineXPlaneType.One &&
                rbc.type == LineXPlaneType.None)
                return TriXPlaneType.Cross;
            //----------- uncommon situation ↓↓↓↓ ------------
            if (rab.type == LineXPlaneType.TwoOnPlane &&
                rac.type == LineXPlaneType.TwoOnPlane &&
                rbc.type == LineXPlaneType.TwoOnPlane)
                return TriXPlaneType.Three;
            
            if ((rab.type == LineXPlaneType.TwoOnPlane)||
                (rac.type == LineXPlaneType.TwoOnPlane) ||
                rbc.type == LineXPlaneType.TwoOnPlane)
                return TriXPlaneType.Two;
            
            //-----
            if ((rab.type == LineXPlaneType.V1OnPlane)&&
                (rac.type == LineXPlaneType.V1OnPlane) &&
                rbc.type == LineXPlaneType.None)
                return TriXPlaneType.One;
            
            if ((rab.type == LineXPlaneType.V1OnPlane)&&
                (rac.type == LineXPlaneType.V1OnPlane) &&
                rbc.type == LineXPlaneType.One)
                return TriXPlaneType.OneAndCross;
            // --
            if ((rab.type == LineXPlaneType.V2OnPlane)&&
                (rac.type == LineXPlaneType.None) &&
                rbc.type == LineXPlaneType.V1OnPlane)
                return TriXPlaneType.One;
            
            if ((rab.type == LineXPlaneType.V2OnPlane)&&
                (rac.type == LineXPlaneType.One) &&
                rbc.type == LineXPlaneType.V1OnPlane)
                return TriXPlaneType.OneAndCross;
            //--
            if ((rab.type == LineXPlaneType.None)&&
                (rac.type == LineXPlaneType.V2OnPlane) &&
                rbc.type == LineXPlaneType.V2OnPlane)
                return TriXPlaneType.One;
            
            if ((rab.type == LineXPlaneType.One)&&
                (rac.type == LineXPlaneType.V2OnPlane) &&
                rbc.type == LineXPlaneType.V2OnPlane)
                return TriXPlaneType.OneAndCross;
            return TriXPlaneType.NoIntersection;
        }
    }
    
    public enum TriXPlaneType
    {
        Cross, OneAndCross, One, Two, Three, NoIntersection
    }
    
    // public struct TriXPlaneResult
    // {
    //     public TriXPlaneType type;
    //     public Vector3[] intersectionPoints;
    //     public float[] k;
    //     public LineXPlaneResult(LineXPlaneType type, Vector3[] intersectionPoints, float[] k)
    //     {
    //         this.type = type;
    //         this.intersectionPoints = intersectionPoints;
    //         this.k = k;
    //     }
    // }
    
    public class Plane
    {
        public Vector3 normal, point;

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            normal = Vector3.Cross(b - a, c - a).normalized;
            point = a;
        }
        public Plane(Vector3 normal, Vector3 point)
        {
            this.normal = normal;
            this.point = point;
        }
        
        //find the intersection point
        public LineXPlaneResult Intersection(Vector3 a, Vector3 b)
        {
            var apXn = Vector3.Dot((a - point), normal);
            var bpXn = Vector3.Dot((b - point), normal);
            var apnXbpn = apXn * bpXn;
            // two points are on the plane
            if (Mathf.Abs(apXn) < Mathf.Epsilon && Mathf.Abs(bpXn) < Mathf.Epsilon) //TODO float == 0?
            {
                return new LineXPlaneResult(LineXPlaneType.TwoOnPlane,
                    new Vector3[] {a, b},
                    new float[] {0, 0});
            }
            
            // one point are on the plane
            if (Mathf.Abs(apXn) < Mathf.Epsilon) 
            {
                return new LineXPlaneResult(LineXPlaneType.V1OnPlane,
                    new Vector3[] {a},
                    new float[] {0});
            }
            if (Mathf.Abs(bpXn) < Mathf.Epsilon) 
            {
                return new LineXPlaneResult(LineXPlaneType.V2OnPlane,
                    new Vector3[] {b},
                    new float[] {0});
            }
            
            // no intersection point
            if (apnXbpn > 0)
            {
                return new LineXPlaneResult(LineXPlaneType.None,
                    null, null);
            }
            
            // one intersection point
            float k = Vector3.Dot((point - a), normal) / Vector3.Dot((b - a), normal);
            k = Mathf.Clamp01(k); //TODO ?
            
            Vector3 intersectionPoint = a + k*(b - a);
            return new LineXPlaneResult(LineXPlaneType.One,
                new Vector3[] {intersectionPoint},
                new float[] {k});
        }

        public int sideOf(Vector3 point)
        {
            var value = Vector3.Dot((point - this.point), this.normal);
            if (Mathf.Abs(value) < Mathf.Epsilon)
                return 0;
            if(value > 0)
                return 1;
            return -1;
        }
    }
}
