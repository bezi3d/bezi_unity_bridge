using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Bezi.Bridge
{
    [System.Serializable]
    public class GLTFSchema
    {
        public object extras;
    }

    [System.Serializable]
    public class BeziObjects
    {
        public List<BeziObject> bezel_objects; // TODO: rename to bezi_objects after backend migration.
    }

    [System.Serializable]
    public class BeziObject
    {
        public int gltf_id;
        public string id;
        public string type;
        public string name;
        public bool isVisible;
        public Transform transform;
        public UDictionary<string, State> states;
        public UDictionary<string, Interaction> interactions;
        public Parameters parameters;
    }

    // Bezi SceneObjectStatefulDataSchema
    [System.Serializable]
    public class State
    {
        public string name;
        public List<float> position;
        public List<float> rotation;

        public State(List<float> rotation, string name)
        {
            this.rotation = rotation;
            this.name = name;
        }
    }

    [System.Serializable]
    public class Interaction
    {
        public Trigger trigger;
        public UDictionary<string, Animation> animations;
    }

    [System.Serializable]
    public class Trigger
    {
        public string type;
        public string @event;
        public List<string> targetEntityIds;
        public List<int> targetEntity_gltf_Ids = new List<int>();
    }

    [System.Serializable]
    public class Animation
    {
        public string id;
        public string sceneEntityIds;
        //public string fromStateId;
        public string toStateId;
        public float duration;
        public float delay;
    }

    [System.Serializable]
    public class Parameters
    {
        public string text;
        public float fontSize;
        public string fontFamily;
        public string fontWeight;
        public string textAlign;        // "left", "center", "right", "justify"
        public string verticalAlign;    // "top", "middle", "bottom"
        public string verticalTrim;     // "standard", "top-cap-to-bottom-baseline"
        public string letterCase;       // "as-typed", "uppercase", "lowercase", "title-case",
        public string lineHeight;
        public float letterSpacing;
        public float curveRadius;
        public object width;
        public object depth;
        public string color;
    }
}
