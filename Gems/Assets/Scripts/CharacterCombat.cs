using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    public int damageValue = 10; 

   public void Attack()
    {
        PlayerMovement.instance.TakeDamage(damageValue);
    }
}
