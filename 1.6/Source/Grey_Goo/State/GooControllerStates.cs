namespace Grey_Goo.State;

/// <summary>
/// All possible high-level operational states for the Grey Goo controller.
/// </summary>
public enum GooControllerStates: byte
{
    /// <summary>
    /// Inactive: spread halted; typically immediately after creation or after shutdown.
    /// </summary>
    Offline,
    /// <summary>
    /// Warming up and seeding the first tile; limited spread multiplier.
    /// </summary>
    Initialising,
    /// <summary>
    /// Normal operation and spreading at baseline rate.
    /// </summary>
    Online,
    /// <summary>
    /// Temporary boost to spread (e.g., after an event); reverts to <see cref="Online"/> later.
    /// </summary>
    Boosted,
    /// <summary>
    /// Temporary reduction to spread (e.g., from counters); can transition out to other states.
    /// </summary>
    Diminished
}
