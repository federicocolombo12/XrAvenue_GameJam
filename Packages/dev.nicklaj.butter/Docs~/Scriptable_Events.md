## Using events as channels to communicate across components
A classic trope in game development is having a the game manager or in some other singleton manager that holds all or most of your UnityEvents. 

This section will show you how to use scriptables events to be more flexible in how you manage your cross-component communication.

Let's imagine this scenario: we have an boxcollider with no renderer to act as a trigger area, when a set amount of entities enter the area, a sphere falls from the sky. This could as example be a boulder trap, when the player walks in the area the boulder activates. We could use a `GameEvent`, which doesn't take any parameter but I want to showcase a cool feature later.

For the sake of the tutorial I will assume you know how to setup a rigidbody on the sphere and a trigger area to the box collider.

![](/Docs~/Assets/Scene_Event.png)

As usual the first step is to go the asset explorer and create a new scriptable event of type int, which you can find under `Create > Butter > Events`. 

![](/Docs~/Assets/Area_Event.png)

> [!tip]
> During play mode you'll be able to see all the listeners attached to the event when inspecting the scriptable. Clicking on any of them will automatically focus the GameObject in your Scene Hierarchy.

You don't have any specific field to set in this event, however you can see that from this tab you can manually Raise the event, and even specify a debug value for it. Neat, let's move on.

Now we have an event but we need something to listen for events. So let's take our sphere and add a `IntEventListener`. Link the event you want to listen for and then whenever it is raised it will execute all the functions assigned to the `UnityEvent` field `Response`.

In our case we want the sphere to enable its renderer component and the gravity of the rigidbody, so the listener will look something like this:

![](/Docs~/Assets/Sphere_Listener.png)

Finally let's bind the box collider to raise the event OnTriggerEnter:

```csharp
[RequireComponent(typeof(BoxCollider))]
public class TriggerArea : MonoBehaviour
{
    [SerializeField] private IntEvent _triggeredEvent;
    public int MaxEntities = 1;
    [SerializeField] private int _currentEntities = 0;

    private void OnTriggerEnter(Collider other)
    {
        if((++_currentEntities) >= MaxEntities) 
            _triggeredEvent.Raise(_currentEntities);
    }
}
```

Let's enter play mode and if you setup everything correctly, when the player enters the trigger area the boulder should fall down.

![](/Docs~/Assets/Sphere_Fall.png)

> [!tip]
One of the properties of the `UnityEvent` is that it calls the assigned methods in order, so you could also, as example, use an event listener to define the execution order of the setup functions of an event manager, without having to bind anything to the `Awake()`. 
>
>If your function flow is in the incorrect order, just reorder them in the response and there you go, problem solved, you won't have to edit your code to fix it.