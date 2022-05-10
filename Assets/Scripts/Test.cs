using System.Collections;
using System.Collections.Generic;
using Slicing;
using UnityEditor;
using UnityEngine;
using Plane = Slicing.Plane;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Slicer slicer = new Slicer();
        Vector3[] v = new[] {Vector3.zero, new Vector3(0, 5, 0), new Vector3(5, 0, 0)};
        int[] t = new[] {0, 1, 2};
        var obj = slicer.CreateMesh(v, t);
        obj.name = "base";
        

        var p = new Plane(new Vector3(0, 1f, 0), new Vector3(0, 3f, 0));
        slicer.Slice(obj, p);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
