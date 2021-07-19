using LmpCommon;
using LmpCommon.Enums;
using LmpGlobal;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using uhttpsharp;
using HttpResponse = uhttpsharp.HttpResponse;

namespace LmpMasterServer.Http.Handlers
{
    public class ServerListHandler : IHttpRequestHandler
    {
        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            context.Response = new HttpResponse(HttpResponseCode.Ok, await GetServerList(), false);
        }

        private static Task<string> GetServerList()
        {
            return Task.Run(() =>
            {
                var servers = Lidgren.MasterServer.ServerDictionary.Values.Select(s => (ServerInfo)s).ToArray();

                using (var stringWriter = new StringWriter())
                using (var writer = new HtmlTextWriter(stringWriter))
                {
                    RenderHead(writer);

                    writer.RenderBeginTag(HtmlTextWriterTag.H1); writer.Write($"Luna Multiplayer servers - Version: {LmpVersioning.CurrentVersion}"); writer.RenderEndTag();

                    writer.RenderBeginTag(HtmlTextWriterTag.H3);
                    writer.Write($"Servers: {servers.Length}");
                    writer.WriteBreak();
                    writer.Write($"Players: {servers.Sum(s => s.PlayerCount)}");
                    writer.RenderEndTag();

                    RenderServersTable(writer, servers);
                    RenderFooter(writer);

                    return stringWriter.ToString();
                }
            });
        }

        private static void RenderHead(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Head);

            writer.RenderBeginTag(HtmlTextWriterTag.Title);
            writer.Write("Luna Multiplayer servers");
            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
            writer.AddAttribute(HtmlTextWriterAttribute.Href, "css/style.css");
            writer.RenderBeginTag(HtmlTextWriterTag.Link);
            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Src, "js/jquery-latest.js");
            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Src, "js/jquery.metadata.min.js");
            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Src, "js/jquery.tablesorter.min.js");
            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Src, "js/lmp.js");
            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        private static void RenderFooter(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.P);
            writer.RenderBeginTag(HtmlTextWriterTag.Small);

            writer.AddAttribute(HtmlTextWriterAttribute.Href, RepoConstants.OfficialWebsite);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write("Luna Multiplayer");
            writer.RenderEndTag();

            writer.Write(" - ");

            writer.AddAttribute(HtmlTextWriterAttribute.Href, RepoConstants.RepoUrl);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write("Github repo");
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private static void RenderServersTable(HtmlTextWriter writer, ServerInfo[] servers)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "LmpTable");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "tablesorter");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.RenderBeginTag("thead");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("IPv4 Address"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("IPv6 Address"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("Country"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("Password"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("Name"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("Description"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("URL"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("Game mode"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("Players"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("Max players"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("Dedicated"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("Mod control"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("Terrain quality"); writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Th); writer.Write("Cheats"); writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.RenderBeginTag("tbody");
            foreach (var server in servers)
            {
                try
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode(server.ExternalEndpoint)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode(server.InternalEndpoint6)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode(server.Country)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode(server.Password)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode(server.ServerName)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode(server.Description)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (!string.IsNullOrEmpty(server.Website))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, server.Website);
                        writer.RenderBeginTag(HtmlTextWriterTag.A); writer.Write(HttpUtility.HtmlEncode(server.WebsiteText)); writer.RenderEndTag();
                    }
                    writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode((GameMode)server.GameMode)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode(server.PlayerCount)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode(server.MaxPlayers)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode(server.DedicatedServer)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode(server.ModControl)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode((TerrainQuality)server.TerrainQuality)); writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td); writer.Write(HttpUtility.HtmlEncode(server.Cheats)); writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            writer.RenderEndTag();

            writer.RenderEndTag();
        }
    }

}
