using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Bezel.Bridge
{
    public class BezelText : MonoBehaviour
    {
        [SerializeField]
        private Parameters parameters;

        private float magicScale = 0.135f; //Bezel to Unity font rescale

        public void SetTextParameters(Parameters parameters)
        {
            this.parameters = parameters;
            this.gameObject.AddComponent<MeshRenderer>();
            this.gameObject.AddComponent<TextMeshPro>();
            TextMeshPro tmp = this.gameObject.GetComponent<TextMeshPro>();
            tmp.text = parameters.text;
            tmp.fontSize = parameters.fontSize;
            tmp.color = convertColorCode(parameters.color);

            // Set to default pivot point
            tmp.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

            tmp.GetComponent<RectTransform>().localScale = new Vector3(-magicScale, magicScale, magicScale);
        }

        private Color convertColorCode(string colorCode)
        {
            Color color = Color.white;
            if (ColorUtility.TryParseHtmlString(colorCode, out color))
            {
                // Do something with the color
                Debug.Log("Converted Color: " + color);
                
            }
            else
            {
                Debug.LogError("Invalid color code: " + colorCode);
            }

            return color;
        }
    }
}