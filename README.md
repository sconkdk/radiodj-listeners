# RadioDJ Listener Count Importer

A RadioDJ v3 plugin that polls a URL for the current listener count on a timer and
writes the value to a text file, so other tools (now-playing displays, overlays,
websites) can read it.

## What it does

- Every *N* minutes (default 5), fetches a URL that returns the listener count as a
  plain number.
- Writes that number to a text file (default `C:\RadioDJv3\listener.txt`), replacing
  the file atomically so readers never see a half-written value.
- On any failure (network error, unreachable host, unexpected response) it logs to
  RadioDJ's debug output and leaves the file untouched — the last good value is kept
  instead of being overwritten with garbage.
- URL, poll interval, and output path are all editable from an in-app settings dialog
  and persist across restarts.

## Requirements

- RadioDJ v3, installed at `C:\RadioDJv3` (or adjust the paths below to match your
  install).
- [.NET SDK](https://dotnet.microsoft.com/) to build (targets `net48` / .NET
  Framework 4.8, matching RadioDJ itself).

## Project layout

```
ListenerCountPlugin.csproj        Main plugin project (builds Plugin_ListenerCount.dll)
PluginClass.cs                    IPlugin implementation: polling, settings, timer
ConfigForm.cs                     Settings dialog (URL / interval / output path)
lib/PluginInterfaceStub/          Compile-time-only stand-in for RadioDJ's PluginInterface.dll
```

### Why the stub project?

RadioDJ's own `PluginInterface.dll` has a corrupted strong-name signature (left over
from obfuscation) that the C# compiler refuses to read as a reference
(`CS0009: Invalid public key`), even though RadioDJ itself loads it at runtime without
any problem. `lib/PluginInterfaceStub` is a small hand-written assembly with the same
name and version and the same `rdjInterface.IPlugin` / `IHost` member signatures, used
only so the plugin can be compiled. It is never deployed — at runtime the plugin binds
to RadioDJ's real `PluginInterface.dll`, exactly as if the stub didn't exist.

## Build

```
dotnet build lib\PluginInterfaceStub\PluginInterfaceStub.csproj -c Release
dotnet build -c Release
```

The output is `bin\Release\net48\Plugin_ListenerCount.dll`.

## Install

1. Close RadioDJ (it locks the plugin DLL while running).
2. Copy `Plugin_ListenerCount.dll` (and `.pdb`, optional) into
   `C:\RadioDJv3\Plugins\`.
3. Start RadioDJ. The plugin appears in the Plugins list as **Listener Count
   Importer**.

## Configuration

Open the plugin's config screen from RadioDJ's Plugins list (gear/config icon) to set:

| Field                  | Default                                   |
|-------------------------|--------------------------------------------|
| URL                    | `http://localhost/listener.txt`     |
| Poll interval (minutes) | `5`                                        |
| Output file             | `C:\RadioDJv3\listener.txt`               |

Clicking **Save** validates the URL and applies the new interval immediately (no
restart needed). Settings are stored in RadioDJ's standard per-plugin settings file
alongside the DLL.

## Troubleshooting

- **File not updating**: open RadioDJ's debug/log window — fetch and write failures
  are logged there with the URL/path involved.
- **Plugin DLL won't overwrite on update**: RadioDJ has it loaded; close RadioDJ
  first, then copy the new DLL, then relaunch.
