using UnityEngine;

public class WolfDespawn : WolfState
{
    public WolfDespawn(WolfStateController controller) : base(controller) { }

    public override void Act()
    {
        // none
    }

    public override void CheckTransitions()
    {
        if (controller.isActiveAndEnabled)
        {
            controller.SetState(new WolfWander(controller));
        }
    }

    public override void OnStateEnter()
    {
        GameObject.Find("TerrainGenerator").GetComponent<RandomSpawner>().RepoolWolf(controller.gameObject);
    }
}
