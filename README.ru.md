# HammerShell

[English version](README.md)

Небольшая .NET-библиотека для **shell-канала Valve Hammer Editor** (Source 1): из своего инструмента можно отправлять
команды правки в карту, открытую в запущенном Hammer, — создавать, менять и удалять сущности, править AI-ноды.

Канал встроен в Hammer: скрытое окно `Worldcraft_ShellMessageWnd` принимает текстовые команды через `WM_COPYDATA`.
Отправляющая сторона — открытый код Valve во всех Source SDK начиная с 2006 года
([`editor_sendcommand.cpp`](https://github.com/ValveSoftware/source-sdk-2013/blob/master/src/public/editor_sendcommand.cpp#L188));
им пользуется и сама игра — команды `map_edit` и `hammer_update_entity`. Эта библиотека — тот же отправитель на C# с
работой с сессиями, которая нужна на реальных сборках Hammer. Вынесена из [Hammer-MCP](https://github.com/EpicMorg/Hammer-MCP).

Только Windows; .NET 8 и .NET 10.

## Установка

```sh
dotnet add package HammerShell.Client
```

## Использование

Откройте карту в Hammer и один раз сохраните её. Понадобятся имя файла без расширения и `mapversion` из блока
`versioninfo` сохранённого `.vmf`.

```csharp
using System.Numerics;
using HammerShell.Client;

var channel = new ShellChannel();                               // первый Hammer с shell-окном
using var session = ShellSession.Begin(channel, "my_map", 12);  // имя карты и сохранённый mapversion
if (session is null)
    return;                                                     // нет Hammer или не совпали карта / версия

var at = new Vector3(0, 0, 64);
if (session.Send(ShellCommands.EntityCreate("info_target", at)) == ShellResult.Ok)
    session.Send(ShellCommands.EntitySetKeyValue("info_target", at, "targetname", "from_my_tool"));
// Dispose отправляет session_end. Карта не сохраняется — сохраните её в Hammer.
```

- `ShellChannel` находит окно и отправляет одну команду; `Send` возвращает `Ok`, `Rejected` (Hammer вернул 0) или
  `NotRunning` (окна нет). Если запущено несколько редакторов, передайте `new ShellChannel(() => hwnd)` с shell-окном
  нужного процесса.
- `ShellSession.Begin` закрывает сессию, которую оставил кто-то другой, затем пробует сохранённую версию, +1 и −1
  (после правок версия в памяти Hammer может уйти вперёд файла). Если ни одна не подошла, возвращает `null`.
- `ShellCommands` форматирует все команды точно как отправитель из SDK: `session_begin`, `session_end`,
  `map_check_version`, `entity_create`, `entity_delete`, `entity_set_keyvalue`, `entity_rotate_incremental`,
  `node_create`, `node_delete`, `nodelink_create`, `nodelink_delete`.

## Что канал умеет и чего нет

Сущность ищется по классу и origin. Канал только пишет: не читает карту, выделение и камеру, не создаёт браши и не
двигает сущности, а outputs, заданные как keyvalues, не видны на вкладке Outputs. Создание, удаление и поворот
сущностей отменяются в Hammer, изменения keyvalues — нет. Подробности, наблюдаемое поведение и в каких сборках Hammer
есть канал (все стоковые Hammer для Source 1, Hammer++ 8867+, Source Level Editor, Slammin' Hammer, Black Mesa — но не
Strata, Source 2 и редакторы GoldSrc):

- [Справочник по shell-каналу](https://github.com/EpicMorg/Hammer-MCP/blob/master/docs/shell-channel.ru.md)
- [Таблица поддержки редакторов](https://github.com/EpicMorg/Hammer-MCP/blob/master/docs/editors.ru.md)

Отправитель должен работать с тем же или более высоким уровнем целостности Windows, что и Hammer (UIPI).

## Сборка

```sh
dotnet build HammerShell.sln
dotnet test --solution HammerShell.sln
```

Релизы: тег `vX.Y.Z` — workflow `release` прогоняет тесты, собирает пакет, публикует его в NuGet и создаёт релиз на
GitHub.

## Лицензия

MIT, см. [LICENSE](LICENSE). Проект не связан с Valve. Hammer и Source — торговые марки Valve Corporation.
