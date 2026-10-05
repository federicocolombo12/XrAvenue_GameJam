using Dev.Nicklaj.Butter;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAIRuntimeSet", menuName = "4. Runtime Sets/EnemyAI Runtime Set")]
public class ButterSample_EnemyAIRuntimeSet : RuntimeSetBase<ButterSample_EnemyAI>
{
    
}

#region Custom Editor
[CustomEditor(typeof(ButterSample_EnemyAIRuntimeSet), true)]
public class GameObjectRuntimeSetDrawer : RuntimeSetDrawer<ButterSample_EnemyAI>
{
    public override void DrawItems()
    {
        foreach (ButterSample_EnemyAI item in (target as ButterSample_EnemyAIRuntimeSet).Items)
        {
            EditorGUILayout.ObjectField(item, typeof(ButterSample_EnemyAI), true);
        }
    }
}
#endregion

