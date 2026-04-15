using UnityEngine;

// todo: needs SO MUCH WORK
public class BigfootAttackState : State
{
    public BigfootAttackState(BigfootStateController controller) : base(controller) { }

    public override void Act()
    {
        BigfootStateController bcontroller = (BigfootStateController) controller;
        if (bcontroller.IsHeRoaring) return;
        float distance = Vector3.Distance(bcontroller.MyPosition, bcontroller.PlayerPosition);
        Debug.Log(distance);
        bcontroller.Direction = (bcontroller.PlayerPosition - bcontroller.MyPosition).normalized;
        bcontroller.FaceDirection();
        bcontroller.Aggression -= Time.deltaTime;

        if (bcontroller.Attacking) return;
        
        bcontroller.Move();
        if (distance < 4f)
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
        ((BigfootStateController) controller).HeRoar();
    }

    public override void OnStateExit()
    {
        ((BigfootStateController) controller).TimeSinceAttackLeave = 0;
    }
}
