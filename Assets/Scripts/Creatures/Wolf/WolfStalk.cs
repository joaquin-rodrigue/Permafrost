using UnityEngine;

public class WolfStalk : WolfState
{
    public WolfStalk(WolfStateController controller) : base(controller) { }

    public override void Act()
    {
        controller.direction = (controller.PlayerPosition - controller.transform.position).normalized;
        //controller.FaceDirection();
        //controller.direction = Vector3.forward;
        controller.Move();
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.transform.position, controller.PlayerPosition);
        if (distance <= controller.AttackDistance)
        {
            controller.SetState(new WolfAttack(controller));
        }
        if (distance >= controller.MaxStalkDistance)
        {
            controller.SetState(new WolfWander(controller));
        }
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        controller.currentSpeed = controller.StalkSpeed;
    }
}
