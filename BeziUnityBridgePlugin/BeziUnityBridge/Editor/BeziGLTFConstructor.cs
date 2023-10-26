using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Bezi.Bridge.Editor.Fonts;
using System.Threading.Tasks;

namespace Bezi.Bridge.Editor
{
    public static class BeziGLTFConstructor
    {
        private static BeziRoot beziRoot;
        private static List<Transform> nodeObjects = new List<Transform>();
        private static Dictionary<string, int> beziIdsLookup = new Dictionary<string, int>();

        public static async Task<bool> PreparBeziTextResources(object objectItem)
        {
            BeziObjects beziObjects;

            try
            {
                beziObjects = JsonConvert.DeserializeObject<BeziObjects>(objectItem.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError("Fail to decode Bezi behavior json, likely schema mismatch from BeziSceneGraph.c. An error occurred: " + e.Message);
                return false;
            }

            FontManager.Reset(); 

            if (beziObjects == null || beziObjects.bezel_objects == null || beziObjects.bezel_objects.Count == 0)
            {
                Debug.LogError("Failed to parse resources.");
                return false;
            }
            foreach (var beziobject in beziObjects.bezel_objects)
            {
                if (beziobject.type == "Text")
                {
                    // Generate font map
                    await GenerateFontMap(beziobject.parameters.fontFamily, beziobject.parameters.fontWeight);
                }
            }

            return true;
        }

        public static void ObjectsContructor(GameObject gameObject, object objectItem)
        {
            ClearImportObjects();

            if (!DecodeBeziGLTFExtras(gameObject, objectItem)) return;

            if (!StoreImportedObjectTransform(gameObject)) return;

            if (!AttachBeziSchemaToRootObject()) return;

            AttachBeziBehavior();

            AttachBeziText();
        }

        private static void ClearImportObjects() {

            beziIdsLookup.Clear();
            nodeObjects.Clear();
        }

        // Convert object to json format and validate the extras are bezi format json string. 
        private static bool DecodeBeziGLTFExtras(GameObject gameObject, object objectItem)
        {
            beziRoot = gameObject.AddComponent<BeziRoot>();

            try
            {
                beziRoot.rootObject = null;
                beziRoot.rootObject = JsonConvert.DeserializeObject<BeziObjects>(objectItem.ToString());
            }
            catch (Exception e) {
                Debug.LogError("Fail to decode Bezi behavior json, likely schema mismatch from BeziSceneGraph.c. An error occurred: " + e.Message);
                return false;
            }

            if (beziRoot.rootObject == null || beziRoot.rootObject.bezel_objects == null || beziRoot.rootObject.bezel_objects.Count == 0) {
                Debug.Log("No valid bezi extras");
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

        private static bool AttachBeziSchemaToRootObject()
        {
            if (beziRoot.rootObject == null) return false;
            if (nodeObjects == null) return false;

            List<BeziObject> bObjects = beziRoot.rootObject.bezel_objects;

            if (nodeObjects.Count != bObjects.Count)
            {
                //Debug.Log("Export glTF object count ("+ nodeObjects.Count +") is not the same as the bezi schema count ("+ bObjects.Count + ")!");
            }
            int id = 0;
            foreach (var beziobject in beziRoot.rootObject.bezel_objects)
            {
                beziobject.gltf_id = id;
                beziobject.transform = nodeObjects[beziobject.gltf_id];
                if (!beziIdsLookup.ContainsKey(beziobject.id))
                {
                    beziIdsLookup.Add(beziobject.id, beziobject.gltf_id);
                }
                id++;

                // Both DirectionalLight and SpotLight have an additional node, hence attach next node to the bezi object. This ensure a correct ordering for the next nodes.
                if (beziobject.type == "DirectionalLight" || beziobject.type == "SpotLight")
                {
                    id++;
                }

            }

            return true;
        }

        private static void AttachBeziBehavior()
        {
            if (beziRoot.rootObject == null) return;
            if (nodeObjects == null) return;

            foreach (var beziobject in beziRoot.rootObject.bezel_objects)
            {
                Transform nodeObject = nodeObjects[beziobject.gltf_id];

                // Set visibility
                nodeObject.gameObject.SetActive(beziobject.isVisible);

                // Todo: Clean up to standarize assignment
                if (beziobject.states.Count == 0 && beziobject.interactions.Count == 0) {
                    continue;
                }
                else {
                    // Todo: Clean up to standarize assignment
                    nodeObject.gameObject.AddComponent<BeziBehavior>();
                    nodeObject.gameObject.GetComponent<BeziBehavior>().AttachBeziBehavior(beziobject.states, beziobject.interactions);
                }

                foreach (var _s in beziobject.states)
                {
                    BeziBehavior beziBehavior = nodeObject.gameObject.GetComponent<BeziBehavior>();

                    if (beziBehavior != null && _s.Value.rotation != null)
                    {
                        // Todo: Setting up the states parameters 
                    }
                }

                foreach (var beziinteraction in beziobject.interactions)
                {
                    if (beziinteraction.Value != null && 
                        beziinteraction.Value.trigger != null && 
                        beziinteraction.Value.trigger.targetEntityIds != null) {

                        foreach (var targetEntityId in beziinteraction.Value.trigger.targetEntityIds) {

                            // Add check to ensure dictionary look up is valid
                            int _targetEntity_gltf_Id;

                            if (beziIdsLookup.TryGetValue(targetEntityId, out _targetEntity_gltf_Id)) {
                                beziinteraction.Value.trigger.targetEntity_gltf_Ids.Add(_targetEntity_gltf_Id);
                            } else {
                                Debug.LogWarning("[Bezi] Some objects (e.g. Text) can't be used as target. "+
                                "The targetEntityId: " + targetEntityId + " is not found in the glTF object list.");
                            }
                            // Insert target frame reference into the trigger frame
                            // Todo: To implement interactions, setup targetObjectTransform based on nodeObjects[_targetEntity_gltf_Id];
                        }
                    }
                }
            }
        }

        private static void AttachBeziText() {
            if (beziRoot.rootObject == null) return;
            if (nodeObjects == null) return;

            int firstValid = 0;

            foreach (var beziobject in beziRoot.rootObject.bezel_objects)
            {
                if (beziobject.type == "Text")
                {
                    if (firstValid == 0)
                    {
                        beziobject.transform.gameObject.AddComponent<BeziText>();

                        System.Threading.Tasks.Task<bool> task = BeziTextConstructor.SetTextParameters(beziobject.transform.gameObject, beziobject.parameters);

                    }

                    // Remove custom text offset after alignment setting changed.
                    if (firstValid == 1)
                    {
                        beziobject.transform.gameObject.transform.localPosition = Vector3.zero;
                    }

                    firstValid++;
                    if (firstValid == 3) { firstValid = 0; }

                }
            }
        }

        private static async Task<BeziFontMap> GenerateFontMap(string fontFamily, string fontWeight)
        {
            int fontWeightInt = FontManager.FontWeightStringToInt(fontWeight);

            BeziFontMap fontMap = await FontManager.GenerateFontMapForDocument(fontFamily, fontWeightInt, true);

            return fontMap;
        }

    }
}

