using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class State
{
    protected Character _character;

    public State(Character character)
    {
        _character = character;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public abstract void Update(float horizontalInput, float verticalInput);
}
