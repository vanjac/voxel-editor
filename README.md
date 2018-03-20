# voxel-editor

A mobile app for building 3D interactive worlds. This is a work in progress.

The app has been tested with Unity 2017.3 on Android. There's no Android specific code though, so it could theoretically work on iOS. Most of the user interface requires touch input and will not work with a mouse, so you will need to use the Unity Remote App, or build for Android directly. There are also some prebuilt APKs in the Releases section.

The first scene is `Menu/menuScene`. This is a file selection menu. You can create new files by tapping "New...", open files in the editor by tapping their name in the list, and delete files by tapping the "X" next to their name. Currently the menu only works correctly on Android.

Opening a map will open `VoxelEditor/editScene`. If you open this scene directly from Unity without choosing a map file, it will look for a file called "mapsave".

## Editor Interface

- Use 2 fingers to rotate and zoom. You are looking at the *interior* of a room, not the exterior of a box. Walls facing away from you are hidden, allowing you to see inside.
- Use 3 fingers to pan
- Tap to select the face of a single block. Tap and drag for a rectangular/box selection
- Double tap to select all contiguous faces
- Triple tap to select an entire substance (see below)
- Drag one of the 3 axes (red/green/blue lines) to move selected faces/objects and "sculpt" the world

### Toolbar icons

Some of these only appear in certain contexts.

- ![Back arrow](https://raw.githubusercontent.com/vanjac/voxel-editor/master/Assets/VoxelEditor/GUI/arrow-left.png): Close the map
- ![2 overlapping dotted boxes](https://raw.githubusercontent.com/vanjac/voxel-editor/master/Assets/VoxelEditor/GUI/vector-selection.png): Select something else, in addition to the current selection
- ![Box with a line through it](https://raw.githubusercontent.com/vanjac/voxel-editor/master/Assets/VoxelEditor/GUI/selection-off.png): Clear selection
- ![Paint roller](https://raw.githubusercontent.com/vanjac/voxel-editor/master/Assets/VoxelEditor/GUI/format-paint.png): Show the Paint interface allowing you to paint the selected faces
- ![Cube](https://raw.githubusercontent.com/vanjac/voxel-editor/master/Assets/VoxelEditor/GUI/cube-send.png): Create a Substance. Substances are independent objects that can move, change, and respond to interaction
- ![Play](https://raw.githubusercontent.com/vanjac/voxel-editor/master/Assets/VoxelEditor/GUI/play.png): Play the map, allowing you to walk around and interact with objects
- ![3 dots](https://raw.githubusercontent.com/vanjac/voxel-editor/master/Assets/VoxelEditor/GUI/dots-vertical.png) Overflow menu...
    - ![](https://raw.githubusercontent.com/vanjac/voxel-editor/master/Assets/VoxelEditor/GUI/earth.png) World: Edit global World properties

## Objects

Besides the walls forming the boundaries of the world, there will be different type of objects you can create to add interactivity. Right now, there are only Substances (see above for how to create one) and the Player. You can tap an object to show the Properties panel (swipe left to temporarily hide it).

### Sensors and Behaviors

Behaviors can be added to an object to affect its behavior in the game. Behaviors will include Visible and Solid, as well as graphical effects, motion, and physics.

A single Sensor can be selected for each object. Sensors turn On and Off in response to specific events in the game. These events could include touching another object, interaction with the player, or responses to the On/Off states of other objects.

Behaviors can optionally be set to only be active when the Sensor is On of Off. This system allows complex interactive elements to be built.
