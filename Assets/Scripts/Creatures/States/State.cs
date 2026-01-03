/// <summary>
/// A template for individual states for creature state machines.
/// </summary>
public abstract class State
{
    protected CreatureStateMachine controller;

    /// <summary>
    /// Check transition states. If you want to change to a different state, 
    /// call <c>SetState()</c> on the controller (usually from this method).
    /// </summary>
    public abstract void CheckTransitions();
    /// <summary>
    /// Perform the state machine actions. Called during the <c>Update()</c>
    /// loop in the state machine.
    /// </summary>
    public abstract void Act();
    /// <summary>
    /// Called whenever a new state is entered. Can be used in place of a
    /// constructor, if the state requires new data to be initialized.
    /// </summary>
    public virtual void OnStateEnter() { }
    /// <summary>
    /// Called when a state is discarded before a new state is entered.
    /// </summary>
    public virtual void OnStateExit() { }

    public State(CreatureStateMachine controller)
    {
        this.controller = controller;
    }
}
