using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UZSG.Entities;
using UZSG.FPP;
using UZSG.Items.Weapons;
using UZSG.Systems;

public class Compass : MonoBehaviour
{
    public Player Player;
    public RawImage compassImage;
    private Transform _posPlayer;

    // Start is called before the first frame update
    void Start()
    {
        _posPlayer = Player.FPP.CameraController.transform;
    }

    // Update is called once per frame
    void Update()
    {
        compassImage.uvRect = new Rect(_posPlayer.localEulerAngles.y / 360f, 0, 1f, 1f);
    }
}
