<p align="center"><img src="https://superfightersredux.tk/assets/LogoBig.png" align="center"><br><br></p>

# F.A.Q.
### What is it?
SFR is an open source mod for [SFD](https://mythologicinteractive.com/SuperfightersDeluxe). It adds new content, mechanics and tweaks exisiting game features for a whole new experience.

### How does it work?
We add new (regular) objects through `.sfdx` files. We also patch or add game code through reflection and [HarmonyLib](https://harmony.pardeike.net/) in order to add and/or modify exisitng mechanics that are present in SFD.

### Do you have devs consent?
Yes, we do. However read the [license](https://github.com/Odex64/SFR/blob/master/LICENSE.txt) for some clarification.

### Can I contribute or create my own mods?
Yes. Everyone with intermetiade coding knowledge can create his own mods or even contribute to the main mod branch if authorized, however you **must** read and include the same license as in this repository. You're advised to change SFR version in your own mods, to avoid conflicts with other servers.<br><br>

## Download
You can download this mod [here](https://github.com/Odex64/SFR/releases).<br><br><br>

# Developing your own mods
Here's some instructions for contributing or developing your own mods.
### Prerequisites
* [Visual Studio](https://visualstudio.microsoft.com/) with ".NET Desktop development" and .NET Framework 4.7.2 SDK installed.
* [dnSpy](https://github.com/dnSpyEx/dnSpy).
* [Git](https://git-scm.com/).
* Fork this repo.

**Clone** your forked repository and open its solution with Visual Studio, then wait for NuGet to install all the required dependencies.<br>

Right click on SFR project and choose properties; then change your configuration from `Active` to `All Configurations`.<br>
In the `Debug` section change your working directory to your SFD installation (by default `C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe`); and external program to `SFR.exe` (must be in the same directory).<br>
<img src="https://i.imgur.com/CalS6n4.png"><br>
**NOTE:** if you don't have a `SFR.exe` to select, create a dummy file and chose that as external program.<br>

If you have installed SFD in a another directory or drive, you must modify `build.bat` as well. You need to change `SFD` variable with your actual installation path.<br>

One last step is to create a `SFR` folder inside your Superfighters Deluxe installation, and manually copy `Core.dll` and `Content` folder from Superfighters Redux solution to the newly created folder.
Now in Visual Studio try to build the solution, if you don't see any errors you're good to go!<br>

You can open `Core.dll` with dnSpy in order to inspect SFD code. It is a slightly modified and non-obfuscated `Superfighters Deluxe.exe` assembly.

You can submit your changes to the main mod branch through a pull request.
