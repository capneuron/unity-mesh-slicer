using UnityEngine;

namespace Slicing
{
    public enum IntersectionResult { Zero, One, Two, } 
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
        public IntersectionResult Intersection(Vector3 a, Vector3 b, out float k, out Vector3 intersectionPoint)
        {
            intersectionPoint = Vector3.zero;
            k = 0;
            var apnXbpn = Vector3.Dot((a - point), normal) * Vector3.Dot((b - point), normal);
            if (apnXbpn > 0) //TODO: == 0?
            {
                return IntersectionResult.Zero;
            }
            if (apnXbpn == 0) // two points of the line are on the plane
            {
                k = 0.5f;
                intersectionPoint = a + k*(b - a);
                return IntersectionResult.Two;
            }
            else
            {
                k = Vector3.Dot((point - a), normal) / Vector3.Dot((b - a), normal);
                k = Mathf.Clamp01(k); //TODO ?
            }
            
            intersectionPoint = a + k*(b - a);
            return IntersectionResult.One;
        }

        public int sideOf(Vector3 point)
        {
            var value = Vector3.Dot((point - this.point), this.normal);
            if(value > 0)
                return 1;
            if (value == 0)
                return 0;
            return -1;
        }
    }
}
