using System;
using System.Collections;
using System.Collections.Generic;
using Slicing;
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
    private HashSet<GameObject> sliceObjects = new HashSet<GameObject>();

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

            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                string objName = hit.transform.gameObject.name;
                if (objName.StartsWith(mainObj))
                {
                    // sliceObjects.Add(objName);
                    // Debug.Log("hit: set.size = " + sliceObjects.Count);
                    sliceObjects.Add(hit.transform.gameObject);
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

    void sliceObjectWithMouse(HashSet<GameObject> sliceObjects, Vector3 downPos, Vector3 upPos)
    {
        Slicer slicer = new Slicer(25);
        List<GameObject> goList = new List<GameObject>();
        List<Plane> planeList = new List<Plane>();
        foreach (GameObject obj in sliceObjects)
        {
            // Debug.Log("cut obj: " + name);

            var p = new Plane(
                Camera.main.ScreenToWorldPoint(new Vector3(downPos.x, downPos.y, Camera.main.nearClipPlane)),
                Camera.main.ScreenToWorldPoint(new Vector3(upPos.x, upPos.y, Camera.main.nearClipPlane)),
                Camera.main.ScreenToWorldPoint(new Vector3(upPos.x, upPos.y, Camera.main.farClipPlane)));

            goList.Add(obj);
            planeList.Add(p);
        }

        StartCoroutine(SliceMultiple(slicer, goList, planeList));
    }

    IEnumerator SliceMultiple(Slicer slicer, List<GameObject> objs, List<Plane> planes, Action<GameObject, GameObject> cb=null)
    {
        for (int i = 0; i < objs.Count; i++)
        {
            slicer.Slice(objs[i], planes[i], out GameObject o1, out GameObject o2);
            cb?.Invoke(o1, o2);

            yield return new WaitForSeconds(.10f);
        }
    }
}