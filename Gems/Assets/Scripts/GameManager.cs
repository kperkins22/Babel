using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform pfPingWorld;

    public int maxHealth = 100;
    public int playerCurrentHealth = 100;
    public Image healthBar;

    public int maxDogHealth = 100;
    public int dogCurrentHealth = 100;
    public Image dogHealthBar;

    public int maxStam = 100;
    public int playerCurrentStam = 100;
    public Image stamBar;
    public void Start()
    {
        instance = this;
    }


    public void UpdateStats()
    {
        healthBar.fillAmount = (float)playerCurrentHealth / (float)maxHealth;
        dogHealthBar.fillAmount = (float)dogCurrentHealth / (float)maxDogHealth;
        stamBar.fillAmount = (float)playerCurrentStam / (float)maxStam;
    }

   
}
