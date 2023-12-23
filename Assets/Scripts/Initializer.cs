using System;
using UnityEngine;
using UZSG;

public sealed class Initializer : MonoBehaviour
{
    public void Spawn(IEntity entity)
    {
        entity.Spawn();
    }
}
