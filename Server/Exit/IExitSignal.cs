using System;

namespace Server.Exit
{
    public interface IExitSignal
    {
        event EventHandler Exit;
    }
}
