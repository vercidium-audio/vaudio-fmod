## Vercidium Audio + FMOD Example

This repository requires the Vercidium Audio SDK v1.1.1 and FMOD SDK v2.03 to run:
- Download the Vercidium Audio SDK from [vercidium.com/audio](https://vercidium.com/audio)
- Download the FMOD SDK from [fmod.com/download](https://fmod.com/download)

> Please note that neither SDK is free for commercial use. See [fmod.com/licensing](https://fmod.com/licensing)

## FMOD Setup

Once FMOD is installed, copy the `C:/Program Files (x86)/FMOD SoundSystem/FMOD Studio API Windows/api/core/lib/x64/fmod.dll` file to the `vaudio-fmod/lib` folder.

## Vercidium Audio Setup

You can either copy the entire vaudio folder to the `vaudio-fmod/lib` folder:

![screenshot of the vaudio-fmod/lib folder. It contains fmod.dll, and a folder called vaudio](docs/lib_folder.png)

Or edit `vaudio-fmod.csproj` to point to the folder where the Vercidium Audio SDK lives:

```xml
<PropertyGroup>
    <!-- Replace this with the path to your vaudio SDK -->
    <VAudioDir>lib\vaudio</VAudioDir>
</PropertyGroup>
```

## File Overview

- `fmod/*.cs` contains the C# bindings for FMOD, directly copied from the FMOD SDK
- `FMODSystem.cs` and `FMODSound.cs` are helper files for working with FMOD
- `resource/audio/speech.ogg` is an example file included for playback
- `Scene.cs` creates a Vercidium Audio context and initialises FMOD

Scene.cs is where you can adjust ray counts, add primitives, change materials and more. See the [Vercidium Audio docs](https://docs.vercidium.com/raytraced-audio/v110/Getting+Started) for more.

## Controls

Open the project in Visual Studio 2022 or 2026, and press F5 to run the project.

A debug window will appear that displays:
- the raytracing scene (primitives and rays)
- an echogram at the top
- raytracing stats in the bottom left.

Controls:

- Use WASD and the mouse to move the camera
- Press escape to release the mouse
- Press shift/control to increase camera speed

![Screenshot of the Vercidium Audio debug window, which shows primitives, rays, echograms and raytracing stats](docs/debug_window.png)