using System.Collections.Generic;
using UnityEngine;

namespace Slicing
{
    public class Slicer
    {
        public Slicer()
        {
            
        }
        
        
        public bool Slice(GameObject obj, Plane plane, out GameObject part1, out GameObject part2)
        {
            part1 = null;
            part2 = null;
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf == null) return false;

            bool intersect = false;
            
            var worldToLocalMatrix = obj.transform.worldToLocalMatrix;
            //change the plane to the obj's base
            plane.point = worldToLocalMatrix.MultiplyPoint(plane.point);
            plane.normal = worldToLocalMatrix.MultiplyVector(plane.normal).normalized;
            Debug.Log("point: "+plane.point);
            Debug.Log("normal: "+plane.normal);
            // iterate triangles
            Mesh mesh = mf.mesh;
            List<MeshVertex> newMeshVertices = MeshVertex.ReadFromMesh(mesh);
            var newTriangles = new List<int>();
            
            for (int vi = 0; vi < mesh.triangles.Length; vi += 3)
            {
                var ai = mesh.triangles[vi];
                var bi = mesh.triangles[vi + 1];
                var ci = mesh.triangles[vi + 2];

                Vector3 a = mesh.vertices[ai];
                Vector3 b = mesh.vertices[bi];
                Vector3 c = mesh.vertices[ci];
                
                float kab, kac, kbc;
                var iab = plane.Intersection(a, b, out kab, out var ipab);
                var iac = plane.Intersection(a, c, out kac, out var ipac);
                var ibc = plane.Intersection(b, c, out kbc, out var ipbc);

                //choose a tip and two other vertices as two points the the edge
                //                ^   a
                //              /   \
                //        -------------------
                //           /        \
                //       c /___________\ b
                int tipi = default, e0i = default, e1i = default; //tip, edge point0, edge point1
                Vector2 uv0 = default, uv1 = default; // uv for the intersection points
                Vector3 i0 = default, i1 = default; // intersection points
                Vector3 n0 = default, n1 = default; // normals of intersection points
                
                // deciding which point of (a,b,c) is the tip.
                // calculating uv = a + k(b-a)
                
                //copy all old vertices, then add intersection points as new vertices
                //make a new triangles list
                
                if (iab==IntersectionResult.One && iac==IntersectionResult.One)
                {
                    tipi = ai; 
                    e0i = bi; i0 = ipab; 
                    uv0 = mesh.uv[ai] + kab*(mesh.uv[bi]-mesh.uv[ai]);
                    n0 = (mesh.normals[ai] + mesh.normals[bi]).normalized;
                    
                    e1i = ci;  i1 = ipac; 
                    uv1 = mesh.uv[ai] + kac*(mesh.uv[ci]-mesh.uv[ai]);
                    n1 = (mesh.normals[ai] + mesh.normals[ci]).normalized;
                }else if (iab==IntersectionResult.One && ibc==IntersectionResult.One)
                {
                    tipi = bi; 
                    e0i = ci; i0 = ipbc; uv0 = mesh.uv[bi] + kbc*(mesh.uv[ci]-mesh.uv[bi]);
                    n0 = (mesh.normals[ci] + mesh.normals[bi]).normalized;
                    
                    e1i = ai; i1 = ipab; uv1 = mesh.uv[ai] + kab*(mesh.uv[bi]-mesh.uv[ai]);
                    n1 = (mesh.normals[ai] + mesh.normals[bi]).normalized;
                }else if (iac==IntersectionResult.One && ibc==IntersectionResult.One)
                {
                    tipi = ci;
                    e0i = ai; i0 = ipac; uv0 = mesh.uv[ai] + kac*(mesh.uv[ci]-mesh.uv[ai]);
                    n0 = (mesh.normals[ai] + mesh.normals[ci]).normalized;
                    
                    e1i = bi; i1 = ipbc; uv1 = mesh.uv[bi] + kbc*(mesh.uv[ci]-mesh.uv[bi]);
                    n1 = (mesh.normals[ci] + mesh.normals[bi]).normalized;
                }
                else if(iab==IntersectionResult.Zero && iac==IntersectionResult.Zero && ibc==IntersectionResult.Zero)
                {
                    //no intersection
                    newTriangles.Add(ai);
                    newTriangles.Add(bi);
                    newTriangles.Add(ci);
                    continue;
                }

                intersect = true;
                //adding new triangles and vertices
                var i0i = newMeshVertices.Count;
                var i1i = i0i+1;
                newMeshVertices.Add(new MeshVertex(i0, uv0,n0));
                newMeshVertices.Add(new MeshVertex(i1, uv1,n1));
                
                newTriangles.Add(tipi);
                newTriangles.Add(i0i);
                newTriangles.Add(i1i);
                
                newTriangles.Add(i0i);
                newTriangles.Add(e0i);
                newTriangles.Add(i1i);
                
                newTriangles.Add(e0i);
                newTriangles.Add(e1i);
                newTriangles.Add(i1i);
            }

            if (!intersect) return false;
            //split into part1 and part2
            var ve1 = new List<MeshVertex>();
            var oldNewIdxDict1 = new Dictionary<int, int>();
            var tri1 = new List<int>();
            
            var ve2 = new List<MeshVertex>();
            var oldNewIdxDict2 = new Dictionary<int, int>();
            var tri2 = new List<int>();
            
            //split old vertices
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                Vector3 vertex = newMeshVertices[i].vertex;
                var side = plane.sideOf(vertex);
                if (side == 1)
                {
                    oldNewIdxDict1[i] = ve1.Count;
                    ve1.Add(newMeshVertices[i]);
                }
                else if(side == -1)
                {
                    oldNewIdxDict2[i] = ve2.Count;
                    ve2.Add(newMeshVertices[i]);
                }
                else //side == 0, on the plane
                {
                    oldNewIdxDict1[i] = ve1.Count;
                    ve1.Add(newMeshVertices[i]);
                    oldNewIdxDict2[i] = ve2.Count;
                    ve2.Add(newMeshVertices[i]);
                }
            }
            
            //split new vertices (shared by two part)
            for (int i = mesh.vertices.Length; i < newMeshVertices.Count; i++)
            {
                oldNewIdxDict1[i] = ve1.Count;
                ve1.Add(newMeshVertices[i]);
                oldNewIdxDict2[i] = ve2.Count;
                ve2.Add(newMeshVertices[i]);
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
                if (oldNewIdxDict2.ContainsKey(idx0) && oldNewIdxDict2.ContainsKey(idx1) && oldNewIdxDict2.ContainsKey(idx2))
                {
                    tri2.Add(oldNewIdxDict2[idx0]);
                    tri2.Add(oldNewIdxDict2[idx1]);
                    tri2.Add(oldNewIdxDict2[idx2]);
                }
            }
            obj.SetActive(false);
            part1 = CreateMesh(ve1, tri1.ToArray(), obj);
            part2 = CreateMesh(ve2, tri2.ToArray(), obj);
            CopyComponents(obj, part1, part2);
            
            // part1 = CreateMeshFromOrigin(ve1, tri1.ToArray(), obj);
            // part2 = CreateMeshFromOrigin(ve2, tri2.ToArray(), obj);
            
            part1.SetActive(true);
            part2.SetActive(true);
            part1.name = obj.name + "1";
            part2.name = obj.name + "2";
            return true;
        }
        
        public GameObject CreateMeshFromOrigin(List<MeshVertex> vertices, int[] triangles, GameObject origin)
        {
            GameObject obj = GameObject.Instantiate(origin);

            MeshFilter mf = obj.GetComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            MeshVertex.WriteToMesh(ref mesh, vertices);
            mesh.SetTriangles(triangles, 0);
            mf.sharedMesh = mesh;
            
            return obj;
        }
        public GameObject CreateMesh(List<MeshVertex> vertices, int[] triangles, GameObject origin)
        {
            GameObject obj = new GameObject();
            var transform = origin.transform;
            if (transform != null)
            {
                obj.transform.SetPositionAndRotation(transform.position, transform.rotation);
                obj.transform.localScale = transform.localScale;
            }
            
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            MeshVertex.WriteToMesh(ref mesh, vertices);
            mesh.SetTriangles(triangles, 0);
            mf.sharedMesh = mesh;
            CopyMeshMaterial(origin, obj);
            return obj;
        }

        private static void CopyComponents(GameObject obj, GameObject part1, GameObject part2)
        {
            if (obj.TryGetComponent(out Rigidbody rb))
            {
                var rb1 = part1.AddComponent<Rigidbody>();
                var rb2 = part2.AddComponent<Rigidbody>();
                rb1.mass = rb.mass/2; //TODO :D
                rb2.mass = rb1.mass;
            }
            
            if (obj.TryGetComponent(out Collider collider))
            {
                var col1 = part1.AddComponent<MeshCollider>();
                col1.convex = true;
                var col2 = part2.AddComponent<MeshCollider>();
                col2.convex = true;
            }
        }
        
        public static void CopyMeshMaterial(GameObject from, GameObject to)
        {
            MeshRenderer oldMeshRenderer = from.GetComponent<MeshRenderer>();
            MeshRenderer newMeshRenderer;
            if(! to.TryGetComponent<MeshRenderer>(out newMeshRenderer))
            {
                newMeshRenderer = to.AddComponent<MeshRenderer>();
            }
            
            //copy material
            List<Material> materialList = new List<Material>();
            oldMeshRenderer.GetSharedMaterials(materialList);
            if (materialList.Count > 0)
            {
                newMeshRenderer.materials = new Material[materialList.Count];
                for (int j = 0; j < materialList.Count; j++)
                {
                    //set material
                    newMeshRenderer.materials[j].CopyPropertiesFromMaterial(materialList[j]);
                    //set shader
                    newMeshRenderer.materials[j].shader = materialList[j].shader;
                }
            }

        }
        public static void CopyMesh(GameObject obj)
        {
            Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
            Mesh newmesh = new Mesh();
            newmesh.vertices = mesh.vertices;
            newmesh.triangles = mesh.triangles;
            newmesh.uv = mesh.uv;
            newmesh.normals = mesh.normals;
            newmesh.colors = mesh.colors;
            newmesh.tangents = mesh.tangents;
            
            GameObject o = new GameObject();
            o.AddComponent<MeshFilter>().sharedMesh = newmesh;
            o.AddComponent<MeshRenderer>();
            CopyMeshMaterial(obj, o);

            o.transform.SetPositionAndRotation(obj.transform.position, obj.transform.rotation);
            o.transform.localScale = obj.transform.localScale;
            o.name = "new";
        }
    }
}

