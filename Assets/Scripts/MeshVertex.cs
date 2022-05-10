using System;
using System.Collections.Generic;
using UnityEngine;
namespace Slicing
{
    public class MeshVertex
    {
        public Vector3 vertex;
        public Vector2 uv;
        public Vector3 normal;
        public Vector4 tangent;

        public MeshVertex(Vector3 vertex, Vector2 uv, Vector3 normal, Vector4 tangent)
        {
            this.vertex = vertex;
            this.uv = uv;
            this.normal = normal;
            this.tangent = tangent;
        }
        public MeshVertex(Vector3 vertex)
        {
            this.vertex = vertex;
        }
        public MeshVertex(Vector3 vertex, Vector2 uv)
        {
            this.vertex = vertex;
            this.uv = uv;
        }
        public MeshVertex(Vector3 vertex, Vector2 uv, Vector3 normal)
        {
            this.vertex = vertex;
            this.uv = uv;
            this.normal = normal;
        }
        
        static public List<MeshVertex> ReadFromMesh(Mesh mesh)
        {
            List<MeshVertex> res = new List<MeshVertex>(mesh.vertexCount);
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                MeshVertex mv = new MeshVertex(
                    mesh.vertices[i],
                    mesh.uv[i],
                    mesh.normals[i],
                    mesh.tangents[i]);
                res.Add(mv);
            }
            return res;
        }

        static public void WriteToMesh(ref Mesh mesh, List<MeshVertex> list)
        {
            var v = new Vector3[list.Count];
            var uv = new Vector2[list.Count];
            var n = new Vector3[list.Count];
            var t = new Vector4[list.Count];
            MeshVertex.Unzip(list, out v, out uv, out n, out t);
            mesh.vertices = v;
            mesh.uv = uv;
            mesh.normals = n;
            mesh.tangents = t;
        }
        
        static public void Unzip(List<MeshVertex> list,
            out Vector3[] v, 
            out Vector2[] uv,
            out Vector3[] n, 
            out Vector4[] t)
        {
            v = new Vector3[list.Count];
            uv = new Vector2[list.Count];
            n = new Vector3[list.Count];
            t = new Vector4[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                v[i] = list[i].vertex;
                uv[i] = list[i].uv;
                n[i] = list[i].normal;
                t[i] = list[i].tangent;
            }
        }

    }
}