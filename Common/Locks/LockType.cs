
namespace LunaCommon.Locks
{
    /// <summary>
    /// Different types of locks
    /// </summary>
    public enum LockType
    {
        /// <summary>
        /// The asteroid lock is owned by only 1 player and it defines who spawns the asteroids
        /// </summary>
        Asteroid,

        /// <summary>
        /// The control lock specifies who controls a given vessel. 
        /// A user can have several control locks depending on the settings.
        /// </summary>
        Control,

        /// <summary>
        /// The update lock specifies who updates the position and definition of a given vessel. 
        /// A user can have several update locks.
        /// </summary>
        Update,

        /// <summary>
        /// The spectator lock specifies if a user is spectating a vessel or not.
        /// A vessel can have several spectators
        /// </summary>
        Spectator
    }
}
