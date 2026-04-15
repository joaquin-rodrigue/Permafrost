using UnityEngine;

public class WanderState : State
{
    public WanderState(CreatureStateMachine controller) : base(controller) { }

    public override void Act()
    {
        controller.TimeSinceAction += Time.deltaTime;
        controller.MovingTime -= Time.deltaTime;

        if (controller.TimeSinceAction > controller.MaxWanderTime)
        {
            controller.Direction = new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f).normalized;
            controller.MovingTime = Random.value * (controller.MaxWanderTime - 1);
            controller.TimeSinceAction = 0;
            controller.FaceDirection();
            if (Random.value > 0.5f) controller.PlayAudio(0, 5);
        }
        if (controller.MovingTime <= 0) controller.DontMove();
        else controller.Move();
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.MyPosition, controller.PlayerPosition);

        if (distance <= controller.DetectPlayerDistance)
        {
            // switch based on controller type
            System.Type type = controller.GetType();
            if (type == typeof(AnimalStateController))
            {
                controller.SetState(new RetreatState(controller));
            }
            else if (type == typeof(WolfStateController))
            {
                controller.SetState(new StalkPlayerState(controller));
            }
        }
        if (distance >= controller.DespawnDistance)
        {
            controller.SetState(new DespawnState(controller));
        }
    }

    public override void OnStateEnter()
    {
        controller.CurrentSpeed = controller.WalkSpeed;
    }
}
