# N-Space

<img src="https://raw.githubusercontent.com/vanjac/voxel-editor/master/Assets/icon.png" alt="Icon" width="128">

A mobile app for building 3D interactive worlds. This is a work in progress.

## Using the app

You can download N-Space for Android on [Google Play](https://play.google.com/store/apps/details?id=com.vantjac.voxel).

The app starts on a list of "worlds" that you have created. Tap "New" to create a new world. Tap a world's name to open it in the Editor. Tap the menu button next to a world to access additional options, including sharing the worlds as JSON files. You can open these files in N-Space from another app, like your Downloads.

N-Space has built-in documentation and tutorials, which you can access through the overflow (!["..."](https://github.com/vanjac/voxel-editor/blob/master/Assets/VoxelEditor/GUI/dots-vertical.png)) menu, by tapping Help.

## Building the app yourself

The app has been tested with Unity 2017.4.10 on Android. There's not much Android specific code though, so it could theoretically work on iOS. Most of the user interface requires touch input and will not work with a mouse, so you will need to use the Unity Remote App, or build for Android directly.

This repository does not come with textures from [Poliigon](https://www.poliigon.com/), [FreePBR](https://freepbr.com/), and others. You can purchase/download them yourself - look in the `Assets/GameAssets/Poliigon` and `Assets/GameAssets/FreePBR` folders for a list of `.meta` files which correspond to the missing textures. It is also possible to test N-Space without the textures at all. Materials will lack texture, but you can paint the walls with colors instead.

The app has four scenes:

- `Menu/menuScene`: The file selection menu
- `VoxelEditor/editScene`: The Editor interface. If you open this scene directly without first choosing a file, it will look for a file called `mapsave`.
- `Game/playScene`: The gameplay interface. Again, without choosing a file it will look for `mapsave`.
- `Menu/fileReceiveScene`: This scene will launch if you try to open a JSON file from another app using N-Space. This only works on Android.

## More info

See [credits.txt](https://github.com/vanjac/voxel-editor/blob/master/Assets/Menu/credits.txt) for sources of some assets.
