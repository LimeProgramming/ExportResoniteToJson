> [!WARNING]
> It is unlikely that I will maintain this, if it breaks, it breaks.<br>If I find out that it's broken and cannot be arsed to fix it, I'll take down the repo at some stage; likely long after it has broken. 

# ExportResoniteToJson

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that allows exporting items as json, bson, 7zbson, and lz4bson files. This allows items to be backed up locally, as well as letting you edit normally inaccessible internals, such as arrays. Note that assets behave in weird ways and will only be linked to. 

Brson, 7zbson, and lz4bson files can be reimported into the game easily by anyone, without needing a mod. Json importing *should* work for people who have a local storage mod. 

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Place [ExportResoniteToJson.dll](https://github.com/LimeProgramming/ExportResoniteToJson/releases/download/2.1.4/ExportResoniteToJson.dll) into your `rml_mods` folder.
3. Start the game. If you want to verify that the mod is working you can check your Resonite logs.

> [!Note]
> This is a fork of [ExportNeosToJson](https://github.com/zkxs/ExportNeosToJson) which was originally made by zkxs and updated for Resonite by Calamity Lime.

## What does this actually do?
It injects additional json, bson, 7zbson, and lz4bson options into the export dialog and restores importing json file into the game.

## Known Issues
* The export window in Resonite is too small. If someone can be bothered to fix that, that would be great.
* I remember having to do special handling for Local mod support and I don't remember what it is, so might be broken, I don't know