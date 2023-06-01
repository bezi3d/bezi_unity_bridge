using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

namespace Bezel.Bridge
{
    public class BezelBehavior : MonoBehaviour
    {

        // Todo: Configure this from bezel state and interactions
        public bool toggle = true;

        public bool triggered = false;

        // Todo: Implement a proper set/get. This will require heavy rewrite to scale to transform array. Trigger multiple objects, each with different action...
        //[SerializeField]
        public Transform[] targetObjectTransform = new Transform[1];

        // Todo: Implement a proper set/get
        public Dictionary<string, State> states;
        public Dictionary<string, Interaction> interactions;


        public Quaternion initialRotation;
        public Quaternion targetRotation;
        public float duration = 0.5f;

        // Constructor
        public void AttachBezelBehavior(Dictionary<string, State> _states, Dictionary<string, Interaction> _interactions) {
            states = _states;
            interactions = _interactions;
        }

        // Todo: Configure Animation event from bezel state and interactions
        private void OnMouseDown()
        {
            triggered = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            PrepareCollider();

            // Todo: Configure following rotations from bezel state and interactions
            //Prepare Base State
            //PrepareBaseState();

            // This can only be set after import, e.g. Start().
            // Unless targetObjectTransform is ready first.
            // For now, targetObjectTransform reference is inserted during import.
            // Todo: Configure following rotations once implement proper states in unity
            initialRotation = targetObjectTransform[0].rotation;

            triggered = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (triggered) {
                triggered = false;

                if (targetObjectTransform[0])
                {
                    if (toggle)
                    {
                        StartCoroutine(TweenRotation(initialRotation, targetRotation));
                        toggle = false;
                    }
                    else
                    {
                        StartCoroutine(TweenRotation(targetRotation, initialRotation));
                        toggle = true;
                    }
                }
            }
        }

        // Preparation ==================================
        // Todo: Match how bezel create collider
        private void PrepareCollider()
        {
            MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>();

            if (childRenderers.Length == 0)
            {
                Debug.LogWarning("No MeshRenderer found in child objects. SphereColliderFitter requires at least one MeshRenderer.");
                return;
            }

            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();

            Bounds bounds = new Bounds(childRenderers[0].bounds.center, Vector3.zero);

            foreach (MeshRenderer renderer in childRenderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            Vector3 center = bounds.center;
            float radius = bounds.extents.magnitude;

            sphereCollider.center = transform.InverseTransformPoint(center);
            sphereCollider.radius = radius;
        }

        private void PrepareBaseState() {
            List<float> _rotation = new List<float>();
            _rotation.Add(targetObjectTransform[0].rotation.eulerAngles.x);
            _rotation.Add(targetObjectTransform[0].rotation.eulerAngles.y);
            _rotation.Add(targetObjectTransform[0].rotation.eulerAngles.z);

            string _name = "Base State";

            State _state = new State(_rotation, _name);

            // Store as Bezel coordinate
            states.Add("0", _state);
            
        }

        private IEnumerator TweenRotation(Quaternion fromRotation, Quaternion toRotation)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                // Calculate the normalized progress
                float normalizedTime = elapsedTime / duration;

                // Interpolate using linear interpolation
                Quaternion newRotation = Quaternion.Lerp(fromRotation, toRotation, normalizedTime);

                // Update target object
                targetObjectTransform[0].rotation = newRotation;
                // Increment the elapsed time
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            // Ensure the final position is set correctly
            targetObjectTransform[0].rotation = toRotation;
        }
    }
}