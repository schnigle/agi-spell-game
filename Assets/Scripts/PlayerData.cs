using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{

    public int totalHP;
    public int hp;
    public Mana mana;

    public PlayerData()
    {
        totalHP = hp = 1000;
        mana = new Mana();
    }


    public void customUpdater()
    {
        if (hp > 0)
            hp -= 1;
        if (hp < 0)
            hp = 0;
    }
}
