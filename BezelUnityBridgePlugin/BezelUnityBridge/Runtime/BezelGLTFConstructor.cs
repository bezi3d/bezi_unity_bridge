using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Bezel.Bridge
{
    public static class BezelGLTFConstructor
    {
        private static BezelRoot bezelRoot;
        private static List<Transform> nodeObjects = new List<Transform>();
        private static Dictionary<string, int> bezelIdsLookup = new Dictionary<string, int>();

        // 1. Decode bezel extras into Unity C# format for later access.
        // 2. Store imported object transforms for later reference.
        // 3. Insert parameters and reference by mapping bezel data into imported objects 

        public static void ObjectsContructor(GameObject gameObject, object objectItem)
        {
            ClearImportObjects();

            if (!DecodeBezelGLTFExtras(gameObject, objectItem)) return;

            if (!StoreImportedObjectTransform(gameObject)) return;

            AttachBezelSchemaToRootObject(gameObject);

            AttachBezelBehavior();
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
                Debug.LogError("Fail to decode Bezel behavior json, likely schema mismatch from BezelSceneGraph.cs");
                Debug.LogError("An error occurred: " + e.Message);

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

                //Debug.Log("ID: " + nodeID + ", GameObject: " + child.name); 

                nodeObjects.Add(child);

                nodeID++;
            }
        }

        //Todo: Return status code (fail, success, ..etc)
        private static void AttachBezelSchemaToRootObject(GameObject gameObject)
        {
            if (bezelRoot.rootObject == null) return;
            if (nodeObjects == null) return;
            int id = 0;
            foreach (var bezelobject in bezelRoot.rootObject.bezel_objects)
            {
                bezelobject.gltf_id = id;
                bezelobject.transform = nodeObjects[bezelobject.gltf_id];
                bezelIdsLookup.Add(bezelobject.id, bezelobject.gltf_id);

                id++;
                // Todo: Clean up to standarize assignment
                nodeObjects[bezelobject.gltf_id].AddComponent<BezelBehavior>();
                nodeObjects[bezelobject.gltf_id].GetComponent<BezelBehavior>().glTF_id = bezelobject.gltf_id;
                nodeObjects[bezelobject.gltf_id].GetComponent<BezelBehavior>().id = bezelobject.id;
                if (bezelobject.type != null)
                {
                    nodeObjects[bezelobject.gltf_id].GetComponent<BezelBehavior>().type = bezelobject.type;
                }
                if (bezelobject.name != null)
                {
                    nodeObjects[bezelobject.gltf_id].GetComponent<BezelBehavior>().name = bezelobject.name;
                }
            }
        }

        //Todo: Return status code (fail, success, ..etc)
        private static void AttachBezelBehavior()
        {
            if (bezelRoot.rootObject == null) return;
            if (nodeObjects == null) return;

            foreach (var bezelobject in bezelRoot.rootObject.bezel_objects)
            {

                // Todo: Clean up to standarize assignment
                if (bezelobject.states.Count == 0 && bezelobject.interactions.Count == 0) {
                    continue;
                }
                else {
                    nodeObjects[bezelobject.gltf_id].GetComponent<BezelBehavior>().AttachBezelBehavior(bezelobject.states, bezelobject.interactions);
                }

                foreach (var _s in bezelobject.states)
                {
                    nodeObjects[bezelobject.gltf_id].GetComponent<BezelBehavior>().targetRotation = Quaternion.Euler(_s.Value.rotation[0] * Mathf.Rad2Deg, _s.Value.rotation[1] * Mathf.Rad2Deg, _s.Value.rotation[2] * Mathf.Rad2Deg);
                }

                foreach (var bezelinteraction in bezelobject.interactions)
                {

                    if (bezelinteraction.Value.trigger != null) {

                        foreach (var id in bezelinteraction.Value.trigger.targetEntityIds) {

                            bezelinteraction.Value.trigger.targetEntity_gltf_Ids.Add(bezelIdsLookup[id]);

                            // Insert target frame reference into the trigger frame
                            // Todo: targetObjectTransform[0] is a hack before fully implementing the interaction
                            nodeObjects[bezelobject.gltf_id].GetComponent<BezelBehavior>().targetObjectTransform[0] = nodeObjects[bezelIdsLookup[id]];
                        }
                    }
                }
            }
        }
    }
}
