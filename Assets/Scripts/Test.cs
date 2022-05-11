using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Slicing;
using UnityEditor;
using UnityEngine;
using Plane = Slicing.Plane;
using Vector3 = UnityEngine.Vector3;

public class Test : MonoBehaviour
{
    public static string mainObj = "testObj";

    // Start is called before the first frame update
    void Start()
    {
         // Slicer slicer = new Slicer();
         // GameObject obj = GameObject.Find("testObj");
         //
         // //TODO:
         // var p = new Plane(new Vector3(0f, 3f, 0), new Vector3(0.5f, 4f, 0), new Vector3(0f, 3f, 1));
         // GameObject p1, p2;
         // if (slicer.Slice(obj, p, out p1, out p2));
    }

    // Update is called once per frame
    Vector3 downMousePos = new Vector3();
    Vector3 upMousePos = new Vector3();
    private HashSet<string> sliceObjects = new HashSet<string>();

    void Update()
    {
        // Left Mouse Button Clicked
        if (Input.GetMouseButtonDown(0))
        {
            downMousePos = Input.mousePosition;
            // Debug.Log("click (x, y, z): ("
            //           + downMousePos.x + ", " + downMousePos.y + ", " + downMousePos.z + ")");
        }

        // Left Mouse Button Dragging
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                string objName = hit.transform.gameObject.name;
                if (objName.StartsWith(mainObj))
                {
                    sliceObjects.Add(objName);
                    // Debug.Log("hit: set.size = " + sliceObjects.Count);
                }
            }
        }

        // Left Mouse Button Released
        if (Input.GetMouseButtonUp(0))
        {
            upMousePos = Input.mousePosition;
            // Debug.Log("UP (x, y, z): ("
            //           + upMousePos.x + ", " + upMousePos.y + ", " + upMousePos.z + ")");

            sliceObjectWithMouse(sliceObjects, downMousePos, upMousePos);
            sliceObjects.Clear();
        }
    }

    void sliceObjectWithMouse(HashSet<string> sliceObjects, Vector3 downPos, Vector3 upPos)
    {
        Slicer slicer = new Slicer();
        foreach (string name in sliceObjects)
        {
            GameObject obj = GameObject.Find(name);
            Debug.Log("cut obj: " + name);

            var p = new Plane(
                Camera.main.ScreenToWorldPoint(new Vector3(downPos.x, downPos.y, Camera.main.nearClipPlane)),
                Camera.main.ScreenToWorldPoint(new Vector3(upPos.x, upPos.y, Camera.main.nearClipPlane)),
                Camera.main.ScreenToWorldPoint(new Vector3(upPos.x, upPos.y, Camera.main.farClipPlane)));

            GameObject p1, p2;
            if (slicer.Slice(obj, p, out p1, out p2)) ;
        }
    }
}