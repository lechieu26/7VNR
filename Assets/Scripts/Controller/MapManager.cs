#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MapManager : Singleton<MapManager>
{
    public Transform mapRoot;
    private GameObject currentMap;

//     public void LoadMap(string mapPrefabPath)
//     {
//         // Xoá map cũ
//         if (currentMap != null)
//         {
//             Destroy(currentMap);
//             EnemyManager.Instance.ClearAllEnemies();
//         }

//         GameObject prefab = null;

// // #if UNITY_EDITOR
// //         prefab = AssetDatabase.LoadAssetAtPath<GameObject>(mapPrefabPath + ".prefab");
// // #else
//         prefab = Resources.Load<GameObject>(mapPrefabPath);
// // #endif

//         if (prefab != null)
//         {
//             currentMap = Instantiate(prefab, Vector3.zero, Quaternion.identity, mapRoot);
//         }
//         else
//         {
//             Debug.LogError($"Không tìm thấy map prefab ở path: {mapPrefabPath}");
//             return;
//         }

//         MapTransport mapTransport = currentMap.GetComponent<MapTransport>();
//         CameraFollowBounds.Instance.SetEdgeParent(mapTransport.edgeParent.transform);

//         // Spawn enemies cho map này
//         MapJsonData mapData = GameManager.Instance.GetMapData(mapPrefabPath);
//         mapTransport.SetMapData(mapData);

//         if (mapData != null)
//         {
//             GameObject player = GameObject.FindGameObjectWithTag("Player");
//             if (player != null)
//             {
//                 player.transform.position = mapData.playerSpawnPos;
//             }
//             else
//             {
//                 // Spawn player
//                 player = Instantiate(GameManager.Instance.playerPrefab);
//                 CameraFollowBounds.Instance.SetTarget(player.transform);
//                 player.transform.position = mapData.playerSpawnPos;
//             }

//             foreach (var enemyInfo in mapData.enemies)
//             {
//                 EnemyManager.Instance.SpawnEnemy(enemyInfo.enemyName, enemyInfo.spawnPosition);
//             }
//         }
//     }

}