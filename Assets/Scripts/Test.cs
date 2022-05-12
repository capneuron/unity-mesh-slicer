using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Slicing;
using UnityEngine;
using Plane = Slicing.Plane;
using Vector3 = UnityEngine.Vector3;

public class Test : MonoBehaviour
{
    [SerializeField] private Camera camera;
    public static string mainObj = "testObj";

    public float force = 25;
    // Start is called before the first frame update
    void Start()
    {

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
        }

        // Left Mouse Button Dragging
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                sliceObjects.Add(hit.transform.gameObject);
            }
        }

        // Left Mouse Button Released
        if (Input.GetMouseButtonUp(0))
        {
            upMousePos = Input.mousePosition;
            sliceObjectWithMouse(sliceObjects, downMousePos, upMousePos);
            sliceObjects.Clear();
        }
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // camera.transform.Translate(new Vector3(10, 0, 0));
            camera.transform.Rotate(new Vector3(0, 1, 0), -10, Space.World);
        }
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // camera.transform.Translate(new Vector3(-10, 0, 0));
            camera.transform.Rotate(new Vector3(0, 1, 0), 10, Space.World);
        }
        
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            camera.transform.Translate(new Vector3(0, 10, 0), Space.World);
            camera.transform.Rotate(new Vector3(1, 0, 0), 10, Space.World);
        }
        
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            camera.transform.Translate(new Vector3(0, -10, 0), Space.World);
            camera.transform.Rotate(new Vector3(1, 0, 0), -10, Space.World);
        }
    }

    void sliceObjectWithMouse(HashSet<GameObject> sliceObjects, Vector3 downPos, Vector3 upPos)
    {
        Slicer slicer = new Slicer(force);
        List<GameObject> goList = new List<GameObject>();
        List<Plane> planeList = new List<Plane>();
        foreach (GameObject obj in sliceObjects)
        {
            if(!Sliceable.IsSliceable(obj)) continue;
            
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