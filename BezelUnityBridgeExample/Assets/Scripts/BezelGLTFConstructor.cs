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
        private static RootObject rootObject;
        private static List<Transform> nodeObjects = new List<Transform>();

        // 1. Decode bezel extras into Unity C# format for later access.
        // 2. Store imported object transforms for later reference.
        // 3. Insert parameters and reference by mapping bezel data into imported objects 

        public static void ObjectsContructor(GameObject gameObject, object objectItem)
        {
            if(!DecodeBezelGLTFExtras(objectItem)) return;

            int nodeID = 0;
            TraverseObjectHierarchy(gameObject.GetComponent<Transform>(), ref nodeID);

            AttachBezelBehavior();
        }

        // Convert object to json format and validate the extras are bezel format json string. 
        private static bool DecodeBezelGLTFExtras(object objectItem)
        {
            rootObject = JsonConvert.DeserializeObject<RootObject> (objectItem.ToString());

            if (rootObject == null || rootObject.bezel_objects == null || rootObject.bezel_objects.Count == 0) {
                Debug.Log("No valid bezel extras");
                return false;
            }

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
        private static void AttachBezelBehavior()
        {
            if (rootObject == null) return;
            if (nodeObjects == null) return;

            foreach (var bezelobject in rootObject.bezel_objects) {
                int objectID;
                if (int.TryParse(bezelobject.Value.id, out objectID))
                {
                    Debug.Log("Attach Bezel Behavior for objectID: " + objectID);
  
                    // Todo: Clean up to standarize assignment
                    nodeObjects[objectID].AddComponent<BezelBehavior>();
nodeObjects[objectID].GetComponent<BezelBehavior>().AttachBezelBehavior(bezelobject.Value.states, bezelobject.Value.interactions);

                    foreach (var bezelinteraction in bezelobject.Value.interactions)
                    {
                        int targetEntityIds;
                        if (int.TryParse(bezelinteraction.Value.trigger.targetEntityIds, out targetEntityIds))
                        {
                            nodeObjects[objectID].GetComponent<BezelBehavior>().targetObjectTransform = nodeObjects[targetEntityIds];
                            Debug.Log(" Insert target transform as reference..." + nodeObjects[targetEntityIds].name);
                        }
                        else
                        {
                            Debug.LogError("Bezel json format is incorrect: RootObject/BezelObejct/interactions/trigger/targetEntityIds");
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogError("Bezel json format is incorrect: RootObject/BezelObejct/id");
                    break;
                }
            }
        }
    }
}
