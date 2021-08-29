using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PingSystem
{
    public static void AddPing(Vector3 poistion)
    {
        Object.Instantiate(GameManager.instance.pfPingWorld, poistion, Quaternion.identity);


    }



}
