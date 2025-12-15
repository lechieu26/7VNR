using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySpawnInfo
{
    public string enemyName;
    public Vector2 spawnPosition;
}

[Serializable]
public class MapJsonData
{
    public string mapPrefab;
    public string leftMapPrefab;
    public string rightMapPrefab;
    public Vector3 playerSpawnPos;
    public List<EnemySpawnInfo> enemies;
}

[Serializable]
public class MapDataWrapper
{
    public List<MapJsonData> maps;
}
