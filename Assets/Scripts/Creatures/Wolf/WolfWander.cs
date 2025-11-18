using UnityEngine;

public class WolfWander : WolfState
{
    public WolfWander(WolfStateController controller) : base(controller) { }

    public override void Act()
    {
        controller.timeSinceWanderCheck += Time.deltaTime;
        controller.movingTime -= Time.deltaTime;

        if (controller.timeSinceWanderCheck > controller.MaxWanderTime)
        {
            controller.direction = new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f).normalized;
            controller.movingTime = Random.value * (controller.MaxWanderTime - 1);
            controller.timeSinceWanderCheck = 0;
        }
        if (controller.movingTime <= 0) controller.DontMove();
        //Debug.Log("MOOOOOOOOOVE");
        else controller.Move();
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.transform.position, controller.PlayerPosition);
        if (distance <= controller.DetectPlayerDistance)
        {
            controller.SetState(new WolfStalk(controller));
        }
        if (distance >= controller.DespawnDistance)
        {
            controller.SetState(new WolfDespawn(controller));
        }
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        controller.currentSpeed = controller.MoveSpeed;
    }
}
