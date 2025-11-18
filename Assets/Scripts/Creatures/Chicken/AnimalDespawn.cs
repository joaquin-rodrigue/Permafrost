using UnityEngine;

public class AnimalDespawn : AnimalState
{
    public AnimalDespawn(AnimalStateController controller) : base(controller) { }

    public override void Act()
    {
        // none
    }

    public override void CheckTransitions()
    {
        if (controller.isActiveAndEnabled)
        {
            controller.SetState(new AnimalWander(controller));
        }
    }

    public override void OnStateEnter()
    {
        GameObject.Find("TerrainGenerator").GetComponent<RandomSpawner>().RepoolAnimal(controller.gameObject);
    }
}
