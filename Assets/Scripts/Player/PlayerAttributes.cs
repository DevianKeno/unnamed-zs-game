
using System;
using UnityEngine;

namespace URMG.Player
{
/// <summary>
/// Represents the default attributes a player has.
/// </summary>
[RequireComponent(typeof(PlayerCore))]
public class PlayerAttributes : Attributes
{        
    [SerializeField] Attribute _movementSpeed;
    public Attribute MoveSpeed { get => _movementSpeed; }
    [SerializeField] Attribute _jumpHeight;
    public Attribute JumpHeight { get => _jumpHeight; }

    public PlayerAttributes()
    {
        Init();
    }

    public void Init()
    {
        _movementSpeed = new(10f);
        _jumpHeight = new(10f);
    }
}
}