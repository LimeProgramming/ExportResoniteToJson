
# ExportResoniteToJson

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that allows exporting items as json, bson, 7zbson, and lz4bson files. This allows items to be backed up locally, as well as letting you edit normally inaccessible internals, such as arrays. Note that assets behave in weird ways and will only be linked to. 

Bson, 7zbson, and lz4bson files can be reimported into the game easily by anyone, without needing a mod. Json importing *should* work for people who have a local storage mod. 

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Place [ExportResoniteToJson-Standalone.dll](https://github.com/zkxs/ExportNeosToJson/releases/latest/download/ExportNeosToJson.dll) into your `nml_mods` folder **if** You *don't* use a local storage mod.
OR
2. Place [ExportResoniteToJson-LocalStorage.dll](https://github.com/zkxs/ExportNeosToJson/releases/latest/download/ExportNeosToJson.dll) into your `nml_mods` folder **if** You *do* use a local storage mod.
4. Start the game. If you want to verify that the mod is working you can check your Neos logs.

> [!Note]
> This is a fork of [ExportNeosToJson](https://github.com/zkxs/ExportNeosToJson) which was originally made by zkxs and updated for Resonite by Calamity Lime. Ever want to feel like a Resonite developer? just find and replace "Neos" with "Resonite", lol there's more to it then that but it felt that way at times.


## What does this actually do?
It injects additional json, bson, 7zbson, and lz4bson options into the export dialog and restores importing json file into the game.