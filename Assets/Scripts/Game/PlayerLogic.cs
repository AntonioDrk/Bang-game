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
    
    // The role associated with the player
    [SerializeField]
    private Role _playerRole;
    public Role PlayerRole { get => _playerRole; set => _playerRole = value; }

    void Start()
    {
        
    }
    
    void Update()
    {
        
    }
}
