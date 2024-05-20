public abstract class ObserverRespawners
{
    /// <summary>
    /// Abstract method for notifying every observer interested in the message to update itself
    /// </summary>
    /// <param name="concreteSubject">Subject which carries information about destroyed body parts</param>
    public abstract void Update(ISubject subject);

    /// <summary>
    /// Method for spawning flies which needs to be overriden
    /// </summary>
    public abstract void SpawnFlies();
}
