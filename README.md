#
# (G)raveyard
Semester Project (Feb 2022) - 3rd person action game - rhythm based combat
Team: Game Direction - 1, Development team - 4, Sound team - 2
Development time: 3 months
Target platforms: Windows, Mac (build not provided)

## About
The project contains third party assets and is therefore not for commercial use.
It was developed within the scope of a semester project as part of the study program Animation&Game at the Hochschule Darmstadt.
The official project documentation can be found [here](https://ag.mediencampus.h-da.de/wp-content/uploads/sites/31/2022/05/CatalougeWS2122_web.pdf).

## Installation & Start
The project can be build through the [Unity build settings](https://docs.unity3d.com/Manual/BuildSettings.html). No further setup needed.

**Required Scenes**
- 01_MainMenu ................0
- 02_LoadingScreen...........1
- 03_MainGame.................2

**Windows**
- You can download the build as .zip from [here](https://drive.google.com/file/d/1bzefD9Ny1B7jrw_80UMpbB7UpQsgcsMD/view).
- Unpack the .zip file
- Execute (G)raveyard.exe

**Controls**
**Mouse/Keyboard | Gamepad**
- WASD | left stick - move
- left mouse | button west - attack
- left shift | button east - dodge
- space | button south - jump
- esc | options - pause

- A full control scheme can be found in the main menu/settings/controls and the pause menu/settings/controls

### Primary gameplay systems, most important directories
- Audio Detection systems .../Assets/Scripts/AudioDetection/AudioSpectrumManager.cs
- Character controller systems .../Assets/Scripts/Characters
- Ability management .../Assets/Scripts/Abilities
- Enemy AI .../Assets/Scripts/StateMachines
