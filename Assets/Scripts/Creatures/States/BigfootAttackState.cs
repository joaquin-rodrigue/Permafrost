using UnityEngine;

public class BigfootAttackState : State
{
    public BigfootAttackState(BigfootStateController controller) : base(controller) { }

    public override void Act()
    {
        BigfootStateController bcontroller = (BigfootStateController) controller;
        float distance = Vector3.Distance(bcontroller.MyPosition, bcontroller.PlayerPosition);

        bcontroller.Direction = (bcontroller.PlayerPosition - bcontroller.MyPosition).normalized;
        bcontroller.Aggression -= Time.deltaTime;

        if (!bcontroller.Attacking)
        {
            bcontroller.Move();
        }
        else if (distance < 1f)
        {
            bcontroller.Attack();
            bcontroller.Aggression--;
        }

        if (distance > bcontroller.AttackDistance / 2)
        {
            bcontroller.Aggression -= Time.deltaTime;
        }
    }

    public override void CheckTransitions()
    {
        BigfootStateController bcontroller = controller as BigfootStateController;
        float distance = Vector3.Distance(bcontroller.MyPosition, bcontroller.PlayerPosition);
        if (distance >= bcontroller.IdealStalkDistance)
        {
            bcontroller.SetState(new BigfootWanderState(bcontroller));
        }
        if (bcontroller.Aggression <= 0)
        {
            bcontroller.SetState(new RetreatState(bcontroller));
        }
    }

    public override void OnStateEnter()
    {
        controller.CurrentSpeed = controller.RunSpeed;
        controller.Aggression = ((BigfootStateController) controller).AggressionLimit;
    }

    public override void OnStateExit()
    {
        ((BigfootStateController) controller).TimeSinceAttackLeave = 0;
    }
}
