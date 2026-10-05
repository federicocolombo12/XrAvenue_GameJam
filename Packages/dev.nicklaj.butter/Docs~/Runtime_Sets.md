## Keeping track of all entities in a scene using a runtime set
In some occasions it can be convenient to keep track of a list of GameObjects or script instances in a scene, and there are some design patterns that allow to easily do that. As example, if you wanted to keep track of all enemies of a specific type you could use a factory pattern that can easily implement such behaviour

Scriptable Objects also allow to easily keep track of objects (or script instances) via runtime sets, that basically are just scriptable lists that guarantee unique items. 

There are two ways to go about creating runtime sets: if you want to save reference to the GameObjects you can use the component integrated with Butter otherwise you'll have to derive its base class to create your own component.

### Keeping track of Game Objects
This is the easiest method, as it doesn't require any additional code at all to work.

Let's assume we have a simple spawning script that spawns enemies. Enemies come from a prefab `PEnemy`. As usual, to create a new runtime set right click in the asset explorer `Create > Butter > Runtime Sets > GameObject Runtime Set`.

![](/Docs~/Assets/Runtime_Set.png)

Everything is pretty straightforward with runtime sets, however there is one thing you should take not of. Unity **cannot** serialize `GameObjects` references from Scriptables, not without a custom inspector at the very least. If you tried to access the list directly from the inspector you would see an ominous `Mismatched Type` in every entry, hence I have disabled the list view and instead I'm showing a custom debug-only list of all the subscribed objects.

Now, back to the task let's go to our `PEnemey` prefab and add a new `RuntimeSetSubscriber` component. This only takes a runtime set as a field, so assign it and you're done. 

Now in any other script you can reference the set that will be updated your runtime informations.

![](/Docs~/Assets/Runtime_Set_Full.png)

### Keeping track of custom scripts
To create a new type of runtime set, all you have to do is create a new class that extends from `RuntimeSetBase` and give it a type.

```csharp
[CreateAssetMenu(fileName = "EnemyAIRuntimeSet", menuName = "Scriptable Objects/EnemyAI Runtime Set")]
public class EnemyAIRuntimeSet : RuntimeSetBase<EnemyAI> { }

#region Custom Editor
[CustomEditor(typeof(EnemyAIRuntimeSet), true)]
public class GameObjectRuntimeSetDrawer : RuntimeSetDrawer<EnemyAI>
{
    public override void DrawItems()
    {
        foreach (EnemyAI item in (target as EnemyAIRuntimeSet).Items)
        {
            EditorGUILayout.ObjectField(item, typeof(EnemyAI), true);
        }
    }
}
#endregion
```

The custom editor is not strictly necessary when creating your own type, however without it you won't be able to see the current elements in the inspector when focusing the set. They will still be there, you just won't be able to see them.

Then, either in your script or by creating a new subscriber you can assign the GameObject to the runtime set. 

```csharp
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyAIRuntimeSet _runtimeSet;

    private void OnEnable()
    {
        _runtimeSet.Add(this);
    }

    private void OnDisable()
    {
        _runtimeSet.Remove(this);
    }

    ...
}
```

Both approaches work perfectly fine, although I would argue that to ensure Single Responsibility using a subscriber would be better.

```csharp
[RequireComponent(typeof(EnemyAI))]
public class EnemyAISubscriber : MonoBehaviour
{
    [SerializeField] private EnemyAIRuntimeSet _runtimeSet;
    [SerializeField] private EnemyAI _enemyAI
    {
        get => _enemyAI ??= GetComponent<EnemyAI>();
        set => _enemyAI = value;
    }

    private void OnEnable()
    {
        _runtimeSet.Add(_enemyAI);
    }

    private void OnDisable()
    {
        _runtimeSet.Remove(_enemyAI);
    }
}
```