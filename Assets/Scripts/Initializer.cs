using System;
using Unity.VisualScripting;
using UnityEngine;
using URMG;

public sealed class Initializer : MonoBehaviour
{
    public void Spawn(IEntity entity)
    {
        entity.Spawn();
    }
}
