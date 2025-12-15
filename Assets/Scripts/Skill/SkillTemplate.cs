using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class SkillTemplate
{
    public int id;
    public string name;
    public string description;
    public string icon;
    public int maxPoint;
    public Skill[] skills;

}
