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
        throw new System.NotImplementedException();
    }

    public override void OnStateEnter()
    {
        GameObject.Find("TerrainGenerator").GetComponent<RandomSpawner>().DeadBigfoot();
    }
}
