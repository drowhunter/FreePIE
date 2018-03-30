namespace JonesCorp.Lookup
{
    /// <summary>
    ///     represents the state of the XArcade Driver
    ///     Stopped - xArcade Driver is not running and not opened
    ///     Opened - xArcade Driver has been opened but is not running
    ///     Running - xArcade Driver is running.
    /// </summary>
    public enum XState
    {
        Stopped,
        Opened,
        Running
    }
}