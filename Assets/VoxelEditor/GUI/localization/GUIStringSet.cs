// This class implements localizable strings for English.
// Other languages are implemented as subclasses that override these methods.
public class GUIStringSet
{
    public virtual string Yes =>
        "Yes";
    public virtual string No =>
        "No";
    public virtual string Ok =>
        "OK";
    public virtual string Done =>
        "Done";
    public virtual string Close =>
        "Close";
    public virtual string AreYouSure =>
        "Are you sure?";

    public virtual string DimensionSeparator =>
        "x";

    public virtual string LoadingWorld =>
        "Loading world...";

    // Main menu
    public virtual string WelcomeMessage =>
        "Welcome to N-Space\nFollowing the tutorial is recommended!";
    public virtual string StartTutorial =>
        "Tutorial";
    public virtual string CreateNewWorld =>
        "New World";
    public virtual string IndoorWorld =>
        "Indoor";
    public virtual string FloatingWorld =>
        "Floating";
    public virtual string PlayWorld =>
        "Play";
    public virtual string RenameWorld =>
        "Rename";
    public virtual string CopyWorld =>
        "Copy";
    public virtual string DeleteWorld =>
        "Delete";
    public virtual string ShareWorld =>
        "Share";
    public virtual string WorldNamePrompt =>
        "Enter new world name...";
    public virtual string WorldRenamePrompt(string oldName) =>
        $"Enter new name for {oldName}";
    public virtual string WorldDeleteConfirm(string name) =>
        $"Are you sure you want to delete {name}?";
    public virtual string ErrorCreatingWorld =>
        "Error creating world file";
    public virtual string ErrorSpecialCharacter =>
        "That name contains a special character which is not allowed.";
    public virtual string ErrorPeriodName =>
        "Name can't start with a period.";
    public virtual string ErrorWorldAlreadyExists =>
        "A world with that name already exists.";
    public virtual string UntitledWorldName(System.DateTime date) =>
        $"Untitled {date:yyyy-MM-dd HHmmss}";

    // Main menu overflow
    public virtual string OpenHelp =>
        "Help";
    public virtual string OpenAbout =>
        "About";
    public virtual string OpenWebsite =>
        "Website";
    public virtual string OpenSubreddit =>
        "Subreddit";
    public virtual string OpenVideos =>
        "Videos";
    public virtual string Donate =>
        "Donate";

    // Editor action bar
    public virtual string CreateObjectTitle =>
        "Create";
    public virtual string OpenWorldProperties =>
        "World";
    public virtual string SelectSubmenu =>
        "Select...";
    public virtual string SelectDraw =>
        "Draw";
    public virtual string SelectWithPaint =>
        "With Paint";
    public virtual string SelectFillPaint =>
        "Fill Paint";
    public virtual string SelectWithTag =>
        "With Tag";
    public virtual string OpenBevel =>
        "Bevel";
    public virtual string RevertChanges =>
        "Revert";
    public virtual string ConfirmRevertChanges =>
        "Undo all changes since the world was opened?";
    public virtual string SelectWithTagTitle =>
        "Select by tag";
    public virtual string SelectWithPaintInstruction =>
        "Tap to pick paint...";
    public virtual string SelectFillPaintInstruction =>
        "Tap to fill paint...";
    public virtual string CreateSubstanceInstruction =>
        "Push or pull to create a substance";
    public virtual string DrawSelectInstruction =>
        "Tap and drag to select";
    public virtual string EntityPickNone =>
        "None";
    public virtual string EntityPickInstruction =>
        "Pick an object...";
    public virtual string EntityPickCount(int count) =>
        $"{count} objects selected";

    // Type names
    public virtual string Substance =>
        "Substance";
    public virtual string Object =>
        "Object";
    
    // Bevel
    public virtual string BevelInstruction =>
        "Select edges to bevel...";
    public virtual string BevelHeader =>
        "Bevel:";
    public virtual string BevelNoSelection =>
        "(none selected)";
    public virtual string BevelShapeHeader =>
        "Shape:";
    public virtual string BevelSizeHeader =>
        "Size:";

    // Data import
    public virtual string ImportFile =>
        "Import file";
    public virtual string ImportFromWorldHeader =>
        "Or import from a world...";
    public virtual string NoDataInWorld(string type) =>
        $"World contains no {type} files.";

    // Filter
    public virtual string FilterSpecificObject =>
        "Specific object";
    public virtual string FilterTags =>
        "Tags";
    public virtual string FilterTagsTitle =>
        "Filter by tags";
    public virtual string FilterActiveBehavior =>
        "Active behavior";
    public virtual string FilterActiveBehaviorTitle =>
        "Filter by active behavior";
    public virtual string FilterAnything =>
        "Anything";

    // Target
    public virtual string TargetAny =>
        "Any";
    public virtual string TargetWorld =>
        "World";
    public virtual string TargetLocal =>
        "Local";
    public virtual string TargetPickObject =>
        "Pick object...";
    public virtual string TargetRandom =>
        "Random";
    public virtual string Center =>
        "Center";
    public virtual string North =>
        "North";
    public virtual string South =>
        "South";
    public virtual string East =>
        "East";
    public virtual string West =>
        "West";
    public virtual string Up =>
        "Up";
    public virtual string Down =>
        "Down";
    public virtual string Top =>
        "Top";
    public virtual string Bottom =>
        "Bottom";
    public virtual string NorthLetter =>
        "N";
    public virtual string SouthLetter =>
        "S";
    public virtual string EastLetter =>
        "E";
    public virtual string WestLetter =>
        "W";

    // Material panel
    public virtual string MaterialCustomCategory =>
        "CUSTOM";
    public virtual string MaterialImportFromWorld =>
        "Import from world...";
    public virtual string MaterialColorHeader =>
        "Adjust color";
    public virtual string ColorTintMode =>
        "Tint";
    public virtual string ColorPaintMode =>
        "Paint";
    public virtual string TextureDeleteConfirm =>
        "Are you sure you want to delete this custom texture?";
    public virtual string NoCustomMaterialsInWorld =>
        "World contains no custom textures for materials.";
    public virtual string NoCustomOverlaysInWorld =>
        "World contains no custom textures for overlays.";

    // Paint panel
    public virtual string PaintMaterial =>
        "Material";
    public virtual string PaintOverlay =>
        "Overlay";

    // Properties panel
    public virtual string PropertiesDifferent =>
        "different";
    public virtual string CloneEntity =>
        "Clone";
    public virtual string CloneInstruction =>
        "Tap to place clone";
    public virtual string DeleteEntity =>
        "Delete";
    public virtual string ChangeSensor =>
        "Change Sensor";
    public virtual string AddBehavior =>
        "Add Behavior";
    public virtual string RemoveBehavior =>
        "Remove";
    public virtual string OtherBehaviorsPlaceholder =>
        "(other behaviors...)";
    public virtual string SensorName(string name) =>
        $"{name} Sensor";
    public virtual string BehaviorName(string name) =>
        $"{name} Behavior";
    public virtual string NoSensor =>
        "No Sensor";
    public virtual string NoGeneric =>
        "None";
    public virtual string TargetEntity(string name) =>
        $"Target:  {name}";
    public virtual string EntityActivators => // plural!
        "Activators";

    // Property GUIs
    public virtual string EntityRefNone =>
        "None";
    public virtual string EntityRefSelf =>
        "Self";
    public virtual string EntityRefTarget =>
        "Target";
    public virtual string EntityRefActivator => // singular!
        "Activator";
    public virtual string RangeSeparator =>
        "to";
    public virtual string ChangeProperty(string name) =>
        $"Change {name}";
    public virtual string SelectProperty(string name) =>
        $"Select {name}";
    public virtual string BehaviorConditionHeader =>
        "When sensor is:";
    public virtual string SensorOn =>
        "On";
    public virtual string SensorOff =>
        "Off";
    public virtual string SensorBoth =>
        "Both";
    public virtual string WhenSensorIsOn =>
        "When sensor is On";
    public virtual string FilterByTitle =>
        "Filter by...";
    public virtual string Camera =>
        "Camera";
    public virtual string InputsHeader =>
        "Inputs:";
    public virtual string AddInput =>
        "Add Input";

    // Editor errors
    public virtual string WorldWarningsHeader =>
        "There were some issues with reading the world:";
    public virtual string UnknownSaveError =>
        "An error occurred while saving the file. Please send me an email about this, and include a screenshot of this message. chroma@chroma.zone\n\n";
    public virtual string UnknownReadError =>
        "An error occurred while reading the file.";

    // Help menu
    public virtual string HelpMenuTitle =>
        "Help";
    public virtual string HelpTutorials =>
        "Tutorials";
    public virtual string HelpDemoWorlds =>
        "Demo Worlds";
    public virtual string TutorialWorldName(string name) =>
        $"Tutorial - {name}";
    public virtual string DemoWorldName(string name) =>
        $"Demo - {name}";
    public virtual string TutorialIntro =>
        "Introduction";
    public virtual string TutorialPainting =>
        "Painting";
    public virtual string TutorialBevels =>
        "Bevels";
    public virtual string TutorialSubstances =>
        "Substances";
    public virtual string TutorialObjects =>
        "Objects";
    public virtual string TutorialTips =>
        "Tips and Shortcuts";
    public virtual string TutorialAdvancedGameLogic1 =>
        "Advanced Game Logic 1";
    public virtual string TutorialAdvancedGameLogic2 =>
        "Advanced Game Logic 2";
    public virtual string DemoDoors =>
        "Doors";
    public virtual string DemoHovercraft =>
        "Hovercraft";
    public virtual string DemoAI =>
        "Character AI";
    public virtual string DemoPlatforms =>
        "Platform Game";
    public virtual string DemoShapes =>
        "Shapes";
    public virtual string DemoLogic =>
        "Logic";
    public virtual string DemoImpossibleHallway =>
        "Impossible Hallway";
    public virtual string DemoConveyor =>
        "Conveyor";
    public virtual string DemoBallPit =>
        "Ball Pit";

    // Game UI
    public virtual string HealthCounterPrefix =>
        "Health: ";
    public virtual string ScoreCounterPrefix =>
        "Score: ";
    public virtual string YouDied =>
        "you died :(";
    public virtual string ResumeGame =>
        "Resume";
    public virtual string RestartGame =>
        "Restart";
    public virtual string OpenEditor =>
        "Editor";
    public virtual string CloseGame =>
        "Close";

    // Tutorial messages...
    // Tutorials use <i>italics</i> to mark commands for the user.
    public virtual string TutorialWelcome =>
        "Welcome! This is a brief tutorial that will guide you through the app. You can access this tutorial and others at any time. Press the right arrow to continue.";
    public virtual string TutorialRoom =>
        "Right now you're looking at the interior of a room. Two walls are hidden so you can see inside. The player is standing in the center.";
    public virtual string TutorialOrbit =>
        "Navigation: Use two fingers to rotate, and pinch to zoom. <i>Try looking around the room.</i> (tutorial will advance when you have completed this)";
    public virtual string TutorialPan =>
        "<i>Use three fingers to pan.</i> (If this doesn't work on your phone, try tapping the button in the bottom right to toggle pan/rotate mode.)";
    public virtual string TutorialSelectFace =>
        "<i>Tap with one finger to select a single face of a block.</i>";
    public virtual string TutorialPull(string axisName) =>
        $"<i>Pull the {axisName} arrow towards the center of the room to pull the block out.</i>";
    public virtual string TutorialAxisX =>
        "Red";
    public virtual string TutorialAxisY =>
        "Green";
    public virtual string TutorialAxisZ =>
        "Blue";
    public virtual string TutorialPush =>
        "<i>Now select a different face and push it away from the center of the room.</i>";
    public virtual string TutorialPushHint(string axisName) =>
        $"<i>(hint: use the {axisName} arrow)</i>";
    public virtual string TutorialColumn =>
        "<i>Now select a different face and pull it towards the center of the room. Keep pulling until you reach the other side.</i>";
    public virtual string TutorialBox =>
        "Tap and drag to select a group of faces in a rectangle or box. <i>Try this a few times.</i>";
    public virtual string TutorialWall =>
        "<i>Double tap to select an entire wall.</i>";
    public virtual string TutorialSculpt =>
        "By selecting faces and pushing/pulling them, you can sculpt the world.";
    public virtual string TutorialButtons =>
        "These buttons appear at the top of the screen, based on context.";
    public virtual string TutorialHelpMenu =>
        "That's enough to get started! You can access more tutorials by choosing Help in the menu.";
    public virtual string TutorialLinks =>
        "Also check out the video tutorials on YouTube and the subreddit. There are links in the main menu.";
    public virtual string TutorialPaintButton =>
        "<i>Select some faces and tap the paint roller icon to open the Paint panel.</i>";
    public virtual string TutorialPaintReopen =>
        "<i>Reopen the paint panel. (Select some faces and tap the paint roller icon)</i>";
    public virtual string TutorialPaintPanel =>
        "You can use the Paint panel to paint the selected faces with <i>materials</i> and <i>overlays</i>.";
    public virtual string TutorialPaintCategories =>
        "Choose any of the categories to browse for a texture. Then tap the Color button to change its color.";
    public virtual string TutorialPaintLayers =>
        "A paint is composed of two parts: an opaque material and a transparent overlay. Use the tabs to switch between the two parts.";
    public virtual string TutorialPaintTransform =>
        "Use these buttons to rotate and mirror the paint.";
    public virtual string TutorialPaintSky =>
        "The \"Sky\" material is special: in the game it is an unobstructed window to the sky. In an Indoor-type world, this is the only way to see the sky.";
    public virtual string TutorialBevelButton =>
        "<i>Tap the menu button then choose \"Bevel\" to open Bevel Mode.</i>";
    public virtual string TutorialBevelReopen =>
        "<i>Reopen Bevel Mode. (Tap the menu button and choose \"Bevel\")</i>";
    public virtual string TutorialBevelSelect =>
        "Instead of selecting faces, you can now select edges. <i>Tap and drag to select.</i>";
    public virtual string TutorialBevelShape =>
        "<i>Tap a bevel shape in the list to bevel the edge.</i>";
    public virtual string TutorialBevelSize =>
        "<i>Tap a size to change the size of the bevel.</i>";
    public virtual string TutorialBevelDoubleTap =>
        "<i>Double tap an edge to select all contiguous edges.</i>";
    public virtual string TutorialBevelExit =>
        "<i>When you're done, tap the check button to exit.</i>";
    public virtual string TutorialSubstanceIntro =>
        "In this tutorial you will build a moving platform using a <i>Substance.</i> Substances are independent objects that can move and respond to interaction.";
    public virtual string TutorialSubstancePit =>
        "First build a pit that is too wide and deep to cross by jumping.";
    public virtual string TutorialSubstanceCreateButton =>
        "Now we'll add a substance which will become a moving platform. <i>Select a row of faces on one side of the pit and tap the cube button.</i>";
    public virtual string TutorialSubstanceSolid =>
        "<i>Choose \"Solid Substance\".</i>";
    public virtual string TutorialSubstancePull =>
        "<i>Pull outwards to build a platform.</i>";
    public virtual string TutorialSubstanceBehaviors =>
        "Substances are controlled by their <i>Behaviors</i>. This substance has <i>Visible</i> and <i>Solid</i> behaviors which make it visible and solid in the game.";
    public virtual string TutorialSubstanceReselect =>
        "<i>Tap the substance to select it again.</i>";
    public virtual string TutorialSubstanceMoveBehavior =>
        "<i>Try adding a Move behavior to the platform.</i> Notice that behaviors are organized into multiple categories.";
    public virtual string TutorialSubstanceEditDirection =>
        "The Move behavior will make this substance move North at a constant speed. <i>Tap the direction to edit it.</i>";
    public virtual string TutorialSubstanceSetDirection =>
        "<i>Make it move toward the other side of the pit (look at the compass rose for guidance)</i>";
    public virtual string TutorialSubstancePlayTest =>
        "<i>Try playing your game.</i> The platform will move in one direction forever. We need to make it change directions at the end of the pit.";
    public virtual string TutorialSubstanceOnOff =>
        "Substances have two states, On and Off. Behaviors can be configured to only be active in the On or Off state.";
    public virtual string TutorialSubstanceMoveOpposite =>
        "<i>Add a second Move behavior, which moves the platform in the opposite direction.</i>";
    public virtual string TutorialSubstanceBehaviorConditions =>
        "<i>Now make one behavior active only in the Off state, and one in the On state.</i> (the substance will start Off).";
    public virtual string TutorialSubstanceSensor =>
        "A substance's On/Off state is controlled by a Sensor. <i>Give the platform a Pulse sensor. (under the Logic tab)</i> This will make it cycle on/off repeatedly.";
    public virtual string TutorialSubstanceTime =>
        "<i>Now adjust the time the sensor spends in the on and off state to make the platform move the full distance of the pit.</i>";
    public virtual string TutorialSubstancePlayFinal =>
        "<i>Play your game now.</i> If you built everything correctly, the platform should move across the pit and back repeatedly.";
    public virtual string TutorialSubstanceNext =>
        "Next try the <i>Objects</i> tutorial to learn about another type of interactive element.";
    public virtual string TutorialObjectSelect =>
        "<i>Select a face and tap the cube button.</i>";
    public virtual string TutorialObjectCreate =>
        "<i>Choose the Object tab, then choose Ball.</i>";
    public virtual string TutorialObjectExplain =>
        "You have just created a ball Object. Like substances, you can give Objects behaviors and sensors to add interactivity.";
    public virtual string TutorialObjectReselect =>
        "<i>Tap the ball to select it again.</i>";
    public virtual string TutorialObjectPaint =>
        "<i>Try painting the ball.</i>";
    public virtual string TutorialObjectAddMove =>
        "<i>Add a Move behavior to the ball.</i>";
    public virtual string TutorialObjectFollowPlayer =>
        "<i>Edit the Move behavior to make the ball follow the player.</i>";
    public virtual string TutorialObjectPlayTest =>
        "<i>Try playing your game.</i> Next we are going to make the ball hurt you when you touch it.";
    public virtual string TutorialObjectSensor =>
        "<i>Give the ball a Touch sensor.</i>";
    public virtual string TutorialObjectTouchPlayer =>
        "<i>Now configure the touch sensor so it only turns on when touching the player.</i>";
    public virtual string TutorialObjectAddTargetedBehavior =>
        "<i>Tap Add Behavior. In the behavior menu, tap the \"Target\" button and select the player as the target. Then choose Hurt/Heal under the \"Life\" tab.</i>";
    public virtual string TutorialObjectIncorrectTarget =>
        "You didn't set the target to the player. Remove the behavior and try again.";
    public virtual string TutorialObjectTargetExplain =>
        "By default, Hurt/Heal hurts the object it's attached to (the ball). By setting a Target, we made it act upon a different object (the player).";
    public virtual string TutorialObjectHurtOn =>
        "<i>Set Hurt/Heal to activate when the Sensor is On.</i> Even though it targets the Player, it will use the Ball's sensor to turn on/off.";
    public virtual string TutorialObjectHurtRate =>
        "<i>Set the Hurt/Heal rate to 1 to hurt repeatedly (every 1 second) as long as you're touching the ball.</i>";
    public virtual string TutorialObjectPlayFinal =>
        "<i>Play your game, and try to avoid dying!</i> You can change the speed of the ball and the hurt amount to adjust the difficulty.";
    public virtual string TutorialObjectCharacterBehavior =>
        "If you build some obstacles, you'll notice that the ball can float and move through walls. <i>Add a Character behavior to fix this. (check the Physics tab)</i>";
    public virtual string TutorialObjectNext =>
        "Read the <i>Advanced Game Logic</i> tutorial to learn how to add more complex interactivity to games.";
    public virtual string TutorialTipsMessage =>
@"•  Double tap to select an entire wall. The selection will be bounded by already-selected faces.
•  Triple tap a face to select <i>all</i> faces connected to it. The selection will be bounded by already-selected faces.
•  Triple tap a substance to select the entire substance.
•  Check the ""X-ray"" box of a substance to make it transparent in the editor only. This lets you see behind it and zoom through it.
•  The paint panel keeps shortcuts for the five most recent paints. To ""copy"" a paint to another face, select the source face, open and close the paint panel, then select the destination faces and use the recent paint shortcut.
•  Sliding faces sideways along a wall moves their paints, leaving a trail behind them.
•  Check the ""Select"" section in the menu for useful shortcuts to select faces and objects.
•  You can select multiple objects/substances to edit all of their properties at once.";
    public virtual string TutorialElevatorIntro =>
        "Here are three floors with an elevator connecting them. Your task is to make the elevator move when you press the buttons.";
    public virtual string TutorialElevatorNoChecks =>
        "(This tutorial will not check if you completed each step correctly. Good luck!)";
    public virtual string TutorialElevatorUpTap =>
        "We'll start by making the Up button on the first floor work. <i>Give it a Tap sensor.</i>";
    public virtual string TutorialElevatorToggle =>
        "<i>Now select the elevator, give it a Toggle sensor (Logic tab), and connect the On input to the Up button.</i>";
    public virtual string TutorialElevatorMoveOn =>
        "<i>Finally, use a Move behavior to make the elevator go up only when its sensor is On.</i>";
    public virtual string TutorialElevatorHelpButtons =>
        "To learn more about these sensors and behaviors, you can tap their icons in the left panel.";
    public virtual string TutorialElevatorPlayMoveUp =>
        "<i>Play your game.</i> The elevator should move up when you press the button. Did it work?";
    public virtual string TutorialElevatorNextFloor =>
        "The elevator needs to stop when it reaches the next floor. There are multiple ways to do this. One way is to make it turn off after 5 seconds...";
    public virtual string TutorialElevatorPulse =>
        "<i>Replace the elevator's sensor with Pulse (Logic tab). Change the On time to 5, and connect the input to the Up button.</i>";
    public virtual string TutorialElevatorPlayFloorStop =>
        "<i>Play your game.</i> The elevator should stop on the second floor. Why does it do this?";
    public virtual string TutorialElevatorMoreButtons =>
        "We have one functioning button, but there are still three more. <i>Give them all Tap sensors.</i> We'll make the second Up button work next...";
    public virtual string TutorialElevatorTooManyButtons =>
        "The elevator's Pulse sensor only allows one input, but we have two Up buttons. We need to merge them into a single input...";
    public virtual string TutorialElevatorHiddenRoom =>
        "<i>Find the hidden room near the elevator shaft.</i> This room will never be seen in the game, so it's a good place to hide extra logic components.";
    public virtual string TutorialElevatorUpCube =>
        "<i>In the hidden room, select the Up arrow cube. Give it a Threshold sensor, and connect two inputs to both of the Up buttons.</i>";
    public virtual string TutorialElevatorUpInput =>
        "<i>Now select the elevator. Change the sensor input to the Up arrow cube in the hidden room.</i>";
    public virtual string TutorialElevatorPlayBothUpButtons =>
        "<i>Play your game.</i> Both Up arrows should function correctly. Why does this work?";
    public virtual string TutorialElevatorDownIntro =>
        "Now the elevator needs to go down. This makes 3 possible states: going up, going down, and stopped. But sensors can only be On/Off...";
    public virtual string TutorialElevatorTargetedExplain =>
        "To solve this, we use Targeted Behaviors. Remember these behaviors use their host object to turn on/off, but act on a Target object.";
    public virtual string TutorialElevatorDownCube =>
        "<i>First, connect the hidden Down arrow cube to the two Down buttons, just like the Up arrow cube.</i>";
    public virtual string TutorialElevatorRedBall =>
        "<i>Next, select the red ball. Give it a Pulse sensor with On time = 5, and connect the input to the Down arrow cube.</i>";
    public virtual string TutorialElevatorMoveTarget =>
        "<i>Now tap Add Behavior. In the behavior menu, tap \"Target\" and select the elevator as the target. Then choose the Move behavior.</i>";
    public virtual string TutorialElevatorMoveDown =>
        "<i>Make the move behavior go Down only when the sensor is On.</i> The sensor of the red ball will turn it on/off, but the Elevator will move!";
    public virtual string TutorialElevatorPlayMoveDown =>
        "<i>Play the game and try the Down buttons.</i> The elevator should go down and stop at each floor. You're done!";
    public virtual string TutorialElevatorNext =>
        "How could you improve this elevator? Add more floors? Make it go faster? Add sliding doors? See what you can come up with!";
    public virtual string TutorialPitIntro =>
        "Your task is to build a Pit of Death. Anything that falls in the pit will die.";
    public virtual string TutorialPitBuild =>
        "<i>Make a large room with a pit in the middle.</i> Make sure the pit isn't directly under the player.";
    public virtual string TutorialPitTrigger =>
        "<i>Add a Trigger substance spanning the bottom of the pit.</i>";
    public virtual string TutorialPitNoEscape =>
        "The trigger should already have a Touch sensor with the filter set to Anything. This is good because nothing escapes the Pit of Death.";
    public virtual string TutorialPitHurtBehavior =>
        "<i>Tap Add Behavior. Tap the Target button, then tap \"Activators\". Then choose the Hurt/Heal behavior.</i>";
    public virtual string TutorialPitActivatorsExplain =>
        "This behavior targets its \"Activators\", which are the objects that \"cause\" the sensor to turn on -- in this case, any objects that touch the trigger.";
    public virtual string TutorialPitHurtPoints =>
        "<i>Make the trigger hurt the Activator by -100 points.</i>";
    public virtual string TutorialPitPlay =>
        "<i>Play your game. Try jumping in the pit.</i>";
    public virtual string TutorialPitBalls =>
        "<i>Now try making some balls with Physics behaviors. Then play the game and push them all into the pit.</i> Enjoy.";

    // Update messages
    public virtual string UpdateMessage_1_3_5 =>
@"N-Space has been updated! Check out the <b>Doors</b> Demo World in the help menu to see the new features.

•  New <b>Scale</b> behavior to change size of objects/substances
•  You can change the <b>Pivot</b> point of substances for rotation/scaling
•  Lights are visible in the editor, even when not selected
•  Objects can be placed inside walls or outside bounds
•  Fixed lag caused by larger substances
•  Button in the bottom right toggles pan/orbit with two fingers
... and some more improvements / fixes!";
}
