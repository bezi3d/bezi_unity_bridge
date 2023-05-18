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

    //=======================

    //[System.Serializable]
    //public class RootObject
    //{
    //    public List<BezelObject> objects;
    //}

    //[System.Serializable]
    //public class BezelObject
    //{
    //    public string id;
    //    public List<State> states;
    //    public List<Interaction> interactions;
    //}

    //[System.Serializable]
    //public class State
    //{
    //    public float[] rotation;
    //    public List<string> parameters;
    //    public string name;
    //}

    //[System.Serializable]
    //public class Interaction
    //{
    //    public Trigger trigger;
    //    public List<Animation> animations;
    //}

    //[System.Serializable]
    //public class Trigger
    //{
    //    public string type;
    //    public string @event;
    //}

    //[System.Serializable]
    //public class Animation
    //{
    //    public string id;
    //    public string targetEntityIds;
    //    public string fromStateId;
    //    public string toStateId;
    //    public float duration;
    //    public float delay;
    //}


    //=======================


    //[System.Serializable]
    //public class RootObject
    //{
    //    public Dictionary<string, BezelObject> objects;
    //}

    //[System.Serializable]
    //public class BezelObject
    //{
    //    public string id;
    //    public Dictionary<string, State> states;
    //    public Dictionary<string, Interaction> interactions;
    //}

    //[System.Serializable]
    //public class State
    //{
    //    public float[] rotation;
    //    public Dictionary<string, object> parameters;
    //    public string name;
    //}

    //[System.Serializable]
    //public class Interaction
    //{
    //    public Trigger trigger;
    //    public Dictionary<string, Animation> animations;
    //}

    //[System.Serializable]
    //public class Trigger
    //{
    //    public string type;
    //    public string @event;
    //}

    //[System.Serializable]
    //public class Animation
    //{
    //    public string id;
    //    public string targetEntityIds;
    //    public string fromStateId;
    //    public string toStateId;
    //    public float duration;
    //    public float delay;
    //}

    //=======================


    //public class RootObject
    //{
    //    public Dictionary<string, BezelObject> objects { get; set; }
    //}

    //public class BezelObject
    //{
    //    public string id { get; set; }
    //    public Dictionary<string, State> states { get; set; }
    //    public Dictionary<string, Interaction> interactions { get; set; }
    //}

    //public class State
    //{
    //    public List<double> position { get; set; }
    //    public List<int> rotation { get; set; }
    //    public List<int> scale { get; set; }
    //    public long lastGeometryUpdated { get; set; }
    //    public Material material { get; set; }
    //    public Dictionary<string, object> parameters { get; set; }
    //    public string name { get; set; }
    //}

    //public class Interaction
    //{
    //    public Trigger trigger { get; set; }
    //    public Dictionary<string, Animation> animations { get; set; }
    //}


    //public class Trigger
    //{
    //    public string type { get; set; }
    //    public string @event { get; set; }
    //    public List<string> targetEntityIds { get; set; }
    //}

    //public class Animation
    //{
    //    public string id { get; set; }
    //    public string sceneEntityId { get; set; }
    //    public string fromStateId { get; set; }
    //    public string toStateId { get; set; }
    //    public double duration { get; set; }
    //    public double delay { get; set; }
    //}


    //=======================
}
