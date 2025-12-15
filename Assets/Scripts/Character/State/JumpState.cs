using UnityEngine;

public class JumpState : State
{
    public JumpState(Character character) : base(character) { }

    public override void Enter()
    {
        base.Enter();
        _character.SetAnimation("Jump", false);

        if (_character.rb != null)
        {
            _character.rb.velocity = new Vector2(_character.rb.velocity.x, _character.jumpForce);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update(float horizontalInput, float verticalInput)
    {
        float moveDir = 0;
        float verticalDir = 0;
        // if (horizontalInput != 0)
        // {
        //     moveDir = horizontalInput;
        //     _character.rb.velocity = new Vector2(moveDir * _character.moveSpeed, _character.rb.velocity.y);
        // }
        // if (verticalInput != 0)
        // {
        //     verticalDir = verticalInput;
        //     if (verticalDir == 0)
        //     {
        //         _character.SetState(new FlyState(_character));
        //         return;
        //     }
        //     _character.rb.velocity = new Vector2(_character.rb.velocity.x, verticalDir == 0 ? _character.rb.velocity.y : _character.jumpForce);
        // }
        if (horizontalInput != 0 || verticalInput != 0)
        {
            moveDir = horizontalInput;
            verticalDir = verticalInput;
            if (verticalDir == 0)
            {
                _character.SetState(new FlyState(_character));
                return;
            }
            _character.rb.velocity = new Vector2(moveDir * _character.moveSpeed, verticalDir == 0 ? _character.rb.velocity.y : _character.jumpForce);
        }
        else
        {
            _character.SetState(new FallState(_character));
        }
        if (_character.rb.velocity.y <= 0 && _character.IsGrounded())
        {
            _character.SetState(new IdleState(_character));
        }
    }
}
