# MultiMod
Unity Assets and Scripts - *Together*!

MultiMod has two main parts:
- A **Unity Editor extension** for exporting your assetmod
- A **BepInEx plugin** to load your assetmod (and other multimod assetmods)

MultiMod assetmods also have two main parts:
- An **assembly DLL** containing your compiled C# code
- A **Unity Assetbundle** containing your prefabs, scenes and scriptable-objects

## What's an Assetmod?

Using assetbundles with Unity modding isn't new. So what's special here?

- You don't need to write a Bepin Plugin
- You build your assetmod in Unity, using the normal workflow
- Prefabs loaded from your assetbundle will **have scripts properly attached**
- Prefabs loaded from your assetbundle will **have proper references to other prefabs**

This has a lot of advantages. If you're building UI, you can build and test it right within the Unity Editor where you can see and play with it. If you're building new prefabs, you can test them out in the Editor's play-mode, assuming your scripts don't depend on any game classes.

Even if your prefabs do depend on game classes, you may not be able to test them in play-mode, but you can still construct them in the Editor. When you put your own scripts onto your prefabs, those scripts will be properly loaded out of your assetmod DLL. Without MultiMod, you must add your scripts to GameObjects at runtime once they're loaded (or have a deeper understanding of Unity and do what MultiMod does yourself, manually).

With MultiMod, you just build your assetmod in Unity, export, and run. 

The ExampleMod is a great example of this simplicity. Check it out.

## Installation

MultiMod is a Bepin plugin, so it is installed just like any other. MultiMod also has its own `mods/` folder where assetmods go.

### Installing the Bepin Plugin

- [Install BepInEx](https://github.com/risk-of-thunder/R2Wiki/wiki/BepInEx)
- Download the latest [multimod.7z](https://github.com/risk-of-thunder/MultiMod/releases)
- Extract `multimod/` to your `BepInEx/plugins/` folder

### Installing the ExampleMod assetmod

- Download the latest [ExampleMod.7z](https://github.com/risk-of-thunder/MultiMod/releases)
- Extract `ExampleMod` to your `BepInEx/plugins/multimod/mods/` folder
- Run the game

### Building from source

- Clone this repository somewhere
- Build `MultiMod.sln`
- Make sure VS refreshes NuGet packages. We depend on `Mono.Cecil`

The assemblies will be copied into `ExampleMod/Assets/MultiMod/` and to `build/multimod/`. The latter is a ready to use Bepin plugin distribution. So copy or symlink it to your `BepInEx/plugins/` folder to use.

To utilize the `.bat` files, you'll need to add the following executables to your path:
- `msbuild.exe`
- `Unity.exe`

