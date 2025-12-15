using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Skill
{
    public int skillId;
    public int damage;
    public int level;
    public int cooldown;
    public int maxFight;
    public int manaCost;
    public int dx;
    public int dy;
    public long lastTimeUseSkill;
    public SkillTemplate template;

    public bool IsCooldown()
    {
        long currentTime = GameManager.GetCurrentMilisecond();
        Debug.Log(currentTime - lastTimeUseSkill);
        return (currentTime - lastTimeUseSkill) > cooldown;
    }
}