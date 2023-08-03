using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Bezel.Bridge.Editor.Fonts;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Bezel.Bridge
{
    public class BezelText : MonoBehaviour
    {
        [SerializeField]
        private Parameters parameters;

        private float magicScale = 0.135f; //Bezel to Unity font rescale

        public async void SetTextParameters(Parameters parameters)
        {
            this.parameters = parameters;

            this.gameObject.AddComponent<MeshRenderer>();

            this.gameObject.AddComponent<TextMeshPro>();

            TextMeshPro text = this.gameObject.GetComponent<TextMeshPro>();
            text.text = parameters.text;
            text.fontSize = parameters.fontSize;
            text.color = convertColorCode(parameters.color);
            //tmp.material.shader = 
            // Set to default pivot point
            text.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

            text.GetComponent<RectTransform>().localScale = new Vector3(-magicScale, magicScale, magicScale);

            //letterSpacing
            text.characterSpacing = parameters.letterSpacing;

            // Generate font map
            BezelFontMap fontMap = await GenerateFontMap();

            // Get font map
            var matchingFontMapping = fontMap.GetFontMapping(parameters.fontFamily, 400);

            text.font = matchingFontMapping.FontAsset;



            var effectMaterialPreset = GetEffectMaterialPreset(matchingFontMapping);
            text.fontMaterial = effectMaterialPreset;
        }

        private async Task<BezelFontMap> GenerateFontMap()
        {
            // Generate font mapping data
            var bezelFontMapTask = FontManager.GenerateFontMapForDocument(parameters.fontFamily, 400, true);

            await bezelFontMapTask;

            var fontMap = bezelFontMapTask.Result;

            return fontMap;
        }

        private Material GetEffectMaterialPreset(BezelFontMapEntry matchingFontMapping)
        {

            var hasShadowEffect = false;
            var shadowColor = UnityEngine.Color.black; // Not in used
            var shadowDistance = Vector2.zero; // Not in used
            var hasOutlineColor = false;
            var outlineColor = UnityEngine.Color.black; // Not in used
            var outlineWidth = 0f; // Not in used
            return FontManager.GetEffectMaterialPreset(matchingFontMapping,
                        hasShadowEffect, shadowColor, shadowDistance, hasOutlineColor, outlineColor, outlineWidth);
        }


        private Color convertColorCode(string colorCode)
        {
            Color color = Color.white;
            if (!ColorUtility.TryParseHtmlString(colorCode, out color))
            {
                Debug.LogError("Invalid color code: " + colorCode);
            }

            return color;
        }
    }
}