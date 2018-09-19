using LmpCommon.Xml;
using System;
using System.Globalization;
using System.Xml;

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

        public VesselInfo(string xml)
        {
            var document = new XmlDocument();
            document.LoadXml(xml);

            var node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='pid']");
            if (node != null) Id = Guid.Parse(node.InnerText);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='name']");
            if (node != null) Name = node.InnerText;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='type']");
            if (node != null) Type = node.InnerText;
            
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='distanceTraveled']");
            if (node != null && double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var dist)) DistanceTravelled = dist;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='sit']");
            if (node != null) Situation = node.InnerText;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='lat']");
            if (node != null && double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat)) Lat = lat;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='lon']");
            if (node != null && double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var lon)) Lon = lon;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='alt']");
            if (node != null && double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var alt)) Alt = alt;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='SMA']");
            if (node != null && double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var sma)) SemiMajorAxis = sma;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='ECC']");
            if (node != null && double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var ecc)) Eccentricity = ecc;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='INC']");
            if (node != null && double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var inc)) Inclination = inc;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='LPE']");
            if (node != null && double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var lpe)) ArgumentOfPeriapsis = lpe;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='LAN']");
            if (node != null && double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var lan)) LongitudeOfAscendingNode = lan;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='MNA']");
            if (node != null && double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var mna)) MeanAnomaly = mna;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='EPH']");
            if (node != null && double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var eph)) Epoch = eph;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='REF']");
            if (node != null && int.TryParse(node.InnerText, out var refBody)) ReferenceBody = refBody;
        }
    }
}
