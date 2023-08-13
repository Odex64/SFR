# Index
* [Setup](https://github.com/Odex64/SFR/blob/master/CONTRIBUTE.md#setup)
* [Speeding up loading times](https://github.com/Odex64/SFR/blob/master/CONTRIBUTE.md#speeding-up-loading-times)
* [Project structure](https://github.com/Odex64/SFR/blob/master/CONTRIBUTE.md#project-structure)



## Setup
You will need the following programs
* [Visual Studio](https://visualstudio.microsoft.com/) with ".NET Desktop development" and .NET Framework 4.7.2 SDK installed.
* [dnSpy](https://github.com/dnSpyEx/dnSpy).
* [Git](https://git-scm.com/).
* Fork this repo.

**Clone** your forked repository and open its solution with Visual Studio, then wait for NuGet to install all the required dependencies.

Right click on SFR project and choose properties; then change your configuration from `Active` to `All Configurations`.
In the `Debug` section change your working directory to your SFD installation (by default `C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe`); and external program to `SFR.exe` (must be in the same directory).

<img src="https://cdn.discordapp.com/attachments/1112069698071310386/1112071322558808144/image.png" />

**NOTE:** if you don't have a `SFR.exe` to select, create a dummy file and chose that as external program.

If you have installed SFD in another directory or drive, you must modify `build.bat` as well. You need to change `SFD` variable with your actual installation path.

<img src="https://cdn.discordapp.com/attachments/1112069698071310386/1112071888135528458/image.png" />

One last step is to create a `SFR` folder inside your Superfighters Deluxe installation, and manually copy `Core.dll` and `Content` folder from Superfighters Redux solution to the newly created folder.

<img src="https://cdn.discordapp.com/attachments/1112069698071310386/1112073584651808830/image.png" />

You can open `Core.dll` with dnSpy in order to inspect SFD code. It is a slightly modified and non-obfuscated `Superfighters Deluxe.exe` assembly.
Now in Visual Studio try to build the solution, if you don't see any errors you're good to go!



## Speeding up loading times
Fast builds & loading times is an important part of the development, especially if you have to frequently restart the game.

### Game settings
First of all, start SFD/SFR normally and low the settings as much as possible, also make sure to set music volume to 0 and port forwarding to manual.

<img src="https://cdn.discordapp.com/attachments/1112076185124483194/1112076185531326535/image.png" />
<img src="https://cdn.discordapp.com/attachments/1112076185124483194/1112076185824940125/image.png" />

### More settings
Now open `C:\Users\YOUR_USER\Documents\Superfighters Deluxe\config.ini` and set the following settings

```
WAIT_FOR_DEVICE=0
ENABLESCREENSHOTS=0
SHOW_MENU_GAME_PREVIEW=0
GAMEPAD_INPUT_DISABLED=1
HOST_GAME_IGNORE_SCRIPTS_WHILE_WAITING_FOR_PLAYERS=1
```

I also recommend to set
```TARGETFPS=144```
or higher if your screen has higher refresh rate, to avoid to use too much GPU.

### Startup arguments
You must use the following program arguments when developing mods `-SFR -SKIP`
It will start SFR without asking & skip updates.

If you want to start the game even faster and immediately load into a map inside map editor, you can use this special argument
`-DEBUG <PATH_TO_MAP_WITHOUT_EXTENSION>`

example: `-SFR -SKIP -DEBUG C:\Users\YOUR_USERNAME\Documents\Superfighters Deluxe\Maps\Custom\Debug`.

### Finally
If you want to maximise loading times and prevent Steam from starting you must crack the game. I won't discuss it here, but if you really want to do that you can DM me on Discord.



# Project structure
The SFR mod is composed by many files and folders, here's a general overview of the project
```
API/                          \ Additions and tweaks to the Scripting API itself.
├─ Sandbox.cs                   | Modification to the Scripting API in order to make SFD.ScriptEngine work in SFR.
├─ ScriptCompiler.cs            | Modification to scripts compilation so it support latest C# features.
Bootstrap/                    \ This folder contains all patches that used for starting SFR.
├─ Assets.cs                    | Change some paths and load all SFR assets.
Debug/                        \ Helpers for when debugging and developing mods.
├─ DebugStartup.cs              | -DEBUG argument which loads the game directly in map editor.
Editor/                       \ Patches that affect the map editor itself.
├─ MapData.cs                   | Get the "official" map token when loading a map - used to mark SFR maps as official.
Fighter/                      \ A bunch of classes & patches regarding the player.
├─ Jetpack/                     \ Jetpacks.
│  ├─ GenericJetpack.cs           | Generic jetpack implementation. All other jetpacks derive from this class.
│  ├─ JetpackType.cs              | Enum containing jetpack types.
│  ├─ JetpackHandler.cs           | Several patches to make jetpacks work.
├─ AnimHandler.cs               | Custom animations.
├─ DevHandler.cs                | Mark SFR team members and load their icon.
├─ ExtendedPlayer               | IMPORTANT class. It is used to extend the Player class and add new features to it.
├─ GadgetHandler.cs             | Handles HUD and visual effects.
├─ GoreHandler.cs               | Decapitations & more.
├─ NameIconHandler.cs           | Draw developer player icon, otherwise normal player icon.
├─ PlayerHandler.cs             | Modifications to the player movements, interactions and so on.
├─ StatusBarHandler.cs          | Draws player bars like health, energy, jetpack fuel and more.
Game/                         \ Patches that affect the whole game or all rounds.
├─ CommandHandler.cs            | Adds additional commands
├─ WorldHandler.cs/             | Patches and fixes that affect the game world.
Helper/                       \ Static helper classes and extension methods.
├─ Fighter.cs                   | Extension methods for Player and ExtendedPlayer classes.
├─ Logger.cs                    | Logger for printing message to the console.
├─ Math.cs                      | Math helper.
├─ Randomizer.cs                | Random numbers generator helper.
├─ Vector.cs                    | Vector helper.
Misc/                         \ Constants and other tweaks.
├─ Constants.cs                 | Constants.
├─ Tweaks.cs                    | Patches to C# assemblies.
Objects/                      \ Custom game objects.
├─ Animal/                      \ Animals implementation.
│  ├─ ObjectAnimal.cs             | Generic animal implementation. All the other animals derive from this class.
├─ ObjectsHandler.cs            | Handler used to load new objects and apply specific tweaks.
OnlineServices/               \  Patches to games browser and hosted game
├─ Browser.cs                   | Make latest SFD version yellow and minor stuff.
├─ Host.cs                      | Apply hosted game patches, like increasing game slots.
Projectiles/                  \ New projectiles for custom weapons
├─ Database.cs                  | Adds new projectile in the game.
├─ IExtendedProjectile.cs       | Interface for extending projectiles functionality.
Sync/                         \ Clients & Server synchronization. 
├─ Generic/                     \ Generic server data to sync any kind of information.
│  ├─ SyncFlag.cs                 | Special flags for when syncing clients & server, like newly spawned objects.
│  ├─ GenericData.cs              | Generic data.
│  ├─ GenericServerData.cs        | Contains read and write methods for transforming generic data in primivite types.
│  ├─ DataType.cs                 | Enum containing generic data type.
├─ SyncHandler.cs               | Patches to make generic data work.
UI/                           \ Custom UI elements like credits.
Weapons/                      \ Contains new weapons
├─ Handgun/                     \ Self explanatory.
├─ Makeshift/                   \ Weapons you find around the map like chairs, bottles and so on.
├─ Melee/                       \ Self explanatory.
├─ Others/                      \ Contains pick-up weapons like medkit or boosts. It can also contain special items like jetpacks.
├─ Rifles/                      \ Self explanatory.
├─ Thrown/                      \ Weapons like grenades, mines and so on.
├─ Database.cs                  | Loads new weapons in game.
├─ IExtendedWeapon.cs           | Interface to add additional functionality to weapons.
├─ ISharpMelee.cs               | Interface to apply a custom decapitation rate.
Program.cs                    | Entry point. Used to check for updates, start SFD and apply SFR patches.
```