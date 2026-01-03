using UnityEngine;

/// <summary>
/// Main state machine for animals that spawn on the map.
/// </summary>
public class AnimalStateController : CreatureStateMachine
{
    #region Unity Methods
    protected new void Awake()
    {
        base.Awake();
        SetState(new WanderState(this));
    }
    #endregion

    #region State Machine Methods
    //empty
    #endregion
}
