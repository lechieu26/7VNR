using UnityEngine;
using UnityEngine.UI;

public class EnemyInfoUI : MonoBehaviour
{
    public Text nameText;
    public Text hpText;

    private static EnemyInfoUI instance;
    private Enemy currentEnemy;

    private EnemyData currentEnemyData;

// #if UNITY_EDITOR
//     private void OnValidate()
//     {
//         //  Tự động tắt trong chế độ chỉnh Scene (chưa bấm Play)
//         // if (!Application.isPlaying && gameObject.activeSelf)
//         // {
//         //     gameObject.SetActive(false);
//         // }
//     }
// #endif

    // private void Awake()
    // {
    //     instance = this;
    //     HideInfo();
    // }

    // private void Start()
    // {
    //     //  Khi Play game, bật UI trở lại để nó hoạt động
    //     gameObject.SetActive(true);
    //     HideInfo(); // ẩn nội dung chờ click enemy
    // }

    public static void Show(Enemy enemy)
    {
        if (instance == null) instance = FindObjectOfType<EnemyInfoUI>(includeInactive: true);
        instance.ShowInfo(enemy);
    }

    public static void Hide()
    {
        if (instance == null) return;
        instance.HideInfo();
    }

    public void ShowInfo(Enemy enemy)
    {
        currentEnemy = enemy;
        nameText.text = enemy.data.name;
        hpText.text = "HP: " + enemy.hp + "/" + enemy.maxHp;
        gameObject.SetActive(true);
    }

    public void HideInfo()
    {
        gameObject.SetActive(false);
        currentEnemy = null;
    }

    private void Update()
    {
        //  Nếu enemy chết thì ẩn UI
        // if (currentEnemy != null && currentEnemy.IsDead)
        // {
        //     HideInfo();
        // }
    }
}
