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
        
        //TODO:
        // var p = new Plane(new Vector3(1f, 1f, 0).normalized, new Vector3(0, 3f, 0));
        var p = new Plane(new Vector3(0f, 3f, 0), new Vector3(0.5f, 4f, 0), new Vector3(0f, 3f, 1));

        GameObject p1, p2;
        if (slicer.Slice(obj, p, out p1, out p2));

        // Slicer.CopyMesh(GameObject.Find("testObj"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
