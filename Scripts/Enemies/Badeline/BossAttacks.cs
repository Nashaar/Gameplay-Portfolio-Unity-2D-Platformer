using UnityEngine;
using System;

[System.Serializable]
public class BossAttack
{
    public string name;
    public Cooldown cooldown;
    public Action action;
    public bool useAlternate = false;
    public bool attackTriggered = false;
}

[System.Serializable]
public class BossStats
{
    public float bossSpeed;
    public float bulletSpeed;
    public float attackCooldown;
    public Vector3 bulletSize;
    public float telegraphDuration;
}