using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    public GameObject cake;
    public GameObject watermelon;
    private void Reset(GameObject go)
    {
        //Kill all
        var all = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (var debris in all)
        {
            if (Sliceable.IsSliceable(debris))
            {
                Destroy(debris);
            }
        }
        GameObject.Instantiate(go, Vector3.zero, Quaternion.identity);
    }

    public void ResetCake()
    {
        Reset(cake);
    }
    
    public void ResetWatermelon()
    {
        Reset(watermelon);
    }
}
