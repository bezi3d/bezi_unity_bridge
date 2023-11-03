using System.Collections.Generic;
using UnityEngine;

namespace Bezi.Bridge
{
    public class BeziBehavior : MonoBehaviour
    {
        [SerializeField]
        private UDictionary<string, State> states;
        [SerializeField]
        private UDictionary<string, Interaction> interactions;

        public bool ContainsStates = false;
        public bool ContainsInteractions = false;

        // Todo: Implement a proper set/get. This will require heavy rewrite to scale to transform array. Trigger multiple objects, each with different action...
        // [HideInInspector]
        // public Transform[] targetObjectTransform = new Transform[1];

        // Constructor
        public void AttachBeziBehavior(UDictionary<string, State> _states, UDictionary<string, Interaction> _interactions) {

            states = _states;
            interactions = _interactions;

            ContainsStates = true;
            ContainsInteractions = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (!ContainsStates || !ContainsInteractions)
            {
                return;
            }

            // Todo: Configure following rotations from bezi state and interactions
            //Prepare Base State
            //PrepareBaseState();

            // This can only be set after import, e.g. Start().
            // Unless targetObjectTransform is ready first.
            // For now, targetObjectTransform reference is inserted during import.
            // Todo: Configure following rotations once implement proper states in unity
        }

        // Update is called once per frame
        void Update()
        {
            if (!ContainsStates || !ContainsInteractions) {
                return;
            }
        }

        private void PrepareBaseState() {
            BeziVector3 _rotation = new BeziVector3();

            // Todo: Setup rotations or other parameters as base state 
            // For example, _rotation = targetObjectTransform's rotation

            string _name = "Base State";

            State _state = new State(_rotation, _name);

            // Store as Bezi coordinate
            states.Add("0", _state);
        }
    }
}