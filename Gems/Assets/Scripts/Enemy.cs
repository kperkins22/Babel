using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    void Start()
    {
        
    }

    public void Interact()
    {
        CharacterCombat playerCombat = PlayerMovement.instance.GetComponent<CharacterCombat>();

        if(playerCombat != null)
        {
            playerCombat.Attack();
        }

    }
}
