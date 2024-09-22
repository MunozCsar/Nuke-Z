using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeTrigger : MonoBehaviour
{
    public WeaponHandler weaponHandler; // Referencia al script de control de armas
    // Muestra el arma
    public void ShowWeapon()
    {
        weaponHandler.ShowWeapon();
    }
}
