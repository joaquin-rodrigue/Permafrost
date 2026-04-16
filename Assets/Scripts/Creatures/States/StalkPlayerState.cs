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
            System.Type type = controller.GetType();
            if (type == typeof(WolfStateController))
            {
                controller.SetState(new WolfAttackState((WolfStateController) controller));
            }
        }
        if (distance >= controller.MaxStalkingDistance)
        {
            controller.SetState(new WanderState(controller));
        }
    }

    public override void OnStateEnter()
    {
        controller.CurrentSpeed = controller.StalkSpeed;
        if (controller.GetType() == typeof(WolfStateController))
        {
            controller.PlayAudio(6, 8);
        }
    }
}
