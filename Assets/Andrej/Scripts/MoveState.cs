using UnityEngine;

public abstract class MoveState : State
{
    protected Vector2 moveDirection;

    public override void DoFixed()
    {
        enemyProperties.controller.Move(enemyProperties.speed * Time.fixedDeltaTime * moveDirection);

        base.DoFixed();
    }
}
