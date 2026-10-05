## Saving a player's health as Scriptable Variable

Let's assume that we have a player and we want to have multiple scripts referencing its health, perhaps the player as - of course - owner of that data while another component is trying to heal the player.

In a classic architecture you would have to create a direct link between the component and the player:

```csharp
class Player : MonoBehaviour {
    public float Health = 100;

    ...
}
```

```csharp
class Healer : MonoBehaviour {
    [SerializeField] Player _player;

    void Update(){
        HealPlayer(1f*Time.deltaTime);
    }

    public void HealPlayer(float amount){
        _player.Health += amount;
    }
}
```

Now what happens if I, for some reason (often debug), don't have a player in my scene? `NullObjectReference`

This is because a `MonoBehaviour` always lives in the scene, and guess what doesn't live in a scene? A `ScriptableObject`! So we can exploit this property to make better variables that can be modified with less hardlinks that clutter our code. 

Moreover we will see in a later page how holding data in a scriptable will be useful to bind data to UI without touching a single line of code.

Let's head to our asset explorer and right click, then navigate to `Create > Butter > Variables > Float`

![](/Docs/Assets/Float_Variable)

The scriptable you created will look something like this. The fields are self-explainatory, so we can move back to our scripts and use the newly created variable to sever the link between our classes.

```csharp
class Player : MonoBehaviour {
    public FloatVariable Health;

    ...
}
```

```csharp
class Healer : MonoBehaviour {
    public FloatVariable PlayerHealth;

    void Update(){
        HealPlayer(1f*Time.deltaTime);
    }

    public void HealPlayer(float amount){
        PlayerHealth.Value += amount;
    }
}
```

Now let's fill our scripts with the correct scriptable object.

![](/Docs~/Assets/Float_Variable_Assignment.png)

You can see that once filled out the inspector will show you the current value of the variable, as well as allow you to change it directly while inspecting the script.

We have now removed the hardlink between our scripts. If for some reason you need to create a new test scene with only the healer you won't have to import player and all its dependencies to test it out. In this simple context this may seem futile, but when your project gets bigger and dependencies start stacking on top of each other, this will save you a lot of time.

Finally, let's have polish our scripts a little bit more to show how you can subscribe to the variable's `OnValueChanged` event.

```csharp
class Player : MonoBehaviour {
    public FloatVariable Health;

    private void Awake() => Health.OnValueChanged += OnHealthChanged;

    private void OnHealthChanged(float newValue)
    {
        Debug.Log($"I have been healed! Now at {newValue}");
    }
}

```

```csharp
class Healer : MonoBehaviour {
    public FloatVariable PlayerHealth;

    void Awake() => StartCoroutine(HealPlayerCoroutine());

    private IEnumerator HealPlayerCoroutine()
    {
        while (this.isActiveAndEnabled)
        {
            HealPlayer(5f);
            yield return new WaitForSeconds(1f);
        }
    }

    public void HealPlayer(float amount){
        PlayerHealth.Value += amount;
    }
}
```

Now our healer will heal the player of 5 points each second and the player will print to console every time the value is changed.

![](/Docs~/Assets/Float_Variable_Print.png)

Neat, isn't it? Next up is how to use Scriptable Events to power up your code and stop using event managers.