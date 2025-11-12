using UnityEngine;

public class AnimalWander : AnimalState
{
    public AnimalWander(AnimalStateController controller) : base(controller) { }

    public override void Act()
    {
        controller.timeSinceWanderCheck += Time.deltaTime;
        controller.movingTime -= Time.deltaTime;

        if (controller.timeSinceWanderCheck > controller.wanderTimeInterval)
        {
            controller.direction = new Vector2(Random.value - 0.5f, Random.value - 0.5f).normalized;
            controller.movingTime = Random.value * (controller.wanderTimeInterval - 1);
            controller.timeSinceWanderCheck = 0;
            controller.FaceDirection();
            //controller.direction = Vector3.forward;
        }
        if (controller.movingTime <= 0) controller.DontMove();
        else controller.Move();
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.transform.position, controller.PlayerPosition);
        if (distance <= controller.MinRunDistance)
        {
            controller.SetState(new AnimalFlee(controller));
        }
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}
