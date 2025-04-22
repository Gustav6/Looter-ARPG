using UnityEngine;

public abstract class MoveState : State
{
   
    public override void DoFixed()
    {
        enemyProperties.controller.Move(enemyProperties.speed * Time.fixedDeltaTime * enemyProperties.direction);

        base.DoFixed();
    }
}
