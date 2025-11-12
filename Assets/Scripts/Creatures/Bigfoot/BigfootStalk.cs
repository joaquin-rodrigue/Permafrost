using UnityEngine;

public class BigfootStalk : BigfootState
{
    public BigfootStalk(BigfootStateController controller) : base(controller) { }

    public override void Act()
    {
        controller.direction = (controller.PlayerPosition - controller.transform.position).normalized;
        controller.FaceDirection();
        //controller.direction = Vector3.forward;
        float distance = Vector3.Distance(controller.transform.position, controller.PlayerPosition);
        Vector3 rotationDiff = (controller.PlayerRotation - controller.ModelRotation);
        Debug.Log("foot" + rotationDiff);
        float diff = Mathf.Abs(rotationDiff.y);
        Debug.Log(diff);

        if (distance > controller.IdealStalkDistance || diff < controller.BehindPlayerDifference)
        {
            controller.Move();
        }
    }

    public override void CheckTransitions()
    {
        float distance = Vector3.Distance(controller.transform.position, controller.PlayerPosition);
        Vector3 rotationDiff = (controller.PlayerRotation - controller.transform.rotation.eulerAngles);
        float diff = Mathf.Abs(rotationDiff.x) + Mathf.Abs(rotationDiff.y) + Mathf.Abs(rotationDiff.z);

        if (distance <= controller.AttackDistance && diff < controller.PlayerStaringDifference)
        {
            //controller.SetState(new BigfootAttack(controller));
        }
        if (distance >= controller.MaxStalkDistance)
        {
            controller.SetState(new BigfootWander(controller));
        }
    }

    public override void OnStateEnter()
    {
        controller.currentSpeed = controller.StalkSpeed;
    }
}
