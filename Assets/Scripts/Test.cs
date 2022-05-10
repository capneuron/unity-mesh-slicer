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
        GameObject obj = GameObject.Find("testObj");
        
        
        var p = new Plane(new Vector3(1f, 1f, 0).normalized, new Vector3(0, 3f, 0));

        GameObject p1, p2;
        if (slicer.Slice(obj, p, out p1, out p2));

        // Slicer.CopyMesh(GameObject.Find("testObj"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
