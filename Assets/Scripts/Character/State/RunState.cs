using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunState : State
{
    public RunState(Character character) : base(character)
    {
    }
    public override void Enter()
    {
        _character.SetAnimation("Run", true);
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void Update(float horizontalInput, float verticalInput)
    {
        if (horizontalInput != 0)
        {
            _character.rb.velocity = new Vector2(horizontalInput * _character.moveSpeed, verticalInput == 0 ? _character.rb.velocity.y : _character.jumpForce);
        }
        else
        {
            _character.rb.velocity = new Vector2(0, _character.rb.velocity.y);
        }
        if (horizontalInput == 0 && _character.IsGrounded())
        {
            _character.SetState(new IdleState(_character));
            return;
        }
        if (verticalInput != 0 && _character.IsGrounded())
        {
            _character.SetState(new JumpState(_character));
        }
    }
}
