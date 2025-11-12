using UnityEngine;

public class WolfAttack : WolfState
{
    private Vector3 target;

    public WolfAttack(WolfStateController controller) : base(controller) { }

    public override void Act()
    {
        controller.aggression += Time.deltaTime;
        float distance = Vector3.Distance(
            new Vector3(controller.transform.position.x, 0, controller.transform.position.z), 
            new Vector3(target.x, 0, target.z));
        //Debug.Log(distance);
        if (distance <= 0.015f)
        {
            target = controller.PlayerPosition + new Vector3(Random.value * 2 - 1, 0, Random.value * 2 - 1).normalized * 2;
            //Debug.Log(target);
        }
        
        controller.direction = (target - controller.transform.position).normalized;
        //controller.FaceDirection();
        //controller.direction = Vector3.forward;
        if (controller.aggression > 1)
        {
            controller.Move();
        }
        if (controller.aggression > controller.AttackInterval)
        {
            target = controller.PlayerPosition;
            controller.direction = (target - controller.transform.position).normalized;

            controller.Attack();
            controller.aggression = 0;
        }
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.transform.position, controller.PlayerPosition);
        if (distance >= controller.MaxStalkDistance)
        {
            controller.SetState(new WolfWander(controller));
        }
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        controller.currentSpeed = controller.RunSpeed;
        target = controller.PlayerPosition + new Vector3(Random.value * 2 - 1, 0, Random.value * 2 - 1).normalized * 2;
    }
}
