using System.Globalization;
using System.Numerics;
using System.Runtime.Versioning;
using HammerShell.Client;

namespace HammerShell.Client.Tests;

public class ShellCommandsTests
{
    private static readonly Vector3 At = new(0, -64.5f, 128);

    [Fact]
    public void SessionCommands() =>
        Assert.Equal(["session_begin my_map 12", "session_end", "map_check_version my_map 12"],
            [ShellCommands.SessionBegin("my_map", 12), ShellCommands.SessionEnd(), ShellCommands.MapCheckVersion("my_map", 12)]);

    [Fact]
    public void EntityCommandsLikeTheSdk()
    {
        Assert.Equal("entity_create info_target 0 -64.5 128", ShellCommands.EntityCreate("info_target", At));
        Assert.Equal("entity_delete info_target 0 -64.5 128", ShellCommands.EntityDelete("info_target", At));
        Assert.Equal("entity_set_keyvalue info_target 0.000000 -64.500000 128.000000 \"targetname\" \"door 1\"",
            ShellCommands.EntitySetKeyValue("info_target", At, "targetname", "door 1"));
        Assert.Equal("entity_rotate_incremental prop_dynamic 0.000000 -64.500000 128.000000 0.000000 15.000000 0.000000",
            ShellCommands.EntityRotateIncremental("prop_dynamic", At, new Vector3(0, 15, 0)));
    }

    [Fact]
    public void NodeCommands()
    {
        Assert.Equal("node_create info_node 7 0 -64.5 128", ShellCommands.NodeCreate("info_node", 7, At));
        Assert.Equal("node_delete 7", ShellCommands.NodeDelete(7));
        Assert.Equal("nodelink_create 7 8", ShellCommands.NodeLinkCreate(7, 8));
        Assert.Equal("nodelink_delete 7 8", ShellCommands.NodeLinkDelete(7, 8));
    }

    [Fact]
    public void NumbersUseADecimalPointInAnyCulture()
    {
        var saved = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            Assert.Equal("entity_create light 0 -64.5 128", ShellCommands.EntityCreate("light", At));
        }
        finally
        {
            CultureInfo.CurrentCulture = saved;
        }
    }

    [Fact]
    public void QuotesInKeyValuesAreRefused() =>
        Assert.Throws<ArgumentException>(() => ShellCommands.EntitySetKeyValue("info_target", At, "targetname", "a\"b"));
}

[SupportedOSPlatform("windows")]
public class ShellChannelTests
{
    [Fact]
    public void NoWindowMeansNotRunning() =>
        Assert.Equal(ShellResult.NotRunning, new ShellChannel(() => 0).Send("session_end"));

    [Fact]
    public void NoSessionWithoutAnEditor() =>
        Assert.Null(ShellSession.Begin(new ShellChannel(() => 0), "my_map", 1));
}
