using System;
using UnityEditor;
using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    public abstract class VariableDrawer<T> : PropertyDrawer
    {
        // Dictionary to track variables we're displaying and their current value text
        private static readonly System.Collections.Generic.Dictionary<int, string> valueDisplayCache = 
            new System.Collections.Generic.Dictionary<int, string>();  
         
        // Keep track of which variables we've already subscribed to for change events
        private static readonly System.Collections.Generic.HashSet<int> subscribedVariables =
            new System.Collections.Generic.HashSet<int>();
            
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ScriptableVariable<T> variable = property.objectReferenceValue as ScriptableVariable<T>;
            bool IsNull = variable == null;
            // Height for the object field plus the value display (with padding)
            return IsNull ? EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Get the current value of the property
            ScriptableVariable<T> variable = property.objectReferenceValue as ScriptableVariable<T>;
            bool IsNull = variable == null;
            
            // Split the rect into two rows
            Rect objectFieldRect = new Rect(
                position.x, 
                position.y, 
                position.width, 
                IsNull ? position.height : EditorGUIUtility.singleLineHeight
            );
            
            // The object field for the scriptable variable
            EditorGUI.ObjectField(
                objectFieldRect, 
                property, 
                typeof(ScriptableVariable<T>), 
                label
            );

            
            // Handle the value display
            if (!IsNull)
            {
#if UNITY_6000_0_OR_NEWER
                int variableId = variable.GetEntityId().GetHashCode();
#else
                int variableId = variable.GetInstanceID();
#endif
                
                Rect valueLabelRect = new Rect(
                    position.x + 20, // Indent the value display
                    position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, 
                    position.width - 20, 
                    EditorGUIUtility.singleLineHeight
                );
                
                // Subscribe to value changes if we haven't already
                if (!subscribedVariables.Contains(variableId))
                {
                    variable.OnValueChanged += _ => {
                        // Update the cached display value and force a repaint
                        valueDisplayCache[variableId] = DisplayString(variable);
                        try
                        {
                            EditorUtility.SetDirty(property.serializedObject.targetObject);
                        }
                        catch (Exception) { }

                    };
                    subscribedVariables.Add(variableId);
                    
                    // Initialize the display value
                    valueDisplayCache[variableId] = DisplayString(variable);
                }

                try
                {
                    DrawField(valueLabelRect, variable);    
                } catch(NullReferenceException) {}
                
            }
            else
            {
                // No variable selected, show nothing in the value field
                //EditorGUI.LabelField(valueLabelRect, "");
            }
            
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// String representation of the ScriptableVariable. Implement this method to define how the variable will be displayed in the inspector.
        /// </summary>
        /// <param name="scriptableVariable"></param>
        /// <returns></returns>
        protected abstract string DisplayString(ScriptableVariable<T> scriptableVariable);

        protected abstract void DrawField(Rect rect, ScriptableVariable<T> variable);
    }
    
    #region Drawers
    [CustomPropertyDrawer(typeof(BoolVariable))]
    public class BoolVariableDrawer : VariableDrawer<bool>
    {
        protected override string DisplayString(ScriptableVariable<bool> scriptableVariable)
        {
            return $"{scriptableVariable.Value}";
        }

        protected override void DrawField(Rect rect, ScriptableVariable<bool> variable)
        {
            EditorGUI.BeginChangeCheck();
            var arg = EditorGUI.Toggle(
                rect,
                "Value",
                variable.Value
            );
            if (EditorGUI.EndChangeCheck())
            {
                variable.Value = arg;    
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(FloatVariable))]
    public class FloatVariableDrawer : VariableDrawer<float>
    {
        protected override string DisplayString(ScriptableVariable<float> scriptableVariable)
        {
            return $"{scriptableVariable.Value}";
        }
        
        protected override void DrawField(Rect rect, ScriptableVariable<float> variable)
        {
            EditorGUI.BeginChangeCheck();
            var arg = EditorGUI.FloatField(
                rect,
                "Value",
                variable.Value
            );
            if (EditorGUI.EndChangeCheck())
            {
                variable.Value = arg;
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(IntVariable))]
    public class IntVariableDrawer : VariableDrawer<int>
    {
        protected override string DisplayString(ScriptableVariable<int> scriptableVariable)
        {
            return $"{scriptableVariable.Value}";
        }
        
        protected override void DrawField(Rect rect, ScriptableVariable<int> variable)
        {
            EditorGUI.BeginChangeCheck();
            var arg = EditorGUI.IntField(
                rect,
                "Value",
                variable.Value
            );
            if (EditorGUI.EndChangeCheck())
            {
                variable.Value = arg;    
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(StringVariable))]
    public class StringVariableDrawer : VariableDrawer<string>
    {
        protected override string DisplayString(ScriptableVariable<string> scriptableVariable)
        {
            return $"{scriptableVariable.Value}";
        }
        
        protected override void DrawField(Rect rect, ScriptableVariable<string> variable)
        {
            EditorGUI.BeginChangeCheck();
            var arg = EditorGUI.TextField(
                rect,
                "Value",
                variable.Value
            );
            if (EditorGUI.EndChangeCheck())
            {
                variable.Value = arg;    
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(Vector2Variable))]
    public class Vector2VariableDrawer : VariableDrawer<Vector2>
    {
        protected override string DisplayString(ScriptableVariable<Vector2> scriptableVariable)
        {
            return $"{scriptableVariable.Value}";
        }
        
        protected override void DrawField(Rect rect, ScriptableVariable<Vector2> variable)
        {
            EditorGUI.BeginChangeCheck();
            var arg = EditorGUI.Vector2Field(
                rect,
                "Value",
                variable.Value
            );
            if (EditorGUI.EndChangeCheck())
            {
                variable.Value = arg;
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(Vector3Variable))]
    public class Vector3VariableDrawer : VariableDrawer<Vector3>
    {
        protected override string DisplayString(ScriptableVariable<Vector3> scriptableVariable)
        {
            return $"{scriptableVariable.Value}";
        }
        
        protected override void DrawField(Rect rect, ScriptableVariable<Vector3> variable)
        {
            EditorGUI.BeginChangeCheck();
            var arg = EditorGUI.Vector3Field(
                rect,
                "Value",
                variable.Value
            );
            if (EditorGUI.EndChangeCheck())
            {
                variable.Value = arg;
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(QuaternionVariable))]
    public class QuaternionVariableDrawer : VariableDrawer<Quaternion>
    {
        protected override string DisplayString(ScriptableVariable<Quaternion> scriptableVariable)
        {
            return $"{scriptableVariable.Value}";
        }
        
        protected override void DrawField(Rect rect, ScriptableVariable<Quaternion> variable)
        {
            EditorGUI.BeginChangeCheck();
            var arg = EditorGUI.Vector3Field(
                rect,
                "Value",
                variable.Value.eulerAngles
            );
            if (EditorGUI.EndChangeCheck())
            {
                variable.Value = Quaternion.Euler(arg);
            }
        }
    }
    #endregion
}
