using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Siccity.GLTFUtility;

namespace Bezel.Bridge
{
    public class BezelGLTFExtrasProcessor : GLTFExtrasProcessor
    {
        public override void ProcessExtras(GameObject importedObject, AnimationClip[] animations, JObject extras)
        {
            // new implementation here
            object objectItem = extras;

            BezelGLTFConstructor.ObjectsContructor(importedObject, objectItem);
        }
    }
}