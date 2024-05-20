public interface ISubject
{
    /// <summary>
    /// Add observer to the subject's list
    /// </summary>
    /// <param name="observerRespawner">Observer</param>
    public void AddObserver(ObserverRespawners observerRespawner);
    /// <summary>
    /// Remove observer from subject's list
    /// </summary>
    /// <param name="observerRespawner">Observer</param>
    public void RemoveObserver(ObserverRespawners observerRespawner);
    /// <summary>
    /// Notify observers that a body part was destroyed
    /// </summary>
    public void Notify();
    /// <summary>
    /// Get current number of destroyed body parts
    /// </summary>
    /// <returns>Current number of destroyed body parts</returns>
    public int GetState();
    /// <summary>
    /// Notify observers to spawn new flies
    /// </summary>
    public void Spawn();
}
