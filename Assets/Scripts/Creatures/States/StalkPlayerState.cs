using UnityEngine;

public class StalkPlayerState : State
{
    public StalkPlayerState(CreatureStateMachine controller) : base(controller) { }

    public override void Act()
    {
        controller.Direction = (controller.PlayerPosition - controller.MyPosition).normalized;
        controller.FaceDirection();
        controller.Move();
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.transform.position, controller.PlayerPosition);
        if (distance <= controller.AttackDistance)
        {
            // TODO: switch
        }
        if (distance >= controller.MaxStalkingDistance)
        {
            // TODO: switch
        }
    }

    public override void OnStateEnter()
    {
        controller.CurrentSpeed = controller.StalkSpeed;
    }
}
