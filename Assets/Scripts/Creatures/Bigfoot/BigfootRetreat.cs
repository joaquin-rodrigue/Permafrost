using UnityEngine;

public class BigfootRetreat : BigfootState
{
    public BigfootRetreat(BigfootStateController controller) : base(controller) { }

    public override void Act()
    {
        controller.aggression -= Time.deltaTime;
        controller.direction = (controller.transform.position - controller.PlayerPosition).normalized;

        controller.Move();
    }

    public override void CheckTransitions()
    {
        if (controller.aggression <= 0)
        {
            controller.SetState(new BigfootWander(controller));
        }
    }

    public override void OnStateEnter()
    {
        controller.currentSpeed = controller.RunSpeed;
        controller.aggression = controller.AggressionLimit;
    }
}
