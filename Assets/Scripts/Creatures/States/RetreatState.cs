using UnityEngine;

public class RetreatState : State
{
    public RetreatState(CreatureStateMachine controller) : base(controller) { }

    public override void Act()
    {
        controller.Direction = (controller.MyPosition - controller.PlayerPosition).normalized;
        controller.FaceDirection();
        controller.Move();

        if (controller.GetType() == typeof(BigfootStateController))
        {
            // This just feels cursed but I guess this is what we get to do with types now
            ((BigfootStateController) controller).Aggression -= Time.deltaTime;
        }
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.MyPosition, controller.PlayerPosition);
        if (controller.GetType() == typeof(BigfootStateController))
        {
            BigfootStateController bcontroller = (BigfootStateController) controller;
            if (bcontroller.Aggression <= 0)
            {
                bcontroller.SetState(new BigfootWanderState(bcontroller));
            }
        }
        else if (distance >= controller.DetectPlayerDistance * 2)
        {
            controller.SetState(new WanderState(controller));
        }
        
    }

    public override void OnStateEnter()
    {
        controller.CurrentSpeed = controller.RunSpeed;
        if (controller.GetType() == typeof(BigfootStateController))
        {
            BigfootStateController bcontroller = (BigfootStateController) controller;
            bcontroller.Aggression = bcontroller.AggressionLimit;
        }
    }
}
