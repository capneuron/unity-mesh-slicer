using System;
using System.Collections;
using System.Collections.Generic;
using Slicing;
using Unity.VisualScripting;
using UnityEngine;
using Plane = Slicing.Plane;
using Vector3 = UnityEngine.Vector3;

public class Test : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private Transform target;
    [SerializeField] private float distanceToTarget = 7000;
    private Vector3 previousPosition;

    public static string mainObj = "testObj";

    public float sliceForce = 25;
    public float windForce = 10;
    public int windDir = 0;

    public float speed = 3f;
 
    private bool rotate = false;
 
    public float maxView = 90;
    public float minView = 10;

    public Vector3[] windDirs = { 
        Vector3.left, Vector3.left, 
        Vector3.forward, Vector3.forward, 
        Vector3.right, Vector3.right,
        Vector3.back, Vector3.back };
    
    // Start is called before the first frame update
    void Start()
    {
        previousPosition = camera.transform.position;
    }

    // Update is called once per frame
    Vector3 downMousePos = new Vector3();
    Vector3 upMousePos = new Vector3();
    private HashSet<GameObject> sliceObjects = new HashSet<GameObject>();

    void Update()
    {
        // Zoom in/out
        float offsetView = -Input.GetAxis("Mouse ScrollWheel") * speed;
        float tmpView = offsetView + Camera.main.fieldOfView;
        tmpView = Mathf.Clamp(tmpView, minView, maxView);
        Camera.main.fieldOfView = tmpView;

        // Left Mouse Button Clicked
        if (Input.GetMouseButtonDown(0))
        {
            downMousePos = Input.mousePosition;
        }

        // Left Mouse Button Dragging
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastAll(ray, 100);
            foreach (var hit in hits)
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

        // Right Mouse Button Down
        if (Input.GetMouseButtonDown(1))
        {
            rotate = true;
        }
        
        // Right Mouse Button Drag
        if (Input.GetMouseButton(1))
        {
            if (rotate)
            {
                camera.transform.RotateAround(transform.position, Vector3.up, speed * Input.GetAxis("Mouse X"));
                if (camera.transform.position.y > 1f || Input.GetAxis("Mouse Y") <0)
                {
                    camera.transform.RotateAround(transform.position, camera.transform.right, -speed * Input.GetAxis("Mouse Y"));
                }
                
                // if (camera.transform.position.y <= 0)
                // {
                //     Vector3 curPos = camera.transform.position;
                //     camera.transform.position = new Vector3(curPos.x, 0, curPos.z);
                // }
            }
        }

        // Right Mouse Button Up
        if (Input.GetMouseButtonUp(1))
        {
            rotate = false;
        }
        
        if(Input.GetKeyUp(KeyCode.Space))
        {
            Blow(windForce);
        }
        
    }

    void sliceObjectWithMouse(HashSet<GameObject> sliceObjects, Vector3 downPos, Vector3 upPos)
    {
        Slicer slicer = new Slicer(sliceForce);
        List<GameObject> goList = new List<GameObject>();
        List<Plane> planeList = new List<Plane>();
        foreach (GameObject obj in sliceObjects)
        {
            if(!Sliceable.IsSliceable(obj)) continue;
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

    private void Blow(float force = 30)
    {
        var all = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (var go in all)
        {
            if (go.activeSelf && go.TryGetComponent(out Rigidbody rdbd) && Sliceable.IsSliceable(go))
            {
                rdbd.AddForce(windDirs[windDir % windDirs.Length] * force);
                rdbd.AddForce(windDirs[(windDir + 1) % windDirs.Length] * force);
                if (windDir % windDirs.Length == 0)
                {
                    windDir = 0;
                }

                windDir++;
            }
        }
    }
}