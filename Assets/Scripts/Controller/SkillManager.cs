using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTemplateWrapper
{
    public List<SkillTemplate> skillTemplates;
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    private Dictionary<int, SkillTemplate> SkillTemplates;
    private Dictionary<int, Skill> Skills;

    [SerializeField] private TextAsset skillDataJson;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadSkillTemplates();
    }

    private void LoadSkillTemplates()
    {
        if (skillDataJson != null)
        {
            SkillTemplateWrapper wrapper = JsonUtility.FromJson<SkillTemplateWrapper>(skillDataJson.text);
            SkillTemplates = new Dictionary<int, SkillTemplate>();
            Skills = new Dictionary<int, Skill>();
            foreach (var skillTemplate in wrapper.skillTemplates)
            {
                SkillTemplates[skillTemplate.id] = skillTemplate;
                foreach (var skill in skillTemplate.skills)
                {
                    skill.template = skillTemplate;
                    Skills[skill.skillId] = skill;
                }
            }
        }
        else
        {
            Debug.LogError("Không tìm thấy skillTemplates.json");
        }
    }



    public SkillTemplate GetSkillTemplate(int id)
    {
        if (SkillTemplates.TryGetValue(id, out SkillTemplate template))
        {
            return template;
        }
        Debug.LogWarning($"Không tìm thấy skill với ID {id}");
        return null;
    }

    public void UseSkill(int skillId)
    {
        Skill skill = GetSkillById(skillId);
        if (skill == null) return;

        // 🔴 KIỂM TRA TARGET
        Enemy target = Character.Instance.targetEnemy;

        if (target == null || target.IsDead)
            return;

        // 🔴 KIỂM TRA COOLDOWN
        if (!skill.IsCooldown())
            return;

        string effectName = EffectManager.GetEffectCharAttackObjById(skill.template.id);

        // EFFECT & ANIM
        EffectCharAttackObj.AddEffect(effectName, Character.Instance.transform.position, Character.Instance);
        Character.Instance.SetAnimation(effectName);

        skill.lastTimeUseSkill = GameManager.GetCurrentMilisecond();

        // (Tùy bạn muốn thêm damage hay không)
        target.TakeDamage(skill.damage);
    }


    public Skill GetSkillById(int skillId)
    {
        // if (Skills.TryGetValue(skillId, out Skill skill))
        // {
        //     return skill;
        // }
        // Debug.LogWarning($"Không tìm thấy kỹ năng với ID {skillId}");
        // return null;
        if (Skills.TryGetValue(skillId, out Skill skill))
        {
            Skill newSkill = new Skill
            {
                skillId = skill.skillId,
                cooldown = skill.cooldown,
                damage = skill.damage,
                template = skill.template
            };
            return newSkill;
        }
        Debug.LogWarning($"Không tìm thấy kỹ năng với ID {skillId}");
        return null;
    }

}
