using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class EffectManager : Singleton<EffectManager>
{
    [SerializeField] private EffectCharAttackObj effectCharAttackPrefab;
    public IObjectPool<EffectCharAttackObj> effectCharAttackPool;

    protected override void Awake()
    {
        base.Awake();
        effectCharAttackPool = new ObjectPool<EffectCharAttackObj>(
            createFunc: CreateEffectCharAttack,
            actionOnGet: OnGetEffectCharAttack,
            actionOnRelease: OnReleaseEffectCharAttack,
            actionOnDestroy: OnDestroyEffectCharAttack,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
        );
    }

    private void OnDestroyEffectCharAttack(EffectCharAttackObj obj)
    {
        Destroy(obj.gameObject);
    }

    private void OnReleaseEffectCharAttack(EffectCharAttackObj obj)
    {
        obj.isRelease = true;
        obj.gameObject.SetActive(false);
    }

    private void OnGetEffectCharAttack(EffectCharAttackObj obj)
    {
        obj.isRelease = false;
        obj.gameObject.SetActive(true);
    }

    private EffectCharAttackObj CreateEffectCharAttack()
    {
        EffectCharAttackObj obj = Instantiate(effectCharAttackPrefab);
        obj.transform.SetParent(GameManager.Instance.EffectHolder);
        return obj;
    }

    public static string GetEffectCharAttackObjById(int id)
    {
        switch (id)
        {
            case 1:
                int[] ints = { 1, 2, 3, 5 };
                int num = ints[UnityEngine.Random.Range(0, ints.Length)];
                return $"Combo{num}";
            case 2:
                Gun gun = GameObject.FindObjectOfType<Gun>();
                if (gun != null)
                    gun.Shoot();
                return "Skill2_3";
            case 3:
                return "ZSkill10";
            case 4:
                return "Effect_Char_Attack_04";
            case 5:
                return "Effect_Char_Attack_05";
            case 6:
                return "Effect_Char_Attack_06";
            case 7:
                return "Effect_Char_Attack_07";
            case 8:
                return "Effect_Char_Attack_08";
            case 9:
                return "Effect_Char_Attack_09";
            case 10:
                return "Effect_Char_Attack_10";
            default:
                return "None";
        }
    }
}
