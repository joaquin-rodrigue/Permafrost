using UnityEngine;
using System;

public class DespawnState : State
{
    public DespawnState(CreatureStateMachine controller) : base(controller) { }

    public override void Act()
    {
        // empty
    }

    // type magic: the method
    public override void CheckTransitions()
    {
        if (controller.isActiveAndEnabled)
        {
            // switch based on controller type
            Type type = controller.GetType();
            if (type == typeof (BigfootStateController))
            {
                controller.SetState(new BigfootWanderState((BigfootStateController) controller));
            }
            else
            {
                controller.SetState(new WanderState(controller));
            }
        }
    }

    public override void OnStateEnter()
    {
        
    }
}
