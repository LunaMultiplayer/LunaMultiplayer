using LmpCommon.Message;
using LmpCommon.Message.Data.MasterServer;
using LmpMasterServer.Structure;
using System.Net;

namespace LmpMasterServerTest;

public class Tests
{
    private Server GetTestServer(IPEndPoint endpoint)
    {
        var messageFactory = new ServerMessageFactory();
        var initialData = messageFactory.CreateNewMessageData<MsRegisterServerMsgData>();
        initialData.Id = 999;
        initialData.ServerName = "Test Server";
        initialData.Cheats = false;
        initialData.CountryCode = "US";
        initialData.Description = "";
        initialData.GameMode = 0;
        initialData.Website = "https://lunamultiplayer.com";
        initialData.WebsiteText = "LMP";
        return new Server(initialData, endpoint);
    }

    [Test]
    public void TestSetCountryFromEndpoint()
    {
        var endpoint = IPEndPoint.Parse("8.8.8.8:8799");
        var server = GetTestServer(endpoint);
        server.Country = "";

        server.SetCountryFromEndpointAsync(endpoint).Wait();

        Assert.That(server.Country, Is.EqualTo("US"), "endpoint country is fetched correctly");
    }
}
