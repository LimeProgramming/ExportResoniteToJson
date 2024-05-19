using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace ExportResoniteToJson
{
    public class ExportResoniteToJson : ResoniteMod
    {
        public override string Name => "ExportResoniteToJson";
        public override string Author => "CalamityLime";
        public override string Version => "2.1.3";
        public override string Link => "https://github.com/LimeProgramming/ExportNeosToJson";


        // ------------------------------------------------------------------------------------------ //
        /* ========== Register Mod Config Data/Keys ========== */

        private static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EXPORT_JSON = new ModConfigurationKey<bool>(
            "Export Json Type", "Enable exprting to Json file type. Changing this setting requires a game restart to apply.", 
            () => true
        );

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EXPORT_BSON = new ModConfigurationKey<bool>(
            "Export Bson Type", "Enable exprting to Bson file type. Changing this setting requires a game restart to apply.", 
            () => false
        );

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EXPORT_7ZBSON = new ModConfigurationKey<bool>(
            "Export 7zbon Type", "Enable exprting to 7zbon file type. Changing this setting requires a game restart to apply.",
            () => false
        );

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EXPORT_LZ4BSON = new ModConfigurationKey<bool>(
            "Export LZ4BSON Type", "Enable exprting to LZ4BSON file type. Changing this setting requires a game restart to apply.", 
            () => false
        );



        // ------------------------------------------------------------------------------------------ //
        /* ========== Our Hook into the game ========== */

        public override void OnEngineInit()
        {
            Config = GetConfiguration(); //Get this mods' current ModConfiguration

            Harmony harmony = new Harmony("net.CalamityLime.ExportResoniteToJson");
            FieldInfo formatsField = AccessTools.DeclaredField(typeof(ModelExportable), "formats");
            if (formatsField == null)
            {
                Error("could not read ModelExportable.formats");
                return;
            }

            // inject addional formats
            List<string> modelFormats = new List<string>((string[])formatsField.GetValue(null));

            if (Config.GetValue(EXPORT_JSON))
            {
                modelFormats.Add("JSON");
            }

            if (Config.GetValue(EXPORT_BSON))
            {
                modelFormats.Add("BSON");
            }

            if (Config.GetValue(EXPORT_7ZBSON))
            {
                modelFormats.Add("7ZBSON");
            }

            if (Config.GetValue(EXPORT_LZ4BSON))
            {
                modelFormats.Add("LZ4BSON");
            }


            if ( (Config.GetValue(EXPORT_JSON)) || (Config.GetValue(EXPORT_BSON)) || (Config.GetValue(EXPORT_7ZBSON)) || !(Config.GetValue(EXPORT_LZ4BSON)) )
            {
                formatsField.SetValue(null, modelFormats.ToArray());
            }


            // ---------- Patch the Export Model function ----------
            MethodInfo exportModelOriginal = AccessTools.DeclaredMethod(typeof(ModelExporter), nameof(ModelExporter.ExportModel), new Type[] {
                typeof(Slot),
                typeof(string),
                typeof(Predicate<Component>)
            });

            if (exportModelOriginal == null)
            {
                Error("Could not find ModelExporter.ExportModel(Slot, string, Predicate<Component>)");
                return;
            }
            MethodInfo exportModelPrefix = AccessTools.DeclaredMethod(typeof(ExportResoniteToJson), nameof(ExportModelPrefix));
            harmony.Patch(exportModelOriginal, prefix: new HarmonyMethod(exportModelPrefix));



            // ---------- Patch the DataTreeDictionary isSupported function ----------
            // Force the game to accept json files as real actual files plz and thnx
            MethodInfo dtcSupportedOrigional = AccessTools.DeclaredMethod(typeof(DataTreeConverter), nameof(DataTreeConverter.IsSupportedFormat), new Type[]
            {
                typeof(string),
            });

            if (dtcSupportedOrigional == null)
            {
                Error("Could not find DataTreeConverter.IsSupportedFormat(string)");
                return;
            }

            MethodInfo dtcSupportedPrefix = AccessTools.DeclaredMethod(typeof(ExportResoniteToJson), nameof(ExportResoniteToJson.IsSupportedFormat));
            harmony.Patch(dtcSupportedOrigional, prefix: new HarmonyMethod(dtcSupportedPrefix));




            // ---------- Patch the DataTreeDictionary Load function ----------
            MethodInfo dtcLoadOrigional = AccessTools.DeclaredMethod(typeof(DataTreeConverter), nameof(DataTreeConverter.Load), new Type[]
            {
                typeof(string),
                typeof(string)
            });

            if (dtcLoadOrigional == null)
            {
                Error("Could not find DataTreeConverter.Load(string, string)");
                return;
            }

            MethodInfo dtcLoadPrefix = AccessTools.DeclaredMethod(typeof(ExportResoniteToJson), nameof(ExportResoniteToJson.Load_Prefix));
            harmony.Patch(dtcLoadOrigional, prefix: new HarmonyMethod(dtcLoadPrefix));
        }




        // ------------------------------------------------------------------------------------------ //
        // Export Model Prefix

        private static bool ExportModelPrefix(Slot slot, string targetFile, Predicate<Component> filter, ref Task<bool> __result)
        {
            string extension = Path.GetExtension(targetFile).Substring(1).ToUpper();
            SavedGraph graph;
            switch (extension)
            {
                case "7ZBSON":
                    graph = slot.SaveObject(DependencyHandling.CollectAssets);
                    __result = Export7zbson(graph, targetFile);
                    return false; // skip original function
                case "JSON":
                    graph = slot.SaveObject(DependencyHandling.CollectAssets);
                    __result = ExportJson(graph, targetFile);
                    return false; // skip original function
                case "LZ4BSON":
                    graph = slot.SaveObject(DependencyHandling.CollectAssets);
                    __result = ExportLz4bson(graph, targetFile);
                    return false; // skip original function
                case "BSON":
                    graph = slot.SaveObject(DependencyHandling.CollectAssets);
                    __result = ExportBson(graph, targetFile);
                    return false; // skip original function
                default:
                    return true; // call original function
            }
        }

        private static async Task<bool> ExportJson(SavedGraph graph, string targetFile)
        {
            await new ToBackground();


            using (StreamWriter fs = File.CreateText(targetFile))
            {
                JsonTextWriter wr = new JsonTextWriter(fs);
                wr.Indentation = 2;
                wr.Formatting = Newtonsoft.Json.Formatting.Indented;
                AccessTools.Method(typeof(DataTreeConverter), "Write", null, null).Invoke(null, new object[] { graph.Root, wr });
            }

            Msg(string.Format("exported {0}", targetFile));
            return true;
        }

        private static async Task<bool> ExportBson(SavedGraph graph, string targetFile)
        {
            await new ToBackground();
            using (FileStream fileStream = File.OpenWrite(targetFile))
            {
                DataTreeConverter.ToBRSON(graph.Root, fileStream);
            }
            Msg(string.Format("exported {0}", targetFile));
            return true; // call original function
        }

        private static async Task<bool> ExportLz4bson(SavedGraph graph, string targetFile)
        {
            await new ToBackground();
            using (FileStream fileStream = File.OpenWrite(targetFile))
            {
                DataTreeConverter.ToLZ4BSON(graph.Root, fileStream);
            }
            Msg(string.Format("exported {0}", targetFile));
            return true;
        }

        private static async Task<bool> Export7zbson(SavedGraph graph, string targetFile)
        {
            await new ToBackground();
            using (FileStream fileStream = File.OpenWrite(targetFile))
            {
                DataTreeConverter.To7zBSON(graph.Root, fileStream);
            }
            Msg(string.Format("exported {0}", targetFile));
            return true;
        }




        // ------------------------------------------------------------------------------------------ //
        // Re-implement loading json files since resonite dropped that. This is an issue for Local storage mod since it does the same thing

        public static bool IsSupportedFormat(ref bool __result, string file)
        {
            string text = Path.GetExtension(file).ToLower();

            if (text == ".json")
            {
                __result = true;
                return false;
            }
            return true; // call original function
        }

        public static bool Load_Prefix(ref DataTreeDictionary __result, string file, string ext = null)
        {
            if (ext == null)
            {
                ext = Path.GetExtension(file).ToLower().Replace(".", "");
            }
            if (ext == "json")
            {
                using (StringReader stringReader = new StringReader(File.ReadAllText(file)))
                using (JsonTextReader jsonTextReader = new JsonTextReader(stringReader))
                {
                    __result = (DataTreeDictionary)AccessTools.Method(typeof(DataTreeConverter), "Read", null, null).Invoke(null, new object[] { jsonTextReader });
                }
                return false;
            }
            else
            {
                return true;
            }


        }



    }


}
