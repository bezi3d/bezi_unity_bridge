using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Bezel.Bridge.Editor.Fonts;
using System.Threading.Tasks;
using UnityEngine.Windows;
using System;

namespace Bezel.Bridge
{
    public class BezelText : MonoBehaviour
    {
        [SerializeField]
        private Parameters parameters;

        private float magicScale1 = 0.135f; //Bezel to Unity font rescale

        private const float magicScale2 = 7.4f; //Bezel to Unity font rescale

        public async void SetTextParameters(Parameters parameters)
        {
            this.parameters = parameters;

            this.gameObject.AddComponent<MeshRenderer>();

            this.gameObject.AddComponent<TextMeshPro>();

            TextMeshPro text = this.gameObject.GetComponent<TextMeshPro>();

            // Generate font map
            BezelFontMap fontMap = await GenerateFontMap();

            // Get font map
            BezelFontMapEntry matchingFontMapping = fontMap.GetFontMapping(parameters.fontFamily, 400);

            try
            {
                text.font = matchingFontMapping.FontAsset;
            }
            catch (NullReferenceException)
            {

                // Handle exception
                Debug.LogWarning("Ignoring NullRef due to TMP error");
            }

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

            var effectMaterialPreset = GetEffectMaterialPreset(matchingFontMapping);
            text.fontMaterial = effectMaterialPreset;
        }

        private async Task<BezelFontMap> GenerateFontMap()
        {
            //Debug.Log("1: GenerateFontMap");
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

        private Vector2 getMaxHeightWidth(Parameters p)
        {
            float maxWidth = 1.0f;
            float maxHeight = 1.0f;
            float parsed;
            if (p.maxWidth != null && float.TryParse(p.maxWidth.ToString(), out parsed))
            {
                maxWidth = parsed;
            }
            if (p.maxHeight != null && float.TryParse(p.maxHeight.ToString(), out parsed))
            {
                maxHeight = parsed;
            }

            return new Vector2(
                maxWidth * magicScale2, maxHeight * magicScale2);

        }

        private Color convertColorCode(string colorCode)
        {
            Color color = Color.white;
            if (!ColorUtility.TryParseHtmlString(colorCode, out color))
            {
                color = Color.white;
                //Debug.Log("Invalid color code: " + colorCode);
            }

            return color;
        }
    }
}