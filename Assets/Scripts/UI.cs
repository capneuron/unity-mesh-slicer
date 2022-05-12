using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public GameObject cake;
    public GameObject watermelon;
    private int cutTimes = 0;
    private int piecesCount = 1;

    public GameObject cutTimeGo;
    public GameObject piecesCountGo;

    private TextMeshProUGUI cutText;
    private TextMeshProUGUI pieceText;

    public Test testComponent;
    private void Start()
    {
        cutText = cutTimeGo.GetComponent<TextMeshProUGUI>();
        pieceText = piecesCountGo.GetComponent<TextMeshProUGUI>();
    }
    

    private void Reset(GameObject go)
    {
        SetCutTimes(0);
        SetPiecesCount(1);
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
    
    public void Blow()
    {
        testComponent.Blow(testComponent.windForce);
    }

    public void SetCutTimes(int count)
    {
        cutTimes = count;
        cutText.SetText("Cut "+cutTimes+" times");
    }
    
    public void SetPiecesCount(int pieces)
    {
        piecesCount = pieces;
        pieceText.SetText("Into "+piecesCount+" Piece");
    }
    
    
}
