using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Bezel.Bridge.Editor.Fonts;
using System.Threading.Tasks;

namespace Bezel.Bridge
{
    public static class BezelGLTFConstructor
    {
        private static BezelRoot bezelRoot;
        private static List<Transform> nodeObjects = new List<Transform>();
        private static Dictionary<string, int> bezelIdsLookup = new Dictionary<string, int>();

        public static async Task<bool> PreparBezelTextResources(object objectItem)
        {
            BezelObjects bezelObjects;

            try
            {
                bezelObjects = JsonConvert.DeserializeObject<BezelObjects>(objectItem.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError("Fail to decode Bezel behavior json, likely schema mismatch from BezelSceneGraph.c. An error occurred: " + e.Message);
                return false;
            }

            FontManager.Reset(); 

            foreach (var bezelobject in bezelObjects.bezel_objects)
            {
                if (bezelobject.type == "Text")
                {
                    // Generate font map
                    await GenerateFontMap(bezelobject.parameters.fontFamily, bezelobject.parameters.fontWeight);
                }
            }

            return true;
        }

        public static void ObjectsContructor(GameObject gameObject, object objectItem)
        {
            ClearImportObjects();

            if (!DecodeBezelGLTFExtras(gameObject, objectItem)) return;

            if (!StoreImportedObjectTransform(gameObject)) return;

            if (!AttachBezelSchemaToRootObject()) return;

            AttachBezelBehavior();

            AttachBezelText();
        }

        private static void ClearImportObjects() {

            bezelIdsLookup.Clear();
            nodeObjects.Clear();
        }

        // Convert object to json format and validate the extras are bezel format json string. 
        private static bool DecodeBezelGLTFExtras(GameObject gameObject, object objectItem)
        {
            bezelRoot = gameObject.AddComponent<BezelRoot>();

            try
            {
                bezelRoot.rootObject = null;
                bezelRoot.rootObject = JsonConvert.DeserializeObject<BezelObjects>(objectItem.ToString());
            }
            catch (Exception e) {
                Debug.LogError("Fail to decode Bezel behavior json, likely schema mismatch from BezelSceneGraph.c. An error occurred: " + e.Message);
                return false;
            }

            if (bezelRoot.rootObject == null || bezelRoot.rootObject.bezel_objects == null || bezelRoot.rootObject.bezel_objects.Count == 0) {
                Debug.Log("No valid bezel extras");
                return false;
            }

            return true;
        }

        private static bool StoreImportedObjectTransform(GameObject gameObject) {

            int nodeID = 0;
            TraverseObjectHierarchy(gameObject.GetComponent<Transform>(), ref nodeID);
            return true;
        }

        // Critical: Recursively traverse the imported object using DFS to align glTF node sequence. 
        private static void TraverseObjectHierarchy(Transform parent, ref int nodeID)
        {
            if (parent.childCount == 0) return;
            
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);

                TraverseObjectHierarchy(child, ref nodeID);

                nodeObjects.Add(child);

                nodeID++;
            }
        }

        private static bool AttachBezelSchemaToRootObject()
        {
            if (bezelRoot.rootObject == null) return false;
            if (nodeObjects == null) return false;

            List<BezelObject> bObjects = bezelRoot.rootObject.bezel_objects;

            if (nodeObjects.Count != bObjects.Count)
            {
                //Debug.Log("Export glTF object count ("+ nodeObjects.Count +") is not the same as the bezel schema count ("+ bObjects.Count + ")!");
            }
            int id = 0;
            foreach (var bezelobject in bezelRoot.rootObject.bezel_objects)
            {
                bezelobject.gltf_id = id;
                bezelobject.transform = nodeObjects[bezelobject.gltf_id];
                if (!bezelIdsLookup.ContainsKey(bezelobject.id))
                {
                    bezelIdsLookup.Add(bezelobject.id, bezelobject.gltf_id);
                }
                id++;

                // Both DirectionalLight and SpotLight have an additional node, hence attach next node to the bezel object. This ensure a correct ordering for the next nodes.
                if (bezelobject.type == "DirectionalLight" || bezelobject.type == "SpotLight")
                {
                    id++;
                }

            }

            return true;
        }

        private static void AttachBezelBehavior()
        {
            if (bezelRoot.rootObject == null) return;
            if (nodeObjects == null) return;

            foreach (var bezelobject in bezelRoot.rootObject.bezel_objects)
            {
                Transform nodeObject = nodeObjects[bezelobject.gltf_id];

                // Set visibility
                nodeObject.gameObject.SetActive(bezelobject.visible);

                // Todo: Clean up to standarize assignment
                if (bezelobject.states.Count == 0 && bezelobject.interactions.Count == 0) {
                    continue;
                }
                else {
                    // Todo: Clean up to standarize assignment
                    nodeObject.gameObject.AddComponent<BezelBehavior>();
                    nodeObject.gameObject.GetComponent<BezelBehavior>().AttachBezelBehavior(bezelobject.states, bezelobject.interactions);
                }

                foreach (var _s in bezelobject.states)
                {
                    BezelBehavior bezelBehavior = nodeObject.gameObject.GetComponent<BezelBehavior>();

                    if (bezelBehavior != null && _s.Value.rotation != null)
                    {
                        // Todo: Setting up the states parameters 
                    }
                }

                foreach (var bezelinteraction in bezelobject.interactions)
                {
                    if (bezelinteraction.Value != null && 
                        bezelinteraction.Value.trigger != null && 
                        bezelinteraction.Value.trigger.targetEntityIds != null) {

                        foreach (var targetEntityId in bezelinteraction.Value.trigger.targetEntityIds) {

                            // Add check to ensure dictionary look up is valid
                            int _targetEntity_gltf_Id;

                            if (bezelIdsLookup.TryGetValue(targetEntityId, out _targetEntity_gltf_Id)) {
                                bezelinteraction.Value.trigger.targetEntity_gltf_Ids.Add(_targetEntity_gltf_Id);
                            } else {
                                Debug.LogWarning("[Bezel] Some objects (e.g. Text) can't be used as target. "+
                                "The targetEntityId: " + targetEntityId + " is not found in the glTF object list.");
                            }
                            // Insert target frame reference into the trigger frame
                            // Todo: To implement interactions, setup targetObjectTransform based on nodeObjects[_targetEntity_gltf_Id];
                        }
                    }
                }
            }
        }

        private static void AttachBezelText() {
            if (bezelRoot.rootObject == null) return;
            if (nodeObjects == null) return;

            int firstValid = 0;

            foreach (var bezelobject in bezelRoot.rootObject.bezel_objects)
            {
                if (bezelobject.type == "Text")
                {
                    if (firstValid == 0)
                    {
                        bezelobject.transform.gameObject.AddComponent<BezelText>();
                        System.Threading.Tasks.Task<bool> task = bezelobject.transform.gameObject.GetComponent<BezelText>().SetTextParameters(bezelobject.parameters);
                    }

                    // Remove custom text offset after alignment setting changed.
                    if (firstValid == 1)
                    {
                        bezelobject.transform.gameObject.transform.localPosition = Vector3.zero;
                    }

                    firstValid++;
                    if (firstValid == 3) { firstValid = 0; }

                }
            }
        }

        private static async Task<BezelFontMap> GenerateFontMap(string fontFamily, string fontWeight)
        {
            int fontWeightInt = FontManager.FontWeightStringToInt(fontWeight);

            BezelFontMap fontMap = await FontManager.GenerateFontMapForDocument(fontFamily, fontWeightInt, true);

            return fontMap;
        }

    }
}

