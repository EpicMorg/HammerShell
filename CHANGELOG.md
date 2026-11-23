# Changelog

## [0.1.0] - unreleased

First release: the shell channel client extracted from Hammer-MCP, package `EpicMorg.HammerShell.Client` (namespace `HammerShell.Client`).

- `ShellChannel`: finds `Worldcraft_ShellMessageWnd` (or a window you pass) and sends one command over
  `WM_COPYDATA` in the system ANSI code page; `Ok` / `Rejected` / `NotRunning`.
- `ShellSession`: closes a stale session, opens one for the saved `mapversion` (also +1 / −1), `session_end` on dispose.
- `ShellCommands`: every command of the Source SDK sender, numbers with a decimal point in any culture, double quotes
  in keyvalues refused.
- Targets .NET 8 and .NET 10, Windows only.
- `docs/shell-channel.md` (EN / RU): the shell channel reference, moved here from Hammer-MCP.
