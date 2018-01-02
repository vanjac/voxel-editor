# voxel-editor

A mobile app for building 3D interactive worlds.

The app has been tested with Unity 2017.2 on Android. Most of the user interface requires touch input and will not work with a mouse, so you will need to use the Unity Remote App, or build for Android directly.

The first scene is `Menu/menuScene`. It's a placeholder interface for what will eventually be a file selection menu. Right now you can type a file name in the upper box and press New to make a new file, then choose the file in the dropdown and press Open to edit the file.

This will open `VoxelEditor/editScene`. If you open this scene directly from Unity without choosing a map file, it will look for a file called "mapsave".

## Editor Interface

- Use 2 fingers to rotate and zoom. You are looking at the *interior* of a room, not the exterior of a box. Walls facing away from you are hidden, allowing you to see inside.
- Use 3 fingers to pan
- Tap to select the face of a single block. Tap and drag for a rectangular/box selection
- Double tap to select all contiguous faces
- Drag one of the 3 axes (red/green/blue lines) to move selected faces/objects and "sculpt" the world

### Toolbar icons

Some of these only appear in certain contexts.

- Back arrow: close the map
- 2 overlapping dotted boxes: select something else, in addition to tbe current selection
- Box with a line through it: clear selection
- Paint roller: show the Paint interface (this is a placeholder interface right now) allowing you to paint the selected faces
- Cube: create a Substance. Substances are independent objects that can move, change, and respond to interaction
- Play: play the map, allowing you to walk around and interact with objects
- Overflow menu...
    - World: edit global World properties

### Properties panel

This appears when a substance is selected. You can swipe left to temporarily hide it (swipe right to show).

TODO
