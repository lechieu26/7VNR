using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public class SkillAttackUi : MonoBehaviour
{
    private Button button;
    public int skillId;
    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClickSkill);
    }

    private void OnClickSkill()
    {
        SkillManager.Instance.UseSkill(skillId);
    }

    public void SetData(int skillId, Sprite sprite)
    {
        this.skillId = skillId;
        button.image.sprite = sprite;
    }
}
