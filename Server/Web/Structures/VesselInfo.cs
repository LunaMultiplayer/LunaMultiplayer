using Server.System.Vessel.Classes;
using System;
using System.Globalization;

namespace Server.Web.Structures
{
    public class VesselInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public double DistanceTravelled { get; set; }
        public string Situation { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }

        public double SemiMajorAxis { get; set; }
        public double Eccentricity { get; set; }
        public double Inclination { get; set; }
        public double ArgumentOfPeriapsis { get; set; }
        public double LongitudeOfAscendingNode { get; set; }
        public double MeanAnomaly { get; set; }
        public double Epoch { get; set; }
        public int ReferenceBody { get; set; }

        public VesselInfo(Vessel vessel)
        {
            Id = Guid.Parse(vessel.Fields.GetSingle("pid").Value);
            Name = vessel.Fields.GetSingle("name").Value;
            Type = vessel.Fields.GetSingle("type").Value;

            if (double.TryParse(vessel.Fields.GetSingle("distanceTraveled").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dist))
                DistanceTravelled = dist;
            if (double.TryParse(vessel.Fields.GetSingle("lat").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat))
                Lat = lat;
            if (double.TryParse(vessel.Fields.GetSingle("lon").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var lon))
                Lon = lon;
            if (double.TryParse(vessel.Fields.GetSingle("distanceTraveled").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var alt))
                Alt = alt;
            if (double.TryParse(vessel.Orbit.GetSingle("SMA").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var sma))
                SemiMajorAxis = sma;
            if (double.TryParse(vessel.Orbit.GetSingle("ECC").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var ecc))
                Eccentricity = ecc;
            if (double.TryParse(vessel.Orbit.GetSingle("INC").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var inc))
                Inclination = inc;
            if (double.TryParse(vessel.Orbit.GetSingle("LPE").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var lpe))
                ArgumentOfPeriapsis = lpe;
            if (double.TryParse(vessel.Orbit.GetSingle("LAN").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var lan))
                LongitudeOfAscendingNode = lan;
            if (double.TryParse(vessel.Orbit.GetSingle("MNA").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var mna))
                MeanAnomaly = mna;
            if (double.TryParse(vessel.Orbit.GetSingle("EPH").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var eph))
                Epoch = eph;
            if (int.TryParse(vessel.Orbit.GetSingle("REF").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var refBody))
                ReferenceBody = refBody;
        }
    }
}
