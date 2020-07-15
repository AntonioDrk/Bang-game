using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LifeTokensContainer : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> lifeTokens;

    private void Awake()
    {
        if(lifeTokens == null)
            Debug.LogError("You haven't set the individual tokens in the inspector!");
    }

    /// <summary>
    /// Shows/hides the proper amount of life tokens depending on how many lives the player has
    /// </summary>
    /// <param name="amount">Lives of the player</param>
    public void SetLifeTokens(int amount)
    {
        int stopIndex = lifeTokens.Count - amount;
        if (stopIndex < 0 || stopIndex > lifeTokens.Count)
        {
            Debug.LogError("The lives amount is set wrong");
            return;
        }
        
        // Hide all the life tokens until stopIndex
        for (int i = 0; i < stopIndex; i++)
        {
            lifeTokens[i].SetActive(false);    
        }

        // Show the other ones
        for (int i = stopIndex; i < lifeTokens.Count; i++)
        {
            lifeTokens[i].SetActive(true);
        }
    }
}
