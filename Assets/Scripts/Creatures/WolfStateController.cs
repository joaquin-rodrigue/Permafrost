using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Main state machine for wolves that spawn on the map.
/// </summary>
public class WolfStateController : CreatureStateMachine
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
