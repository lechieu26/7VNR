using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyState : State
{
    public FlyState(Character character) : base(character)
    {
    }

    public override void Enter()
    {
        _character.SetAnimation("Fly", true);
        _character.rb.gravityScale = 0f;
    }

    public override void Exit()
    {
        base.Exit();
    }
    public override void Update(float horizontalInput, float verticalInput)
    {
        _character.rb.velocity = new Vector2(horizontalInput * _character.moveSpeed, verticalInput * _character.moveSpeed);
        _character.horizontalInput = 0;
        if (verticalInput > 0)
        {
            _character.SetState(new JumpState(_character));
            return;
        }
        if (_character.rb.velocity.x == 0)
        {
            _character.SetState(new FallState(_character));
            return;
        }
    }
}
