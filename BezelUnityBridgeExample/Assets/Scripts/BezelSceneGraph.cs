using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bezel.Bridge
{
    [System.Serializable]
    public class RootObject
    {
        public Dictionary<string, BezelObject> bezel_objects;
    }

    [System.Serializable]
    public class BezelObject
    {
        public string id;
        public Dictionary<string, State> states;
        public Dictionary<string, Interaction> interactions;
    }

    [System.Serializable]
    public class State
    {
        public List<float> rotation;
        public string parameters;
        public string name;

        public State(List<float> rotation, string parameters, string name)
        {
            this.rotation = rotation;
            this.parameters = parameters;
            this.name = name;
        }
    }

    [System.Serializable]
    public class Interaction
    {
        public Trigger trigger;
        public Dictionary<string, Animation> animations;
    }

    [System.Serializable]
    public class Trigger
    {
        public string type;
        public string @event;
        public string targetEntityIds;
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
}