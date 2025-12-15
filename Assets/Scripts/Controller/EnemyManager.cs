#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using static EnemyData;
using System;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public List<Enemy> enemies = new List<Enemy>();
    private Dictionary<string, EnemyData> enemyDatabase;

    [SerializeField] private TextAsset enemyDataJson;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Load Enemy Data từ JSON
        EnemyDataWrapper wrapper = JsonUtility.FromJson<EnemyDataWrapper>(enemyDataJson.text);
        enemyDatabase = new Dictionary<string, EnemyData>();
        foreach (var e in wrapper.enemies)
        {
            enemyDatabase[e.name] = e;
        }
    }

    public Enemy SpawnEnemy(string enemyName, Vector2 position)
    {
        if (!enemyDatabase.TryGetValue(enemyName, out EnemyData data))
        {
            Debug.LogWarning($"Không tìm thấy enemy {enemyName}");
            return null;
        }

        // Load prefab theo path từ JSON
        GameObject prefab = null;

        prefab = Resources.Load<GameObject>(data.prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"Không tìm thấy prefab cho enemy {enemyName} ở path {data.prefabPath}");
            return null;
        }

        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(GameManager.Instance.EnemiesHolder);
        Enemy enemy = obj.GetComponent<Enemy>();
        enemy.transform.position = position;
        enemy.Init(data, position);
        enemies.Add(enemy);

        Debug.Log($"Spawn {data.name} tại {position}");
        return enemy;
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(ray.origin, Vector2.zero);
            foreach (var h in hit)
            {
                if (h.collider != null && h.collider.CompareTag("Enemy"))
                {
                    Enemy enemy = h.collider.GetComponent<Enemy>();
                    EnemyInfoUI.Show(enemy);
                    break;
                }
            }
        }
    }

    public void ClearAllEnemies()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }
        enemies.Clear();
    }

    public void LoadMapEnemies(List<EnemySpawnInfo> enemies)
    {
        Debug.Log(enemies);
        foreach (var enemyInfo in enemies)
        {
            Debug.Log("Spawn enemy: " + enemyInfo.enemyName + " at " + enemyInfo.spawnPosition);
            SpawnEnemy(enemyInfo.enemyName, enemyInfo.spawnPosition);
        }
    }
}

