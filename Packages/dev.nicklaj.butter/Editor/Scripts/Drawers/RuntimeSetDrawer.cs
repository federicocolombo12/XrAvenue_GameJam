using UnityEngine;
using UnityEditor;

namespace Dev.Nicklaj.Butter
{
    public abstract class RuntimeSetDrawer<T> : Editor
    {
        private IRuntimeSet raiseTarget;
        private bool isRaiseTarget = false;
        
        private void OnEnable()
        {
            // Check if target implements ISerializedRaise
            raiseTarget = target as IRuntimeSet;
            isRaiseTarget = raiseTarget != null;
        }

        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();
            if (!isRaiseTarget) return;
            int elems = (target as RuntimeSetBase<T>).Items.Count;
            string suffix = elems > 0 ? "(" + elems + ")" : "";
            EditorGUILayout.LabelField($"Values {suffix}");
            if ((target as RuntimeSetBase<T>).Items.Count > 0)
                DrawItems();
            else
                EditorGUILayout.HelpBox($"Set is empty", MessageType.None);
            EditorGUILayout.HelpBox($"This list is for visualization purposes only, do not modify.", MessageType.Info);
        }

        public abstract void DrawItems();
    }
    
    #region Drawers
    [CustomEditor(typeof(GameObjectRuntimeSet), true)]
    public class GameObjectRuntimeSetDrawer : RuntimeSetDrawer<GameObject>
    {
        public override void DrawItems()
        {
            foreach (GameObject item in (target as GameObjectRuntimeSet).Items)
            {
                EditorGUILayout.ObjectField(item, typeof(GameObject), true);
            }
        }
    }
    
    
    #endregion
}