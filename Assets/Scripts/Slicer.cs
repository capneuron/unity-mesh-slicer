using System.Collections.Generic;
using UnityEngine;

namespace Slicing
{
    public class Slicer
    {
        public Slicer()
        {
            
        }
        
        
        public bool Slice(GameObject obj, Plane plane)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf == null) return false;

            bool intersect = false;
            
            //change the plane to the obj's base
            // var v1 = obj.transform.worldToLocalMatrix.MultiplyPoint(plane.a);
            // var v2 = obj.transform.worldToLocalMatrix.MultiplyPoint(plane.b);
            // var v3 = obj.transform.worldToLocalMatrix.MultiplyPoint(plane.c);
            // plane.point = v2;
            // plane.normal = Vector3.Cross(v3 - v2, v1 - v2).normalized;
            plane.point = obj.transform.worldToLocalMatrix.MultiplyPoint(plane.point);
            plane.normal = obj.transform.worldToLocalMatrix.MultiplyPoint(plane.normal);
            
            // iterate triangles
            Mesh mesh = mf.mesh;
            var newVertices = new List<Vector3>(mesh.vertices);
            var newTriangles = new List<int>();
            
            for (int vi = 0; vi < mesh.triangles.Length; vi += 3)
            {
                var ai = mesh.triangles[vi];
                var bi = mesh.triangles[vi + 1];
                var ci = mesh.triangles[vi + 2];

                Vector3 a = mesh.vertices[ai];
                Vector3 b = mesh.vertices[bi];
                Vector3 c = mesh.vertices[ci];

                bool iab = plane.Intersection(a, b, out var ipab);
                bool iac = plane.Intersection(a, c, out var ipac);
                bool ibc = plane.Intersection(b, c, out var ipbc);

                int tipi, b0i, b1i;
                Vector3 i0, i1;
                if (iab && iac)
                {
                    tipi = ai; 
                    b0i = bi; i0 = ipab;
                    b1i = ci;  i1 = ipac;
                }else if (iab && ibc)
                {
                    tipi = bi; 
                    b0i = ci; i0 = ipbc;
                    b1i = ai; i1 = ipab;
                }else if (iac && ibc)
                {
                    tipi = ci;
                    b0i = ai; i0 = ipac;
                    b1i = bi; i1 = ipbc;
                }
                else
                {
                    //no intersection
                    newTriangles.Add(ai);
                    newTriangles.Add(bi);
                    newTriangles.Add(ci);
                    continue;
                }

                intersect = true;
                //adding new triangles and vertices
                var i0i = newVertices.Count;
                var i1i = i0i+1;
                newVertices.Add(i0);
                newVertices.Add(i1);
                
                newTriangles.Add(tipi);
                newTriangles.Add(i0i);
                newTriangles.Add(i1i);
                
                newTriangles.Add(i0i);
                newTriangles.Add(b0i);
                newTriangles.Add(i1i);
                
                newTriangles.Add(b0i);
                newTriangles.Add(b1i);
                newTriangles.Add(i1i);
            }

            if (!intersect) return false;
            //split
            var ve1 = new List<Vector3>();
            var oldNewIdxDict1 = new Dictionary<int, int>();
            var tri1 = new List<int>();
            
            var ve2 = new List<Vector3>();
            var oldNewIdxDict2 = new Dictionary<int, int>();
            var tri2 = new List<int>();
            
            //split old vertices
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                var side = plane.sideOf(newVertices[i]);
                if (side == 0)
                {
                    oldNewIdxDict1[i] = ve1.Count;
                    ve1.Add(newVertices[i]);
                }
                else
                {
                    oldNewIdxDict2[i] = ve2.Count;
                    ve2.Add(newVertices[i]);
                }
            }
            
            //split new vertices (shared by two part)
            for (int i = mesh.vertices.Length; i < newVertices.Count; i++)
            {
                oldNewIdxDict1[i] = ve1.Count;
                ve1.Add(newVertices[i]);
                oldNewIdxDict2[i] = ve2.Count;
                ve2.Add(newVertices[i]);
            }
            
            //split triangles
            for (int i = 0; i < newTriangles.Count; i+=3)
            {
                var idx0 = newTriangles[i];
                var idx1 = newTriangles[i + 1];
                var idx2 = newTriangles[i + 2];
                if (oldNewIdxDict1.ContainsKey(idx0) && oldNewIdxDict1.ContainsKey(idx1) && oldNewIdxDict1.ContainsKey(idx2))
                {
                    tri1.Add(oldNewIdxDict1[idx0]);
                    tri1.Add(oldNewIdxDict1[idx1]);
                    tri1.Add(oldNewIdxDict1[idx2]);
                }
                else
                {
                    tri2.Add(oldNewIdxDict2[idx0]);
                    tri2.Add(oldNewIdxDict2[idx1]);
                    tri2.Add(oldNewIdxDict2[idx2]);
                }
            }
            obj.SetActive(false);
            var o1 = CreateMesh(ve1.ToArray(), tri1.ToArray(), obj.transform); 
            var o2 = CreateMesh(ve2.ToArray(), tri2.ToArray(), obj.transform);
            return true;
        }
        
        
        public GameObject CreateMesh(Vector3[] vertices, int[] triangles, Transform transform=null)
        {
            GameObject obj = new GameObject();
            if (transform != null)
            {
                obj.transform.SetPositionAndRotation(transform.position, transform.rotation);
                obj.transform.localScale = transform.localScale;
            }
            
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mf.sharedMesh = mesh;
            return obj;
        }
    }
}

