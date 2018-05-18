# voxel-editor

A mobile app for building 3D interactive worlds. This is a work in progress.

The app has been tested with Unity 2017.4.2 on Android. There's not much Android specific code though, so it could theoretically work on iOS. Most of the user interface requires touch input and will not work with a mouse, so you will need to use the Unity Remote App, or build for Android directly. There are also some prebuilt APKs in the Releases section.

The app starts on the file selection menu (scene: `Menu/menuScene`). You can create new files by tapping "New", open files by tapping their name in the list, and view additional options by tapping the "..." button next to their name.

On Android, you can send files in JSON format, and open them from another app (like your Downloads).

> If you are testing in the Unity Editor and open the Editor scene (`VoxelEditor/editScene`) directly without first choosing a file, it will look for a file called `mapsave`.

## Editor Interface

The app has built-in documentation and tutorials. You can access tutorials through the overflow ("...") menu, by tapping Help.
