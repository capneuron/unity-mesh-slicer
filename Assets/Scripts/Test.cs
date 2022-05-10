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
        GameObject obj = GameObject.Find("Cube");
        
        
        var p = new Plane(new Vector3(1f, 1f, 0).normalized, new Vector3(0, 3f, 0));
        slicer.Slice(obj, p);

        // Slicer.CopyMesh(GameObject.Find("Cube"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
