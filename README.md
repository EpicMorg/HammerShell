# HammerShell

[Русская версия](README.ru.md)

A small .NET library for the **shell channel of Valve Hammer Editor** (Source 1): send edit commands to the map that is
open in a running Hammer — create, change and delete entities, edit AI nodes — from your own tool.

The channel is built into Hammer: a hidden window, `Worldcraft_ShellMessageWnd`, takes text commands over
`WM_COPYDATA`. The sending side is public Valve code in every Source SDK since 2006
([`editor_sendcommand.cpp`](https://github.com/ValveSoftware/source-sdk-2013/blob/master/src/public/editor_sendcommand.cpp#L188));
the game itself uses it for `map_edit` and `hammer_update_entity`. This library is that sender in C#, with the session
handling that real Hammer builds need. It was extracted from [Hammer-MCP](https://github.com/EpicMorg/Hammer-MCP).

Windows only; .NET 8 and .NET 10.

## Install

```sh
dotnet add package EpicMorg.HammerShell.Client
```

## Use

Open a map in Hammer and save it once. You need its file name without the extension and the `mapversion` from the
`versioninfo` block of the saved `.vmf`.

```csharp
using System.Numerics;
using HammerShell.Client;

var channel = new ShellChannel();                               // first Hammer with a shell window
using var session = ShellSession.Begin(channel, "my_map", 12);  // map name and saved mapversion
if (session is null)
    return;                                                     // no Hammer, or the map / version does not match

var at = new Vector3(0, 0, 64);
if (session.Send(ShellCommands.EntityCreate("info_target", at)) == ShellResult.Ok)
    session.Send(ShellCommands.EntitySetKeyValue("info_target", at, "targetname", "from_my_tool"));
// Dispose sends session_end. The map is not saved - save it in Hammer.
```

- `ShellChannel` finds the window and sends one command; `Send` returns `Ok`, `Rejected` (Hammer returned 0) or
  `NotRunning` (no window). With several editors running, pass `new ShellChannel(() => hwnd)` with the shell window of
  the process you want.
- `ShellSession.Begin` closes a session left open by someone else, then tries the saved version, +1 and −1 (Hammer's
  version in memory can run ahead of the file after edits). It returns `null` if none is accepted.
- `ShellCommands` formats every command exactly like the SDK sender: `session_begin`, `session_end`,
  `map_check_version`, `entity_create`, `entity_delete`, `entity_set_keyvalue`, `entity_rotate_incremental`,
  `node_create`, `node_delete`, `nodelink_create`, `nodelink_delete`.

## What the channel can and cannot do

Entities are found by class name and origin. The channel only writes: it cannot read the map, the selection or the
camera, cannot create brushes or move entities, and outputs set as keyvalues do not show on the Outputs tab.
Creating, deleting and rotating entities can be undone in Hammer; keyvalue changes cannot. Details, observed behaviour
and which Hammer builds have the channel (every stock Source 1 Hammer, Hammer++ 8867+, Source Level Editor, Slammin'
Hammer, Black Mesa — not Strata, Source 2 or GoldSrc editors):

- [Shell channel reference](https://github.com/EpicMorg/HammerShell/blob/master/docs/shell-channel.md) (this repository)
- [Editor support matrix](https://github.com/EpicMorg/Hammer-MCP/blob/master/docs/editors.md)

The sender must run at the same or a higher Windows integrity level as Hammer (UIPI).

## Build

```sh
dotnet build HammerShell.sln
dotnet test --solution HammerShell.sln
```

Releases: push a tag `vX.Y.Z` — the `release` workflow tests, packs and publishes the package to NuGet and creates a
GitHub release.

## License

MIT, see [LICENSE](LICENSE). Not affiliated with Valve. Hammer and Source are trademarks of Valve Corporation.
