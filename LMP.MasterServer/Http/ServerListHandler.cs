using LunaCommon;
using LunaCommon.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using uhttpsharp;

namespace LMP.MasterServer.Http
{
    public class ServerListHandler : IHttpRequestHandler
    {
        public Task Handle(IHttpContext context, Func<Task> next)
        {
            context.Response = new HttpResponse(HttpResponseCode.Ok, GetServerList(), false);
            return Task.Factory.GetCompleted();
        }

        private static string GetServerList()
        {
            var servers = Lidgren.MasterServer.ServerDictionary.Values.Select(s => s.Info).ToArray();

            using (var stringWriter = new StringWriter())
            using (var writer = new HtmlTextWriter(stringWriter))
            {
                RenderHead(writer);

                writer.RenderBeginTag(HtmlTextWriterTag.H1); writer.Write($"Luna Multiplayer servers - Version: {LmpVersioning.CurrentVersion}"); writer.RenderEndTag();

                RenderServersTable(writer, servers);
                RenderFooter(writer);

                return stringWriter.ToString();
            }
        }

        private static void RenderHead(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Head);
            writer.RenderBeginTag(HtmlTextWriterTag.Title);
            writer.Write("Luna Multiplayer servers");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Style);
            writer.Write(Properties.Resources.style);
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private static void RenderFooter(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.P);
            writer.RenderBeginTag(HtmlTextWriterTag.Small);
            writer.Write("Luna Multiplayer - ");
            writer.AddAttribute(HtmlTextWriterAttribute.Href, "https://github.com/LunaMultiplayer/LunaMultiplayer");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write("Github repo");
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private static void RenderServersTable(HtmlTextWriter writer, ServerInfo[] servers)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Th);writer.Write("Address");writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th);writer.Write("Password");writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th);writer.Write("Name");writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th);writer.Write("Description");writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th);writer.Write("Game Mode");writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th);writer.Write("Players/Max");writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th);writer.Write("Mod control");writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th);writer.Write("Terrain quality");writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th);writer.Write("Cheats");writer.RenderEndTag();
            writer.RenderEndTag();

            // Loop over some strings.
            foreach (var server in servers)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);writer.Write(server.ExternalEndpoint);writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Td);writer.Write(server.Password);writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Td);writer.Write(server.ServerName);writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Td);writer.Write(server.Description);writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Td);writer.Write((GameMode)server.GameMode);writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Td);writer.Write($"{server.PlayerCount}/{server.MaxPlayers}");writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Td);writer.Write(server.ModControl);writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Td);writer.Write((TerrainQuality)server.TerrainQuality);writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Td);writer.Write(server.Cheats);writer.RenderEndTag();
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }
    }

}
