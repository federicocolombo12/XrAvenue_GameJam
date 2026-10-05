### Enabling persistency for Scriptable Variables
All games use save data to ensure that player progress isn't lost. Well Butter supports Variable Persistency with a simple drag and drop.

In the previous pages I've shown how to use Scriptable Variables and Scriptable Events. I will use these two components in this demo, so if you don't know what these are go back and explore the previous pages. This demo will be a followup to the scenario created in the first page of the docs.

To make your data persistent we'll have to use a new scriptable type. Introducing the `PersistentDataMasterList`. 

![](/Docs~/Assets/Master_List.png)

This element has a list that will contain all the variables you want to save, which will be saved under `Application.persistenDataPath + $"\{SavePath}.json"`.

> [!tip]
 You don't need to add `.json` to the save path. During the saving process it will be automatically added if it's not there.

You'll want to drag and drop your Scriptable Variables to the list for them to be considered during the saving process. Note that his list can be of mixed types, you don't have to create multiple master lists for different variable types, however you can create multiple master lists to separate where your data is saved (eg. options vs actual save data).

Create a new `GameObject` in the scene and add a `PersistentDataManager` component. This component exposes the `SaveData()` and `LoadData()` methods. To use it you will have to link the manager to the master list you're trying to save.

![](/Docs~/Assets/Persistent_Manager.png)

Whenever you want to save or load you can call those methods, however, let's spice things up. Rememeber the Scriptable Events? Yeah, let's start using them properly. Create two new `GameEvents`, which I will call `SaveEvent` and `LoadEvent`. These are plain events with no argument.

Go back to the GameObject we created earlier and add two `GameEventListener` components and link them up to `SaveEvent` and `LoadEvent`, which will call `SaveData()` and `LoadData()`.

![](/Docs~/Assets/Persistency_Manager_Full.png)

Remember how we started from where we left things in the first demo page? Well now we can finally drag and drop `PlayerHealth` in the master list and fire up our `SaveEvent` while in Play Mode.

This will create the json save data and whenever you raise `LoadEvent`, the data will be loaded up seamlessly.

![](/Docs~/Assets/Load_Console.png)

Using events is not the only way you can invoke these two methods of course, but as you can see this allows you to cut down on the amount of code required to do a lot of stuff.

> [!warning]
> Scriptable Variables are still subject to the `Reset On` field, which by default is set to `Scene Loaded`. In other words, if you find that when changing scene your variables lose their loaded state, simply change the `Reset On` field to `Application Start` or `None`.