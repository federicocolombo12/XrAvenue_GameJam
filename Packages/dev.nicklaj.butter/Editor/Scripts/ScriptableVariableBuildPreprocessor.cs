using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    public class ScriptableVariableBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; }
        
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("[Build] Resetting all ScriptableVariable<T> instances before build...");
            
            // Load all ScriptableObjects in the project
            string[] guids = AssetDatabase.FindAssets("t:RuntimeScriptableObject");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RuntimeScriptableObject obj = AssetDatabase.LoadAssetAtPath<RuntimeScriptableObject>(path);

                if (obj == null)
                    continue;
                
                obj.Reset();
                EditorUtility.SetDirty(obj); // Mark it dirty to persist changes
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log("[Build] Scriptable variables reset complete.");
        }
    }
}