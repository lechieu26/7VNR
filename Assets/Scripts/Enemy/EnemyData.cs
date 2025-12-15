using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public string name;
    public string spineAsset;
    public string prefabPath;

    public int hp;
    public int hpMax;
    public int damage;
    public float speed;
    public float detectionRange;     
    public float attackRange;
    public float attackCooldown;

    public Vector2 spawnPoint;
    public float respawnTime;
    
    // Patrol settings
    public float patrolRange = 3f;  
    public float patrolSpeed = 2f; 
    public float patrolWaitTime = 2f; 

    public AnimationData animations;

    [SerializeField]
    public class EnemyDataWrapper
    {
        public EnemyData[] enemies;
    }

    [System.Serializable]
    public class AnimationData
    {
        public string idle;
        public string walk;
        public string attack;
        public string hit;
        public string die;
    }
}