using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private float min;
    [SerializeField] private Transform bar;
    private float ratePercent;
    private Enemy target;
    private Vector3 offset;

    void Awake()
    {
        ratePercent = min;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 pos = target.transform.position + offset;
        transform.position = pos;

        UpdateFill(target.hp, target.maxHp);
    }

    public void UpdateFill(long currentHp, long hpMax)
    {
        float value = Mathf.Clamp01((float)currentHp / hpMax);
        ratePercent = -((value - 1) * min);
        bar.transform.localPosition = new Vector2(ratePercent, 0);
    }

    public void SetTarget(Enemy enemy)
    {
        target = enemy;

        transform.SetParent(null);

        offset = new Vector3(0, enemy.GetTopCollider(), 0);
        transform.position = enemy.transform.position + offset;

        UpdateFill(enemy.hp, enemy.maxHp);
    }

    public void ClearTarget()
    {
        target = null;
        gameObject.SetActive(false);
    }
}