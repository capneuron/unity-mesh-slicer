using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliceable : MonoBehaviour
{
    public Material insideMaterial;

    static public bool IsSliceable(GameObject obj)
    {
        return obj.GetComponent<Sliceable>() != null;
    }
}
