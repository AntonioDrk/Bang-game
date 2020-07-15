using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Role
{
    None,
    Sheriff,
    Outlaw,
    Deputy,
    Renegade
}

/// <summary>
/// This component represents the entire GAME logic of the player, such as lives, role, character card and so on...
/// </summary>
public class PlayerLogic : MonoBehaviour
{

    // The character card associated with this player
    private CharacterCard _charCard;
    public CharacterCard CharCard { get => _charCard; set => _charCard = value; }

    // The current lives this player has
    private int _currentLives;
    public int CurrentLives { get => _currentLives; private set => _currentLives = value; }
    
    // The current range of the player
    private int _currentRange;
    public int CurrentRange { get => _currentRange; set => _currentRange = value < 1 ? 1 : value; }

    // The distance bonus of the player
    private int _distanceBonus;
    public int DistanceBonus { get => _distanceBonus; set => _distanceBonus = value < -1 ? -1 : value > 1 ? 1 : value ; }
        
    // The role associated with the player
    [SerializeField]
    private Role _playerRole;
    public Role PlayerRole { get => _playerRole; set => _playerRole = value; }

    public void SetLives(int amount)
    {
        CurrentLives = amount;
        if (CurrentLives < 0)
        {
            CurrentLives = 0;
            Debug.LogError("Tried to set the lives to a negative number!");
        }

        if (_playerRole == Role.Sheriff)
        {
            if (CurrentLives > _charCard.Lives + 1)
            {
                CurrentLives = (int)_charCard.Lives + 1;
                Debug.LogError("Tried to set the lives to a higher number than the number on the role card!");
            }
            
        } else {
            if (CurrentLives > _charCard.Lives)
            {
                CurrentLives = (int)_charCard.Lives;
                Debug.LogError("Tried to set the lives to a higher number than the number on the role card!");
            }
        }
        
        // Callback so the local player updates his canvas
        gameObject.GetComponent<PlayerManager>().UpdateLivesText();
    }
}
