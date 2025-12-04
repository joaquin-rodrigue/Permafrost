using UnityEngine;

public class AnimalFlee : AnimalState
{
    public AnimalFlee(AnimalStateController controller) : base(controller) { }

    public override void Act()
    {
        controller.direction = new Vector3(controller.transform.position.x - controller.PlayerPosition.x, controller.transform.position.z - controller.PlayerPosition.z, 0).normalized;
        controller.FaceDirection();
        //controller.direction = Vector3.forward * 3;
        controller.currentSpeed = controller.RunSpeed;
        controller.Move();
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.transform.position, controller.PlayerPosition);
        if (distance >= controller.MinRunDistance * 2)
        {
            controller.SetState(new AnimalWander(controller));
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
