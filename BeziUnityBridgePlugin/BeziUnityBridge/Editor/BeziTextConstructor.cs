using TMPro;
using UnityEngine;
using Bezi.Bridge.Editor.Fonts;
using System.Threading.Tasks;
using System;
using Bezi.Bridge;

namespace Bezi.Bridge.Editor
{
    public static class BeziTextConstructor
    {
        [SerializeField]
        private static Bezi.Bridge.Parameters parameters;

        private static float magicScale1 = 0.135f; //Bezi to Unity font rescale

        private const float magicScale2 = 7.4f; //Bezi to Unity font rescale

        public static async Task<bool> SetTextParameters(GameObject textObject, Parameters parameters)
        {
            textObject.GetComponent<BeziText>().parameters = parameters;

            textObject.AddComponent<MeshRenderer>();

            textObject.AddComponent<TextMeshPro>();

            if (!textObject.GetComponent<TextMeshPro>()) return false;

            TextMeshPro text = textObject.GetComponent<TextMeshPro>();

            text.text = parameters.text;
            text.fontSize = parameters.fontSize;
            text.color = convertColorCode(parameters.color);

            // Set to default pivot point
            text.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

            text.GetComponent<RectTransform>().localScale = new Vector3(
                -magicScale1, magicScale1, magicScale1);

            text.GetComponent<RectTransform>().sizeDelta = getMaxHeightWidth(parameters);

            //letterSpacing
            text.characterSpacing = parameters.letterSpacing;

            text.horizontalAlignment = parameters.textAlign switch
            {
                "left" => HorizontalAlignmentOptions.Left,
                "center" => HorizontalAlignmentOptions.Center,
                "justify" => HorizontalAlignmentOptions.Justified,
                "right" => HorizontalAlignmentOptions.Right,
                _ => HorizontalAlignmentOptions.Left
            };

            text.verticalAlignment = parameters.verticalAlign switch
            {
                "top" => VerticalAlignmentOptions.Top,
                "middle" => VerticalAlignmentOptions.Middle,
                "bottom" => VerticalAlignmentOptions.Bottom,
                _ => VerticalAlignmentOptions.Top,
            };

            text.fontStyle |= parameters.letterCase switch
            {
                "as-typed" => 0,
                "uppercase" => FontStyles.UpperCase,
                "lowercase" => FontStyles.LowerCase,
                "title-case" => FontStyles.SmallCaps,
                _ => 0
            };

            // Generate font map
            BeziFontMap fontMap = await GenerateFontMap(parameters.fontFamily, parameters.fontWeight);

            int fontWeightInt = FontManager.FontWeightStringToInt(parameters.fontWeight);

            // Get font map
            BeziFontMapEntry matchingFontMapping = fontMap.GetFontMapping(parameters.fontFamily, fontWeightInt);

            try
            {
                text.font = matchingFontMapping.FontAsset;
            }
            catch (NullReferenceException)
            {

                // Handle exception
                Debug.LogWarning("Ignoring NullRef due to TMP error");
            }

            var effectMaterialPreset = GetEffectMaterialPreset(matchingFontMapping);
            text.fontMaterial = effectMaterialPreset;

            return true;
        }

        private static async Task<BeziFontMap> GenerateFontMap(string fontFamily, string fontWeight)
        {
            int fontWeightInt = FontManager.FontWeightStringToInt(fontWeight);

            BeziFontMap fontMap = await FontManager.GenerateFontMapForDocument(fontFamily, fontWeightInt, true);

            return fontMap;
        }


        private static Material GetEffectMaterialPreset(BeziFontMapEntry matchingFontMapping)
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

        private static Vector2 getMaxHeightWidth(Parameters p)
        {
            float maxWidth = 1.0f;
            float maxHeight = 1.0f;
            float parsed;
            if (p.width != null && float.TryParse(p.width.ToString(), out parsed))
            {
                maxWidth = parsed;
            }
            if (p.depth != null && float.TryParse(p.depth.ToString(), out parsed))
            {
                maxHeight = parsed;
            }

            return new Vector2(
                maxWidth * magicScale2, maxHeight * magicScale2);

        }

        private static Color convertColorCode(string colorCode)
        {
            Color color = Color.white;
            if (!ColorUtility.TryParseHtmlString(colorCode, out color))
            {
                color = Color.white;
            }

            return color;
        }
    }
}