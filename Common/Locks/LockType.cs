
namespace LunaCommon.Locks
{
    /// <summary>
    /// Different types of locks
    /// </summary>
    public enum LockType
    {
        /// <summary>
        /// The contract lock is owned by only 1 player and it defines who can generate new contracts.
        /// </summary>
        Contract,
        
        /// <summary>
        /// The asteroid lock is owned by only 1 player and it defines who spawns the asteroids
        /// </summary>
        Asteroid,

        /// <summary>
        /// The kerbal lock specifies if a user is the owner of a kerbal.
        /// </summary>
        Kerbal,

        /// <summary>
        /// The spectator lock specifies if a user is spectating a vessel or not.
        /// A vessel can have several spectators
        /// </summary>
        Spectator,

        /// <summary>
        /// The update lock specifies who updates the position and definition of a given vessel. 
        /// A user can have several update/UnloadedUpdate locks.
        /// You get the UnloadedUpdate lock when there are vessels far away from other players and nobody is close enought
        /// to get the "Update" lock. If a player get close to the vessel and gets the "Update" lock, he will steal the 
        /// "UnloadedUpdate" lock from you
        /// </summary>
        UnloadedUpdate,

        /// <summary>
        /// The update lock specifies who updates the position and definition of a given vessel. 
        /// A user can have several update locks. 
        /// If you are controlling a vessel you also get the update lock.
        /// If you are in LOADING distance of a vessel that nobody controls and nobody already has the update lock you get it
        /// If you own the update lock you also own the "UnloadedUpdate" lock
        /// </summary>
        Update,

        /// <summary>
        /// The control lock specifies who controls a given vessel. 
        /// A user can have several control locks depending on the settings.
        /// </summary>
        Control,        
    }
}
