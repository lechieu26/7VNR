using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject HubCanvas;
    [SerializeField] private SkillAttackPanel skillAttackPanel;

    public void SetSkillButtons(List<Skill> skills)
    {
        skillAttackPanel.SetSkillButtons(skills.ToArray());
    }

    private void Start()
    {
        if (HubCanvas != null)
        {
            HubCanvas.SetActive(true);
        }
    }
    void Update()
    {

    }
}
