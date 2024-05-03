using System;

// Space Engineers DLLs
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using VRageMath;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;

/*
 * Must be unique per each script project.
 * Prevents collisions of multiple `class Program` declarations.
 * Will be used to detect the ingame script region, whose name is the same.
 */
namespace AirLockV1 {

/*
 * Do not change this declaration because this is the game requirement.
 */
public sealed class Program : MyGridProgram {

    /*
     * Must be same as the namespace. Will be used for automatic script export.
     * The code inside this region is the ingame script.
     */
    #region AirLockV1



    // Airlock Configuration
    // We need to know a few things about the airlock
    // - The airlock name. This is the prefix for all the blocks in the airlock
    string airlockBlocksPrefix = "Airlock";
    string innerDoorName = "Inner Door";
    string outerDoorName = "Outer Door";
    string airVentName = "Air Vent";


    IMyAirtightDoorBase innerDoor;
    IMyAirtightDoorBase outerDoor;
    IMyAirVent airVent;

    // State is a class that holds the current state of the airlock
    class State {
        public bool innerDoorOpen = false;
        public bool outerDoorOpen = false;
        public bool depressurized = false;
    }

    State state = new State();


    /*
     * The constructor, called only once every session and always before any
     * other method is called. Use it to initialize your script.
     *
     * The constructor is optional and can be removed if not needed.
     *
     * It's recommended to set RuntimeInfo.UpdateFrequency here, which will
     * allow your script to run itself without a timer block.
     */
    public Program() {
        // Search for all the blocks in the airlock
        List<IMyTerminalBlock> airlockBlocks = new List<IMyTerminalBlock>();
        GridTerminalSystem.SearchBlocksOfName(airlockBlocksPrefix, airlockBlocks);

        // Find the inner door
        innerDoor = GridTerminalSystem.GetBlockWithName(innerDoorName) as IMyAirtightDoorBase;
        if (innerDoor == null) {
            Echo("Inner door not found");
            return;
        }

        // Find the outer door
        outerDoor = GridTerminalSystem.GetBlockWithName(outerDoorName) as IMyAirtightDoorBase;
        if (outerDoor == null) {
            Echo("Outer door not found");
            return;
        }

        // Find the air vent
        airVent = GridTerminalSystem.GetBlockWithName(airVentName) as IMyAirVent;
        if (airVent == null) {
            Echo("Air vent not found");
            return;
        }
    }

    /*
     * Called when the program needs to save its state. Use this method to save
     * your state to the Storage field or some other means.
     *
     * This method is optional and can be removed if not needed.
     */
    public void Save() {}

    /*
     * The main entry point of the script, invoked every time one of the
     * programmable block's Run actions are invoked, or the script updates
     * itself. The updateSource argument describes where the update came from.
     *
     * The method itself is required, but the arguments above can be removed
     * if not needed.
     */
    public void Main(string argument, UpdateType updateSource) {
        
    }

    #endregion // AirLockV1
}}
