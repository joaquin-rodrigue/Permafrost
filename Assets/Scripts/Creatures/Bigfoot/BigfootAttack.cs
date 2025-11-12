using UnityEngine;

public class BigfootAttack : BigfootState
{
    public BigfootAttack(BigfootStateController controller) : base(controller) { }

    public override void Act()
    {
        float distance = Vector3.Distance(controller.transform.position, controller.PlayerPosition);

        controller.direction = (controller.PlayerPosition - controller.transform.position).normalized;

        if (distance < 1f)
        {
            controller.Attack();
            controller.aggression--;
        }
        if (!controller.Attacking)
        {
            controller.Move();
        }

        if (distance > controller.AttackDistance / 2)
        {
            controller.aggression -= Time.deltaTime;
        }
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.transform.position, controller.PlayerPosition);
        if (distance >= controller.IdealStalkDistance)
        {
            controller.SetState(new BigfootWander(controller));
        }
        if (controller.aggression <= 0)
        {
            controller.SetState(new BigfootRetreat(controller));
        }
    }

    public override void OnStateEnter()
    {
        controller.currentSpeed = controller.RunSpeed;
        controller.aggression = controller.AggressionLimit;
    }

    public override void OnStateExit()
    {
        controller.timeSinceAttackLeave = 0;
    }
}
