using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameCanvasManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberOfLivesText;
    [SerializeField] private TextMeshProUGUI turnInfo;

    private void Start()
    {
        StartChecks();
    }

    private void StartChecks()
    {
        if(numberOfLivesText == null)
            Debug.LogWarning("CanvasManagerGame: Number of lives not set in the inspector, make sure it's linked!");
        if(turnInfo == null)
            Debug.LogWarning("CanvasManagerGame: Turn info not set in the inspector, make sure it's linked!");
    }

    public void SetLives(int nrOfLives)
    {
        numberOfLivesText.text = nrOfLives.ToString();
    }
}
