This library is my personal implementation of the SOAP architecture.
It's free to use for personal and non-commercial projects.

Most of my knowledge has been taken from these sources:
- [Unite Austin 2017 - Game Architecture with Scriptable Objects](https://youtu.be/raQ3iHhE_Kk)
- [Improve your Game Architecture with Scriptable Objects](https://youtu.be/bO8WOHCxPq8)
- [Game architecture with ScriptableObjects | Open Projects Devlog](https://youtu.be/WLDgtRNK2VE)
- [Create modular game architecture in Unity with ScriptableObjects](https://unity.com/resources/create-modular-game-architecture-with-scriptable-objects-ebook)


## Features
Before starting this section, keep in mind that Scriptable Objects live in the game's memory outside of a scene, and are generally publicly accessible by all memebers, which is why they are a good way to implement data decoupling and dependency inversion.

For use cases check out the [wiki](https://github.com/xNicklaj/Butter/wiki).

### Scriptable Variables
A Scriptable Variable is a runtime variable that can represent a piece of data that you want to reference from different points. It can be player health, score, a simple flag to check whether you pressed New Game or Continue in the main menu... you can do pretty much anything with it, but it's supposed to be an atomic piece of data.

Scriptable Variables can also have persistent states, meaning that you can easily save and restore their state.

### Scriptable Events
Scriptable Events are scriptable objects that make cross-component communication very easy and fexible.

### Runtime Sets
Runtime sets are, just like the name suggests, sets that live in a Scriptable Object. 

These can be useful to quickly define sets of items that you might want to reference somewhere else without having to create a hard link between scripts.
In his talk at Unity, Ryan Hipple explains how at Schell Games they used this technique to quickly add some enemies to a set and reference them from another script that applied a custom renderer on top of them.

### Installation
Navigate to the unity package manager, press the + icon to add a package and select "*Install Package from git URL*". Finally input `https://github.com/xNicklaj/Butter.git` as URL.

This should install the whole library. To check that it's working you can right click anywhere in your *Assets* folder and make sure that under *Create* you have `Butter`

### TODO
This is a list of features I'd like to see implemented. The order does not necessarily imply priority.

- [ ] Add Object Pools as native scriptables.
- [ ] Custom editor renderer for list properties and dictionaries.
- [X] Native color variable.
- [ ] Custom icons for variables, events and sets.
