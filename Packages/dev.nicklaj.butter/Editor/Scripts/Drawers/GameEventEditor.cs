using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Dev.Nicklaj.Butter
{
    /// <summary>
    /// Custom editor for any ScriptableObject that implements ISerializedRaise interface
    /// </summary>
    public abstract class GameEventEditor<T> : Editor
    {
        private string inputArgument = "";
        private uint inputChannel = 0;
        private ISerializedRaise raiseTarget;
        private GameEvent<T> gameEvent;
        private bool isRaiseTarget = false;

        private void OnEnable()
        {
            // Check if target implements ISerializedRaise
            raiseTarget = target as ISerializedRaise;
            gameEvent = target as GameEvent<T>;
            isRaiseTarget = raiseTarget != null;
        }

        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // If not implementing ISerializedRaise, don't add anything
            if (!isRaiseTarget)
                return;

            // Add separator
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // Test event section
            EditorGUILayout.LabelField("Test Event", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();

            inputChannel = (uint)Mathf.Clamp(EditorGUILayout.LongField("Channel", (long)inputChannel), 0, uint.MaxValue);

            // Only show input field if the event has an argument
            if (raiseTarget.HasArgument)
            {
                // Get the scriptable type name to give a hint to the user
                string typeName = target.GetType().Name;
                string placeholder = GetPlaceholderFromTypeName(typeName);

                // Draw argument field
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Debug Value");
                inputArgument = EditorGUILayout.TextField(inputArgument);
                // Create a button to raise the event
                if (GUILayout.Button("Raise Event"))
                {
                    // Call the implementation's OnRaiseButtonSubmit method
                    raiseTarget.OnRaiseButtonSubmit(inputArgument);
                }
                EditorGUILayout.EndHorizontal();

                // Optional: Add hint about expected input type
                EditorGUILayout.HelpBox($"Expected format: {placeholder}", MessageType.Info);
            }else
            {
                // Create a button to raise the event
                if (GUILayout.Button("Raise Event"))
                {
                    // Call the implementation's OnRaiseButtonSubmit method
                    raiseTarget.OnRaiseButtonSubmit(inputArgument, inputChannel);
                }
            }
            
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Attached Listeners", EditorStyles.boldLabel);
            
            if (GetListeners(target).Count > 0)
            {
                foreach (var listener in GetListeners(target))
                {
                    if (GUILayout.Button(listener.Target.name))
                    {
                        Selection.activeObject = listener.Target;
                        EditorGUIUtility.PingObject(listener.Target);
                    }  
                }  
            }
            else
            {
                EditorGUILayout.HelpBox("No listeners Attached", MessageType.Info);
            }
        }
        
        /// <summary>
        /// Helper method to provide a hint based on the event type name
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private string GetPlaceholderFromTypeName(string typeName)
        {
            if (typeName.Contains("Int")) return "Integer value (e.g. 42)";
            if (typeName.Contains("Float")) return "Float value (e.g. 3.14)";
            if (typeName.Contains("String")) return "Text value";
            if (typeName.Contains("Bool")) return "true or false";
            if (typeName.Contains("Vector2")) return "x,y (e.g. 1,2)";
            if (typeName.Contains("Vector3")) return "x,y,z (e.g. 1,2,3)";

            return "Enter value";
        }
        
        /// <summary>
        /// Method used to return the listeners to the GameEventEditor. It's necessary to override it to maintain proper address structure.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected abstract List<IGameEventListener<T>> GetListeners(Object target);
    }
    
    #region Drawers
    [CustomEditor(typeof(BoolEvent), true)]
    public class BoolEventDrawer : GameEventEditor<bool>
    {
        protected override List<IGameEventListener<bool>> GetListeners(Object target) => (target as BoolEvent)?.Listeners;
    }
    
    
    [CustomEditor(typeof(StringEvent), true)]
    public class StringEventDrawer : GameEventEditor<string>
    {
        protected override List<IGameEventListener<string>> GetListeners(Object target) => (target as StringEvent)?.Listeners;
    }
    
    [CustomEditor(typeof(GameEvent), true)]
    public class GameEventDrawer : GameEventEditor<Unit>
    {
        protected override List<IGameEventListener<Unit>> GetListeners(Object target) => (target as GameEvent)?.Listeners;
    }
    
    [CustomEditor(typeof(IntEvent), true)]
    public class IntEventDrawer : GameEventEditor<int>
    {
        protected override List<IGameEventListener<int>> GetListeners(Object target) => (target as IntEvent)?.Listeners;
    }
    
    [CustomEditor(typeof(Vector2Event), true)]
    public class Vector2EventDrawer : GameEventEditor<Vector2>
    {
        protected override List<IGameEventListener<Vector2>> GetListeners(Object target) => (target as Vector2Event)?.Listeners;
    }
    
    [CustomEditor(typeof(FloatEvent), true)]
    public class FloatEventDrawer : GameEventEditor<float>
    {
        protected override List<IGameEventListener<float>> GetListeners(Object target) => (target as FloatEvent)?.Listeners;
    }
    
    [CustomEditor(typeof(Vector3Event), true)]
    public class Vector3EventDrawer : GameEventEditor<Vector3>
    {
        protected override List<IGameEventListener<Vector3>> GetListeners(Object target) => (target as Vector3Event)?.Listeners;
    }

    [CustomEditor(typeof(QuaternionEvent), true)]
    public class QuaternionEventDrawer : GameEventEditor<Quaternion>
    {
        protected override List<IGameEventListener<Quaternion>> GetListeners(Object target) => (target as QuaternionEvent)?.Listeners;
    }
    #endregion
}

