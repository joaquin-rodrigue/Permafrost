using UnityEngine;

public class BigfootStalkState : State
{
    public BigfootStalkState(BigfootStateController controller) : base(controller) { }

    public override void Act()
    {
        BigfootStateController bcontroller = controller as BigfootStateController;
        bcontroller.Direction = (bcontroller.PlayerPosition - bcontroller.MyPosition).normalized;
        bcontroller.FaceDirection();
        float distance = Vector3.Distance(bcontroller.MyPosition, bcontroller.PlayerPosition);
        Vector3 rotationDiff = (bcontroller.PlayerRotation - bcontroller.ModelRotation);
        Debug.Log("foot" + rotationDiff);
        float diff = Mathf.Abs(rotationDiff.y);
        Debug.Log(diff);

        if (distance > bcontroller.IdealStalkDistance || diff < bcontroller.BehindPlayerDifference)
        {
            bcontroller.Move();
            if (Random.value > 0.023f) bcontroller.PlayAudio(0, 4);
        }
    }

    public override void CheckTransitions()
    {
        BigfootStateController bcontroller = controller as BigfootStateController;
        float distance = Vector3.Distance(bcontroller.MyPosition, bcontroller.PlayerPosition);
        Vector3 rotationDiff = (bcontroller.PlayerRotation - bcontroller.ModelRotation);
        float diff = Mathf.Abs(rotationDiff.x) + Mathf.Abs(rotationDiff.y) + Mathf.Abs(rotationDiff.z);

        if (distance <= bcontroller.AttackDistance && diff < bcontroller.PlayerStaringDifference)
        {
            controller.SetState(new BigfootAttackState(bcontroller));
        }
        if (distance >= bcontroller.MaxStalkingDistance)
        {
            controller.SetState(new BigfootWanderState(bcontroller));
        }
    }

    public override void OnStateEnter()
    {
        controller.CurrentSpeed = controller.StalkSpeed;
    }
}
