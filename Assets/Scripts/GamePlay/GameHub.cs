using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHub : MonoBehaviour
{
    [SerializeField] private SkillAttackPanel skillAttackPanel;

    public void SetSkillButtons(List<Skill> skills)
    {
        skillAttackPanel.SetSkillButtons(skills.ToArray());
    }
}
