using LmpClient.Events.Base;
using System;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class EvaEvent : LmpBaseEvent
    {
        public static EventData<Vessel> onCrewEvaReady;
        public static EventData<Guid, string, Vessel> onCrewEvaBoarded;
    }
}
