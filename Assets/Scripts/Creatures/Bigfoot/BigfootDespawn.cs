using UnityEngine;

public class BigfootDespawn : BigfootState
{
    public BigfootDespawn(BigfootStateController controller) : base(controller) { }

    public override void Act()
    {
        // none
    }

    public override void CheckTransitions()
    {
        if (controller.isActiveAndEnabled)
        {
            controller.SetState(new BigfootWander(controller));
        }
    }

    public override void OnStateEnter()
    {
        GameObject.Find("TerrainGenerator").GetComponent<RandomSpawner>().TeleportBigfoot();
    }
}
