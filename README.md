# voxel-editor

A mobile app for building 3D interactive worlds. This is a work in progress. It doesn't have a name yet (Voxel Editor is a placeholder), but I'm working on that.

## Using the app

The app will be availible on the Google Play Store soon. Until then, you can download an older version from the [Releases](https://github.com/vanjac/voxel-editor/releases) page.

The app starts on a list of "worlds" that you have created. Tap "New" to create a new world. Tap a world's name to open it in the Editor. Tap the menu button next to a world to access additional options, including sharing the worlds as JSON files. You can open these files in Voxel Editor from another app, like your Downloads.

The Editor has built-in documentation and tutorials, which you can access through the overflow (!["..."](https://github.com/vanjac/voxel-editor/blob/master/Assets/VoxelEditor/GUI/dots-vertical.png)) menu, by tapping Help.

## Building the app yourself

The app has been tested with Unity 2017.4.2 on Android. There's not much Android specific code though, so it could theoretically work on iOS. Most of the user interface requires touch input and will not work with a mouse, so you will need to use the Unity Remote App, or build for Android directly.

This repository does not come with textures from [Poliigon](https://www.poliigon.com/), [FreePBR](https://freepbr.com/), and others. You can purchase/download them yourself - look in the `Assets/GameAssets/Poliigon` and `Assets/GameAssets/FreePBR` for a list of `.meta` files which correspond to the missing textures. It is also possible to test the app without the textures at all. Materials will lack texture, but you can paint the walls with colors instead.

The app has four scenes:

- `Menu/menuScene`: The file selection menu
- `VoxelEditor/editScene`: The Editor interface. If you open this scene directly without first choosing a file, it will look for a file called `mapsave`.
- `Game/playScene`: The gameplay interface. Again, without choosing a file it will look for `mapsave`.
- `Menu/fileReceiveScene`: This scene will launch if you try to open a JSON file from another app using Voxel Editor. This only works on Android.

## More info

See [credits.txt](https://github.com/vanjac/voxel-editor/blob/master/Assets/Menu/credits.txt) for sources of some assets.
