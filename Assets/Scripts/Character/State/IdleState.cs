using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public IdleState(Character character) : base(character)
    {
    }

    public override void Enter()
    {
        _character.SetAnimation("Idle", true);
        _character.rb.gravityScale = 5f;
    }

    public override void Exit()
    {
        base.Exit();
    }
    public override void Update(float horizontalInput, float verticalInput)
    {
        if (!_character.IsGrounded())
        {
            _character.SetState(new FallState(_character));
            return;
        }
        if (verticalInput != 0 && _character.IsGrounded())
        {
            _character.SetState(new JumpState(_character));
        }
        else if (horizontalInput != 0)
        {
            _character.SetState(new RunState(_character));
        }
    }
}
