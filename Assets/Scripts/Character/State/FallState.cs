using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallState : State
{
    public FallState(Character character) : base(character)
    {
    }

    public override void Enter()
    {
        _character.SetAnimation("Fall", true);
        _character.rb.velocity = Vector2.zero;
        _character.rb.gravityScale = 5f;
    }

    public override void Exit()
    {
        base.Exit();
    }
    public override void Update(float horizontalInput, float verticalInput)
    {
        if (verticalInput > 0)
        {
            _character.SetState(new JumpState(_character));
            return;
        }
        _character.rb.velocity = new Vector2(horizontalInput * _character.moveSpeed, _character.rb.velocity.y);
        _character.horizontalInput = 0;
        if (_character.rb.velocity.y <= 0 && _character.IsGrounded())
        {
            _character.SetState(new IdleState(_character));
        }
    }
}
