using UnityEngine;

public class WolfAttackState : State
{
    private Vector3 target;

    public WolfAttackState(WolfStateController controller) : base(controller) { }

    public override void Act()
    {
        controller.Aggression += Time.deltaTime;
        float distance = Vector3.Distance(
            new Vector3(controller.MyPosition.x, 0, controller.MyPosition.z), 
            new Vector3(target.x, 0, target.z));
        //Debug.Log(distance);
        if (distance <= 0.015f)
        {
            target = controller.PlayerPosition + new Vector3(Random.value * 2 - 1, 0, Random.value * 2 - 1).normalized * 2;
            //Debug.Log(target);
        }
        
        controller.Direction = (target - controller.MyPosition).normalized;
        //controller.FaceDirection();
        //controller.direction = Vector3.forward;
        if (controller.Aggression > 1)
        {
            controller.Move();
        }
        else
        {
            controller.DontMove();
        }
        if (controller.Aggression > controller.AttackInterval)
        {
            target = controller.PlayerPosition;
            controller.Direction = (target - controller.MyPosition).normalized;

            controller.Attack();
            controller.Aggression = 0;
        }
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.MyPosition, controller.PlayerPosition);
        if (distance >= controller.MaxStalkingDistance)
        {
            controller.SetState(new WanderState(controller));
        }
    }

    public override void OnStateEnter()
    {
        controller.CurrentSpeed = controller.RunSpeed;
        target = controller.PlayerPosition + new Vector3(Random.value * 2 - 1, 0, Random.value * 2 - 1).normalized * 2;
    }
}
