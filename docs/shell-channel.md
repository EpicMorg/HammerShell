# Hammer shell channel

[Русская версия](shell-channel.ru.md) · [HammerShell](../README.md) · [Editor support matrix](https://github.com/EpicMorg/Hammer-MCP/blob/master/docs/editors.md)

Source 1 Hammer has a small built-in remote control: a hidden window that accepts text commands from other programs
and applies them to the open map — create, change and delete entities, edit AI nodes. Valve uses it so that a running
game can push changes back into the editor. HammerShell (and [Hammer-MCP](https://github.com/EpicMorg/Hammer-MCP), where it comes from) use the same channel to make small edits to the
map that is open in Hammer, without patching the editor.

## Where it comes from

- **The sending side is public Valve code.** Every public Source SDK since 2006 (Episode One, Orange Box, Alien
  Swarm, 2013) ships `public/editor_sendcommand.cpp`, which finds the window and sends the commands
  ([SDK 2013 source][sdk-sender]).
- **The game uses it.** `game/server/wcedit.cpp` ([source][sdk-game]) implements the console commands `map_edit` and
  `hammer_update_entity`, which send entity positions and angles from the running game to Hammer; the AI network code
  (`ai_networkmanager.cpp`, `ai_dynamiclink.cpp`) sends node edits the same way.
- **Hammer's side was never published.** No public SDK contains the editor's handler, so everything below about how
  Hammer reacts comes from the SDK's sender and from testing real Hammer builds.
- In the older engine branches (2006/2007 up to Left 4 Dead 2 and Portal 2) the launcher can also load Hammer into the
  game process with `-edit`; on the current Left 4 Dead 2 `hammer_dll.dll` does get loaded, but no longer starts
  properly. The 2013 branch dropped that path; the shell channel stayed in the standalone Hammer.

## Sending a command

1. Find the window whose class name and title are both `Worldcraft_ShellMessageWnd` (`FindWindow`). With several
   editors running, enumerate the windows and pick the one that belongs to the editor's process.
2. Encode one command as a NUL-terminated byte string in the system ANSI code page. Numbers use a decimal point.
3. Send it with `SendMessage(hwnd, WM_COPYDATA, 0, &cds)`, where `cds.dwData = 0`, `cds.cbData` is the byte count
   including the NUL and `cds.lpData` points to the bytes.
4. A non-zero result means Hammer accepted the command; zero means it was rejected. There is no error text and no
   data in the reply.

`SendMessage` waits until Hammer has processed the command. The sender must run at the same or a higher Windows
integrity level as Hammer (UIPI), otherwise the message is dropped.

## Sessions and map version

Edits are only accepted inside a session that is bound to the open map:

```text
session_begin <mapname> <mapversion>
  ... edit commands ...
session_end
```

- `mapname` is the VMF file name without path and extension, `mapversion` the `versioninfo.mapversion` of the open
  map. Hammer rejects the session if they don't match, so save the map once after opening it and read the version
  from the file.
- The version in Hammer's memory can move ahead of the file: after edits made through the channel the next session
  may need `mapversion + 1`. `ShellSession.Begin` tries the saved version, then +1 and −1.
- Only one session can be open; a second `session_begin` is rejected. A session left open by a crashed sender stays
  open, so a client should send `session_end` before its own `session_begin`.
- `map_check_version <mapname> <mapversion>` checks the binding; in our tests it only succeeds inside a session.
- Closing a session does not save the map. Saving with Ctrl+S during a session keeps the session open.

## Commands

| Command | Arguments | Effect |
| --- | --- | --- |
| `session_begin` | `mapname mapversion` | open an edit session for the open map |
| `session_end` | | close it |
| `map_check_version` | `mapname mapversion` | check that the session matches the map |
| `entity_create` | `classname x y z` | create a point entity |
| `entity_delete` | `classname x y z` | delete the entity of that class at that origin |
| `entity_set_keyvalue` | `classname x y z "key" "value"` | set a keyvalue (also on brush entities, addressed by the centre of their brushes) |
| `entity_rotate_incremental` | `classname x y z pitch yaw roll` | rotate by the given angles, in degrees |
| `node_create` | `nodeclass id x y z` | create an AI node |
| `node_delete` | `id` | delete an AI node |
| `nodelink_create`, `nodelink_delete` | `start_id end_id` | link or unlink two AI nodes |

Entities are found by class name and origin. Two entities of the same class at the same spot cannot be told apart.
Keys and values are quoted and have no escaping, so they must not contain `"`.

## What it cannot do

- Read anything: no map data, no selection, no camera. Tools such as Hammer-MCP read the saved VMF, Hammer's autosaves and, with
  the optional adapter, the selection and camera.
- Create or change brushes, or move entities.
- Outputs: setting them as keyvalues works in game, but they do not appear on the Outputs tab.
- Undo: creating, deleting and rotating entities can be undone in Hammer; keyvalue changes are not in the Undo
  history.
- Stock Hammer rotates only entities that have a model; for others set `angles`.
- Hammer's views are not redrawn until the editor gets focus again. Hammer++ does not mark the map as modified.
- Menu actions such as Save or Run Map are not shell commands; they are ordinary `WM_COMMAND` messages to the main
  window.

## Which editors have it

Every stock Source 1 Hammer examined (Episode One 2007 and later, x86 and x64), Hammer++ from build 8867, Source
Level Editor, Slammin' Hammer, Black Mesa and Nuclear Dawn. Not in Hammer++ up to 8866, Strata Hammer, Source 2 Hammer,
GoldSrc editors or J.A.C.K. Details per build: [editor support matrix](https://github.com/EpicMorg/Hammer-MCP/blob/master/docs/editors.md) and the `shell.present` field of
the [build registry](https://github.com/EpicMorg/Hammer-MCP/blob/master/src/registry/hammers.json).

## Using it from C#

Install [`EpicMorg.HammerShell.Client`](https://www.nuget.org/packages/EpicMorg.HammerShell.Client) — this repository.
`ShellChannel` sends one command, `ShellSession` handles the session and version rules above, `ShellCommands` formats
every command; see the [README](../README.md) for an example.

[sdk-sender]: https://github.com/ValveSoftware/source-sdk-2013/blob/master/src/public/editor_sendcommand.cpp#L188
[sdk-game]: https://github.com/ValveSoftware/source-sdk-2013/blob/master/src/game/server/wcedit.cpp
