using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Slicing
{
    public class Slicer
    {
        private float force;
        public Slicer(float force = 0f)
        {
            this.force = force;
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
            
            // iterate triangles
            Mesh mesh = mf.mesh;
            int oldVerticesSize = mesh.vertices.Length;
            List<MeshVertex> newMeshVertices = MeshVertex.ReadFromMesh(mesh);
            List<List<int>> newTrianglesList = new List<List<int>>(); //for two submesh
            
            //split into part1 and part2
            var ve1 = new List<MeshVertex>();
            var oldNewIdxDict1 = new Dictionary<int, int>();
            List<int>[] tri1 = new List<int>[]{new List<int>(), new List<int>()}; //for two submesh
            
            var ve2 = new List<MeshVertex>();
            var oldNewIdxDict2 = new Dictionary<int, int>();
            List<int>[] tri2 = new List<int>[]{new List<int>(), new List<int>()}; //for two submesh
            
            HashSet<int> crossSurfaceVerIdx = new HashSet<int>();
            
            //split old vertices: all new vertices are appended to the end of all old vertices
            for (int i = 0; i < oldVerticesSize; i++)
            {
                Vector3 vertex = newMeshVertices[i].vertex;
                var side = plane.sideOf(vertex);
                MeshVertex mv = newMeshVertices[i];
                if (side == 1)
                {
                    oldNewIdxDict1[i] = ve1.Count;
                    ve1.Add(mv);
                }
                else if(side == -1)
                {
                    oldNewIdxDict2[i] = ve2.Count;
                    ve2.Add(mv);
                }
                else //side == 0, on the plane
                {
                    oldNewIdxDict1[i] = ve1.Count;
                    ve1.Add(mv);
                    oldNewIdxDict2[i] = ve2.Count;
                    ve2.Add(mv);
                    crossSurfaceVerIdx.Add(i);
                }
            }
            
            //cutting
            for (int submesh = 0; submesh < 2 && submesh < mesh.subMeshCount; submesh++)
            {
                if (SliceBySubMeshTriangles(mesh, submesh, plane, newMeshVertices, out List<int> newTriangles))
                    intersect = true;
                
                //split new vertices (shared by two part)
                for (int i = oldVerticesSize; i < newMeshVertices.Count; i++)
                {
                    oldNewIdxDict1[i] = ve1.Count;
                    ve1.Add(newMeshVertices[i]);
                    oldNewIdxDict2[i] = ve2.Count;
                    ve2.Add(newMeshVertices[i]);
                    crossSurfaceVerIdx.Add(i);
                }
                oldVerticesSize = newMeshVertices.Count;
                
                
                //split triangles
                for (int i = 0; i < newTriangles.Count; i+=3)
                {
                    var idx0 = newTriangles[i];
                    var idx1 = newTriangles[i+1];
                    var idx2 = newTriangles[i+2];
                    if (oldNewIdxDict1.ContainsKey(idx0) && oldNewIdxDict1.ContainsKey(idx1) && oldNewIdxDict1.ContainsKey(idx2))
                    {
                        tri1[submesh].Add(oldNewIdxDict1[idx0]);
                        tri1[submesh].Add(oldNewIdxDict1[idx1]);
                        tri1[submesh].Add(oldNewIdxDict1[idx2]);
                    }
                    if (oldNewIdxDict2.ContainsKey(idx0) && oldNewIdxDict2.ContainsKey(idx1) && oldNewIdxDict2.ContainsKey(idx2))
                    {
                        tri2[submesh].Add(oldNewIdxDict2[idx0]);
                        tri2[submesh].Add(oldNewIdxDict2[idx1]);
                        tri2[submesh].Add(oldNewIdxDict2[idx2]);
                    }
                }
                newTrianglesList.Add(newTriangles);
            }
            if (!intersect) return false;
            
            //seal the surface
            int root = crossSurfaceVerIdx.ElementAt(0);
            var rootV1 = newMeshVertices[root].vertex;
            var rootV2 = newMeshVertices[root].vertex;
            
            int rootIdx1 = ve1.Count;
            ve1.Add(new MeshVertex(rootV1, Vector2.zero, plane.normal));
            
            int rootIdx2 = ve2.Count;
            ve2.Add(new MeshVertex(rootV2, Vector2.zero, -plane.normal));

            int c1=0, c2=0;
            for (int submesh = 0; submesh < 2 && submesh < mesh.subMeshCount; submesh++)
            {
                var newTriangles = newTrianglesList[submesh];
                for (int i = 0; i < newTriangles.Count; i+=3)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var idx0 = newTriangles[i+j];
                        var idx1 = newTriangles[i+(j+1)%3];
                        c1++;
                        if (crossSurfaceVerIdx.Contains(idx0) && crossSurfaceVerIdx.Contains(idx1) && idx0 != root &&
                            idx1 != root)
                        {
                            c2++;
                            tri1[1].Add(ve1.Count);
                            ve1.Add(new MeshVertex(newMeshVertices[idx1].vertex, Vector2.zero, plane.normal));
                            tri1[1].Add(ve1.Count);
                            ve1.Add(new MeshVertex(newMeshVertices[idx0].vertex, Vector2.zero, plane.normal));
                            tri1[1].Add(rootIdx1);
                            
                            tri2[1].Add(ve2.Count);
                            ve2.Add(new MeshVertex(newMeshVertices[idx1].vertex, Vector2.zero, -plane.normal));
                            tri2[1].Add(ve2.Count);
                            ve2.Add(new MeshVertex(newMeshVertices[idx0].vertex, Vector2.zero, -plane.normal));
                            tri2[1].Add(rootIdx2);
                        }
                    }
                }
            }

           
            Debug.Log(c1+","+c2);
            
            obj.SetActive(false);
            part1 = CreateMesh(ve1, tri1[0].ToArray(), tri1[1].ToArray(), obj);
            part2 = CreateMesh(ve2, tri2[0].ToArray(), tri2[1].ToArray(), obj);
            CopyComponents(obj, part1, part2);
            
            // part1 = CreateMeshFromOrigin(ve1, tri1.ToArray(), obj);
            // part2 = CreateMeshFromOrigin(ve2, tri2.ToArray(), obj);
            
            part1.SetActive(true);
            part2.SetActive(true);
            part1.name = obj.name + "1";
            part2.name = obj.name + "2";
            if (force > 0.0001f)
            {
                if(part1.TryGetComponent(out Rigidbody rgbd1))
                {
                    rgbd1.AddForce(-plane.normal * force * rgbd1.mass);
                }
                
                if(part2.TryGetComponent(out Rigidbody rgbd2))
                {
                    rgbd2.AddForce(plane.normal * force* rgbd2.mass);
                }
            }
            
            return true;
        }

        private bool SliceBySubMeshTriangles(Mesh mesh, int submesh, Plane plane, List<MeshVertex> newMeshVertices, 
           out List<int> newTriangles)
        {
            bool intersect = false;
            // iterate triangles
            newTriangles = new List<int>();
            var oldTriangles = mesh.GetTriangles(submesh);
            for (int vi = 0; vi < oldTriangles.Length; vi += 3)
            {
                var ai = oldTriangles[vi];
                var bi = oldTriangles[vi + 1];
                var ci = oldTriangles[vi + 2];

                Vector3 a = mesh.vertices[ai];
                Vector3 b = mesh.vertices[bi];
                Vector3 c = mesh.vertices[ci];
                
                float kab, kac, kbc;
                var rab = plane.Intersection(a, b);
                var rac = plane.Intersection(a, c);
                var rbc = plane.Intersection(b, c);

                // no intersection
                TriXPlaneType result = LineXPlaneResult.TriXPaneIntersection(rab, rac, rbc);
                if(result == TriXPlaneType.NoIntersection || result == TriXPlaneType.One || result == TriXPlaneType.Two || result == TriXPlaneType.Three)
                {
                    newTriangles.Add(ai);
                    newTriangles.Add(bi);
                    newTriangles.Add(ci);
                    continue;
                }

                if (result == TriXPlaneType.Cross)
                {
                    SliceTriTo3(mesh, new []{a,b,c}, new []{ai,bi,ci}, new []{rab,rac,rbc}, newMeshVertices, newTriangles);
                }
                else if (result == TriXPlaneType.OneAndCross)
                {
                    SliceTriTo2(mesh, new []{a,b,c}, new []{ai,bi,ci}, new []{rab,rac,rbc}, newMeshVertices, newTriangles);
                }
                intersect = true;
            }
            return intersect;
        }
        
        private void SliceTriTo3(Mesh mesh, Vector3[] abc, int[] abci, LineXPlaneResult[] rabc, List<MeshVertex> newMeshVertices, List<int> newTriangles)
        {
            Vector3 a = abc[0];
            Vector3 b = abc[1]; 
            Vector3 c = abc[2];
            
            int ai = abci[0];
            int bi = abci[1]; 
            int ci = abci[2]; 
            
            LineXPlaneResult rab = rabc[0];
            LineXPlaneResult rac = rabc[1]; 
            LineXPlaneResult rbc = rabc[2];

            //choose a tip and two other vertices as two points the the edge
            //                /\   a(tip)
            //              /   \
            //        -------------------
            //           /        \
            // (e0i) c /___________\ b (e1i)
            int tipi = default, e0i = default, e1i = default; //tip, edge point0, edge point1
            Vector3 i0 = default, i1 = default; // intersection points
            Vector2 uv0 = default, uv1 = default; // uv for the intersection points
            Vector3 n0 = default, n1 = default; // normals of intersection points
            
            // deciding which point of (a,b,c) is the tip.
            // calculating uv = a + k(b-a)
            
            //copy all old vertices, then add intersection points as new vertices
            //make a new triangles list
            
            if (rab.type==LineXPlaneType.One && rac.type==LineXPlaneType.One)
            {
                tipi = ai; 
                e0i = bi; i0 = rab.intersectionPoints[0]; 
                uv0 = mesh.uv[ai] + rab.k[0]*(mesh.uv[bi]-mesh.uv[ai]);
                n0 = (mesh.normals[ai] + mesh.normals[bi]).normalized;
                
                e1i = ci;  i1 = rac.intersectionPoints[0]; 
                uv1 = mesh.uv[ai] + rac.k[0]*(mesh.uv[ci]-mesh.uv[ai]);
                n1 = (mesh.normals[ai] + mesh.normals[ci]).normalized;
            }else if (rab.type==LineXPlaneType.One && rbc.type==LineXPlaneType.One)
            {
                tipi = bi; 
                e0i = ci; i0 = rbc.intersectionPoints[0]; uv0 = mesh.uv[bi] + rbc.k[0]*(mesh.uv[ci]-mesh.uv[bi]);
                n0 = (mesh.normals[ci] + mesh.normals[bi]).normalized;
                
                e1i = ai; i1 = rab.intersectionPoints[0]; uv1 = mesh.uv[ai] + rab.k[0]*(mesh.uv[bi]-mesh.uv[ai]);
                n1 = (mesh.normals[ai] + mesh.normals[bi]).normalized;
            }else if (rac.type==LineXPlaneType.One && rbc.type==LineXPlaneType.One)
            {
                tipi = ci;
                e0i = ai; i0 = rac.intersectionPoints[0]; uv0 = mesh.uv[ai] + rac.k[0]*(mesh.uv[ci]-mesh.uv[ai]);
                n0 = (mesh.normals[ai] + mesh.normals[ci]).normalized;
                
                e1i = bi; i1 = rbc.intersectionPoints[0]; uv1 = mesh.uv[bi] + rbc.k[0]*(mesh.uv[ci]-mesh.uv[bi]);
                n1 = (mesh.normals[ci] + mesh.normals[bi]).normalized;
            }
            else
            {
                Debug.LogError("Wrong type in SliceTo3()");
            }
            
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
        
        private void SliceTriTo2(Mesh mesh, Vector3[] abc, int[] abci, LineXPlaneResult[] rabc, List<MeshVertex> newMeshVertices, List<int> newTriangles)
        {
            Vector3 a = abc[0];
            Vector3 b = abc[1]; 
            Vector3 c = abc[2];
            
            int ai = abci[0];
            int bi = abci[1]; 
            int ci = abci[2]; 
            
            LineXPlaneResult rab = rabc[0];
            LineXPlaneResult rac = rabc[1]; 
            LineXPlaneResult rbc = rabc[2];

            //choose a tip and two other vertices as two points the the edge
            //                  /\
            //                / | \   a(tip)
            //              /  |   \
            //           /    |     \
            // (e0i) c /_____|_______\ b (e1i)
            int tipi = default, e0i = default, e1i = default; //tip, edge point0, edge point1
            Vector3 inter = default; // intersection points
            Vector2 uv = default; // uv for the intersection points
            Vector3 normal = default; // normals of intersection points
            
            // deciding which point of (a,b,c) is the tip.
            // calculating uv = a + k(b-a)
            
            //copy all old vertices, then add intersection points as new vertices
            //make a new triangles list
            
            if (rbc.type==LineXPlaneType.One) // a is the tip
            {
                tipi = ai; 
                inter = rbc.intersectionPoints[0]; uv = mesh.uv[bi] + rbc.k[0]*(mesh.uv[ci]-mesh.uv[bi]);
                normal = (mesh.normals[ci] + mesh.normals[bi]).normalized;
                
                e0i = bi; 
                e1i = ci; 
            }else if (rac.type==LineXPlaneType.One)
            {
                tipi = bi; 
                inter = rac.intersectionPoints[0]; uv = mesh.uv[ai] + rac.k[0]*(mesh.uv[ci]-mesh.uv[ai]);
                normal = (mesh.normals[ai] + mesh.normals[ci]).normalized;
                
                e0i = ci;
                e1i = ai;
            }else if (rab.type==LineXPlaneType.One)
            {
                tipi = ci;
                inter = rab.intersectionPoints[0]; uv = mesh.uv[ai] + rab.k[0]*(mesh.uv[bi]-mesh.uv[ai]);
                normal = (mesh.normals[ai] + mesh.normals[bi]).normalized;
                
                e0i = ai;
                e1i = bi;
            }
            else
            {
                Debug.LogError("Wrong type in SliceTo3()");
            }
            
            //adding new triangles and vertices
            var interIdx = newMeshVertices.Count;
            newMeshVertices.Add(new MeshVertex(inter, uv,normal));
            
            newTriangles.Add(tipi);
            newTriangles.Add(e0i);
            newTriangles.Add(interIdx);
            
            newTriangles.Add(interIdx);
            newTriangles.Add(e1i);
            newTriangles.Add(tipi);
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
        
        public GameObject CreateMesh(List<MeshVertex> vertices, int[] triangles, int[] crossTriangles, GameObject origin)
        {
            GameObject obj = new GameObject();
            var transform = origin.transform;
            if (transform != null)
            {
                obj.transform.SetPositionAndRotation(transform.position, transform.rotation);
                obj.transform.localScale = transform.localScale;
            }
            
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mf.sharedMesh = mesh;
            CopyMeshMaterial(origin, obj);
            MeshVertex.WriteToMesh(ref mesh, vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetTriangles(crossTriangles, 1);
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
            to.GetComponent<MeshFilter>().sharedMesh.subMeshCount = materialList.Count;
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

