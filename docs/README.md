# N-Space

<img src="https://raw.githubusercontent.com/vanjac/voxel-editor/master/Assets/icon.png" alt="Icon" width="128">

A mobile app for building 3D interactive worlds. This is a work in progress.

- [Google Play](https://play.google.com/store/apps/details?id=com.vantjac.voxel)
- [iOS App Store](https://itunes.apple.com/us/app/n-space/id1448016814) (currently unavailable due to development costs)
- [Video tutorials](https://www.youtube.com/playlist?list=PLMiQPjIk5IrpgNcQY5EUYaGFDuAf7PLY2)

## Features

- Sculpt indoor/outdoor 3D environments. It uses a voxel sculpting interface inspired by the Portal 2 Puzzle Maker, which is easy to use and very efficient for building complex spaces.
- Paint surfaces with a selection of over 100 built-in materials/overlays, or import your own from your photos library.
- Use the Bevel tool to create complex shapes including rounded edges and stair steps.
- Use "substances" to build dynamic worlds with moving objects, water, triggers, and physics.
- Simplified component system similar to Unity, and an extensive logic system for wiring up complex game logic. You can even build very simple "AI."
- Customize the sky, lighting, and fog.
- Import your own sound effects / music.
- Experience your creations from a first person perspective. Make a game or just an interesting environment to explore.
- Interactive tutorials and demos guide you through the interface and more advanced features of the app.
- World files can be sent to/from other apps.

# Screenshots

<span><img src="https://user-images.githubusercontent.com/8228102/206854943-364af43e-7b16-403d-9479-253b4b2f6b39.png" width="427" height="240">
<img src="https://user-images.githubusercontent.com/8228102/206854950-f061274c-e1c9-4227-a9fa-67d78c2b3da8.png" width="427" height="240">
<img src="https://user-images.githubusercontent.com/8228102/206854952-2faa8a2d-8eb6-4d52-a750-ee18973ae01f.png" width="427" height="240"></span>

## Using the app

The app opens to a list of "worlds" that you have created. Tap "New" to create a new world. Tap a world to open it in the Editor. Tap the menu button next to a world to access additional options, including sharing the worlds as `.nspace` files. You can open these files in N-Space from another app, like an email attachment or downloaded file.

N-Space has built-in documentation and tutorials, which you can access through the overflow (<img src="https://user-images.githubusercontent.com/8228102/206855184-e73ee339-7490-478c-a93a-d57609127541.png" width="24" height="24">) menu, by tapping Help.

## Building the app yourself

The app has been tested with Unity 2019.4.X on Android and iOS. There is little platform-specific code (only for importing/exporting files). Most of the user interface requires touch input and will not work with a mouse, so you will need to use the Unity Remote App, or build for Android directly.

This repository does not come with textures from [Poliigon](https://www.poliigon.com/), [FreePBR](https://freepbr.com/), [3dtextures](3dtextures.me), and [Genetica](http://spiralgraphics.biz/genetica.htm). You can purchase/download them yourself - look in the folders inside `Assets/GameAssets` for a list of `.meta` files which correspond to the missing textures. It is also possible to test N-Space without the textures at all. Materials will lack texture, but you can paint the walls with colors instead.

The app has four scenes:

- `Menu/menuScene`: The file selection menu
- `VoxelEditor/editScene`: The Editor interface. If you open this scene directly without first choosing a file, it will look for a file called `mapsave`.
- `Game/playScene`: The gameplay interface. Again, without choosing a file it will look for `mapsave`.
- `Menu/fileReceiveScene`: This scene will launch if you try to open a world file from another app using N-Space. This only works on Android.

### Build Environment Setup

These notes are mostly for me, but you can read them too.

- iOS: install Xcode
- Install latest release of Unity 2019, with build tools for Android/iOS.
- Clone repo. **Don't open in Unity yet!!**
- Optional: Download/copy textures from Poliigon, FreePBR, 3dtextures, and Genetica (see above)
- Open project in Unity, and switch platform to Android/iOS
- Open Project Settings > Editor and switch Unity Remote device to Android
- Android: open Player settings and browse for the keystore location.
- iOS: download certificate from Apple and install, get private key and install

## More info

[MIT License](https://github.com/vanjac/voxel-editor/blob/master/LICENSE.txt)

See [credits.txt](https://github.com/vanjac/voxel-editor/blob/master/Assets/Menu/credits.txt) for sources of assets and libraries.

## Contact me:

chroma [at] chroma [dot] zone
