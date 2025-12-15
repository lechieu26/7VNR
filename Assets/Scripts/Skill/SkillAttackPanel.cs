using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillAttackPanel : MonoBehaviour
{
    public List<SkillAttackUi> skillButtons;

    public void SetSkillButtons(Skill[] skills)
    {
        if(skills == null)
        {
            Debug.LogWarning("Skills array is null.");
            return;
        }
        for (int i = 0; i < skillButtons.Count; i++)
        {
            if (i < skills.Length && skills[i] != null)
            {
                skillButtons[i].gameObject.SetActive(true);
                skillButtons[i].SetData(skills[i].skillId, GetIconForSkill(skills[i].template.icon));
            }
            else
            {
                skillButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private Sprite GetIconForSkill(string icon)
    {
        return Resources.Load<Sprite>($"uisprite/iconSkill/{icon}");
    }
}
