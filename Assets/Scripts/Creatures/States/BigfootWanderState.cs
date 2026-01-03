using UnityEngine;

public class BigfootWanderState : State
{
    public BigfootWanderState(BigfootStateController controller) : base(controller) { }

    public override void Act()
    {
        // this feels a bit hacky, but I'll allow it...
        // I mean, the contructor above should throw if the state is applied to a non-bigfoot controller, so...
        BigfootStateController bcontroller = (BigfootStateController) controller; 

        bcontroller.TimeSinceAction += Time.deltaTime;
        bcontroller.TimeSinceAttackLeave += Time.deltaTime;
        bcontroller.TimeSinceLightCheck += Time.deltaTime;
        bcontroller.MovingTime -= Time.deltaTime;

        if (bcontroller.TimeSinceLightCheck > bcontroller.LightCheckInterval && bcontroller.TargetLightSource == null)
        {
            GameObject[] sources = GameObject.FindGameObjectsWithTag("Light Source");
            int choice = Random.Range(0, sources.Length);
            bcontroller.TargetLightSource = sources[choice].transform;
        }

        if (bcontroller.TimeSinceAction > bcontroller.MaxWanderTime)
        {
            bcontroller.Direction = (new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f) 
                + (bcontroller.MyPosition - bcontroller.TargetLightSource.position).normalized * 2)
                .normalized;
            bcontroller.MovingTime = Random.value * (controller.MaxWanderTime - 1);
            bcontroller.TimeSinceAction = 0;
            bcontroller.FaceDirection();
        }
        if (bcontroller.MovingTime <= 0) bcontroller.DontMove();
        //Debug.Log("MOOOOOOOOOVE");
        else bcontroller.Move();
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.MyPosition, controller.PlayerPosition);
        BigfootStateController bcontroller = (BigfootStateController) controller;
        if (distance <= bcontroller.DetectPlayerDistance && bcontroller.TimeSinceAttackLeave > bcontroller.AttackStateCooldown)
        {
            bcontroller.SetState(new BigfootStalkState(bcontroller));
        }
        if (distance >= bcontroller.DespawnDistance)
        {
            bcontroller.SetState(new DespawnState(bcontroller));
        }
    }

    public override void OnStateEnter()
    {
        controller.CurrentSpeed = controller.WalkSpeed;
    }

    public override void OnStateExit()
    {
        BigfootStateController bcontroller = (BigfootStateController) controller;
        bcontroller.TargetLightSource = controller.transform;
    }
}
