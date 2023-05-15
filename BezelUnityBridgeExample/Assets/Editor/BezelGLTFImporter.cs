using UnityEngine;
using UnityEditor.AssetImporters;
using Siccity.GLTFUtility;

namespace Bezel.Bridge
{
#if ANOTHER_IMPORTER_HAS_HIGHER_PRIORITY
    [ScriptedImporter(1, "gltf")]
#endif
    public class BezelGLTFImporter : ScriptedImporter
    {
        BezelGLTFExtrasProcessor bezelExtrasProcessor;

        public ImportSettings importSettings;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            // Load asset
            AnimationClip[] animations;
            if (importSettings == null) importSettings = new ImportSettings();
            Debug.Log("Bezel GLTFImporter Design Time Import!");
            bezelExtrasProcessor = new BezelGLTFExtrasProcessor();
            importSettings.extrasProcessor = bezelExtrasProcessor;

            GameObject root = Importer.LoadFromFile(ctx.assetPath, importSettings, out animations, Format.GLTF);

            // Save asset
            GLTFAssetUtility.SaveToAsset(root, animations, ctx, importSettings);
        }
    }
}