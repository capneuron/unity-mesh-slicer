using UnityEngine;

namespace Slicing
{
    public class Plane
    {
        public Vector3 normal, point;
        public Vector3 a, b, c;

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = a;
            this.c = a;
            
            normal = Vector3.Cross(b - a, c - a).normalized;
            point = a;
        }
        public Plane(Vector3 normal, Vector3 point)
        {
            this.normal = normal;
            this.point = point;
        }
        
        public bool Intersection(Vector3 a, Vector3 b, out float k, out Vector3 intersectionPoint)
        {
            intersectionPoint = Vector3.zero;
            k = 0;
            if (Vector3.Dot((a - point), normal) * Vector3.Dot((b - point), normal) >= 0) //TODO: == 0?
            {
                return false;
            }
            k = Vector3.Dot((point - a), normal) / Vector3.Dot((b - a), normal);

            k = Mathf.Clamp01(k); //TODO ?
            
            intersectionPoint = a + k*(b - a);
            return true;
        }

        public int sideOf(Vector3 point)
        {
            if (Vector3.Dot((point - this.point), this.normal) >= 0)
                return 0;
            return 1;
        }
    }
}
