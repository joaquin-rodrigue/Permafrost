/// <summary>
/// Individual states for the Animal state machine.
/// </summary>
public abstract class AnimalState
{
    protected AnimalStateController controller;

    /// <summary>
    /// Check transistion states. If you want to change to a different state, call <c>SetState()</c> on the controller.
    /// </summary>
    public abstract void CheckTransitions();
    /// <summary>
    /// Perform the state machine actions.
    /// </summary>
    public abstract void Act();
    /// <summary>
    /// Called whenever a new state is entered. Can be used in place of a constructor.
    /// </summary>
    public virtual void OnStateEnter() { }
    /// <summary>
    /// Called when a state is discarded before a new state is entered.
    /// </summary>
    public virtual void OnStateExit() { }

    public AnimalState(AnimalStateController controller)
    {
        this.controller = controller;
    }
}
