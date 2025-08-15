# LoFTweaksPatcher

LoFTweaksPatcher is a small Windows Forms tool for Windows that replaces the
game's Assembly-CSharp.dll with a pre-modified file (tweak.dll) and writes a
simple JSON config under LocalLow.

This repo contains the patcher source so anyone can review and build it.

-------------------------------------------------------------------------------

## Building from source

### Requirements

Option A (recommended)
- Windows 10 or 11 (x64)
- Visual Studio 2022 with the ".NET desktop development" workload

Option B (command line)
- Windows 10 or 11 (x64)
- Visual Studio Build Tools 2022 with MSBuild and .NET Framework targeting packs

Note: Classic .NET Framework WinForms projects do not build with "dotnet build"
alone unless you have the VS build tools installed. Use MSBuild from the VS
Developer Command Prompt if you want CLI only.

### Steps (Visual Studio)

1) Clone the repo:
```sh
$ git clone https://github.com/EukkMaru/LoFTweaks.git
```

2) Open the solution in Visual Studio:
```
LoFTweaksPatcher_Client.sln
```

3) Set Configuration to "Release".

4) Build:
- Menu: Build -> Build Solution
- or press: Ctrl+Shift+B

5) The exe will be here:
```
LoFTweaksPatcher_Client\bin\Release\
```

6) Place your "tweak.dll" next to the exe before running the patcher.

### Steps (Command Line with MSBuild)

1) Open the "x64 Native Tools Command Prompt for VS 2022".

2) Clone the repo and build in Release:
```sh
$ git clone https://github.com/YOUR_USERNAME/LoFTweaksPatcher.git
$ cd LoFTweaksPatcher\LoFTweaksPatcher_Client
$ msbuild LoFTweaksPatcher_Client.sln /p:Configuration=Release
```

3) The exe will be here:
```
bin\Release\
```

4) Place "tweak.dll" next to the exe before running.

-------------------------------------------------------------------------------

## Running

- Keep LoFTweaksPatcher_Client.exe and tweak.dll in the same folder.
- Run the patcher, select your game folder (the one that contains
  "Layers Of FearSub.exe"), choose your options, and click Patch.
- Config is written to LocalLow\Bloober Team\Layers of Fear\[SteamID]\cfg.

To revert to vanilla: Steam -> Properties -> Installed Files -> Verify integrity.
