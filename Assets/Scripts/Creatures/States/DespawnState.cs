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
        Type type = controller.GetType();
        RandomSpawner spawner = GameObject.FindGameObjectWithTag("GameController").GetComponent<RandomSpawner>();

        if (type == typeof(AnimalStateController))
        {
            spawner.RepoolAnimal(controller.gameObject);
        }
        else if (type == typeof(WolfStateController))
        {
            spawner.RepoolWolf(controller.gameObject);
        }
        else if (type == typeof(BigfootStateController))
        {
            spawner.TeleportBigfoot();
        }
    }
}
