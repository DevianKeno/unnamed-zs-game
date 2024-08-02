using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Entities;
using UZSG.FPP;
using UZSG.Items.Weapons;
using UZSG.Systems;

public class SwitchCrosshair : MonoBehaviour
{
    public Player player;
    // Start is called before the first frame update
    void Start()
    {
        if (player.FPP.HeldItem is GunWeaponController gunWeapon)
        {
            gameObject.transform.Find("Non-Weapon Crosshair").gameObject.SetActive(false);
        }
        else
        {
            gameObject.transform.Find("Weapon Crosshair").gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player.FPP.HeldItem is GunWeaponController gunWeapon)
        {
            gameObject.transform.Find("Non-Weapon Crosshair").gameObject.SetActive(false);
            gameObject.transform.Find("Weapon Crosshair").gameObject.SetActive(true);
        }
        else
        {
            gameObject.transform.Find("Non-Weapon Crosshair").gameObject.SetActive(true);
            gameObject.transform.Find("Weapon Crosshair").gameObject.SetActive(false);
        }
    }
}
