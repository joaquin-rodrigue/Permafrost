using Unity.VisualScripting;
using UnityEngine;

public class BigfootWander : BigfootState
{
    public BigfootWander(BigfootStateController controller) : base(controller) { }

    public override void Act()
    {
        controller.timeSinceWanderCheck += Time.deltaTime;
        controller.timeSinceAttackLeave += Time.deltaTime;
        controller.timeSinceLightCheck += Time.deltaTime;
        controller.movingTime -= Time.deltaTime;

        if (controller.timeSinceLightCheck > controller.LightCheckInterval && controller.TargetLightSource == null)
        {
            GameObject[] sources = GameObject.FindGameObjectsWithTag("Light Source");
            int choice = Random.Range(0, sources.Length);
            controller.TargetLightSource = sources[choice].transform;
        }

        if (controller.timeSinceWanderCheck > controller.MaxWanderTime)
        {
            controller.direction = (new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f) + (controller.transform.position - controller.TargetLightSource.position).normalized).normalized;
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
        if (distance <= controller.DetectPlayerDistance && controller.timeSinceAttackLeave > controller.AttackStateCooldown)
        {
            controller.SetState(new BigfootStalk(controller));
        }
    }

    public override void OnStateEnter()
    {
        controller.currentSpeed = controller.MoveSpeed;
        controller.TargetLightSource = controller.transform;
    }
}
