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
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

/*
 * Must be unique per each script project.
 * Prevents collisions of multiple `class Program` declarations.
 * Will be used to detect the ingame script region, whose name is the same.
 */
namespace testing {

/*
 * Do not change this declaration because this is the game requirement.
 */
public sealed class Program : MyGridProgram {

    /*
     * Must be same as the namespace. Will be used for automatic script export.
     * The code inside this region is the ingame script.
     */
    #region testing

    /// <summary>
    /// Airlock script for Space Engineers
    /// </summary>
    //

    //////////////////////////////////////////////////////////////////////
    /// Configuration section.
    /// You can change these values to match your airlock setup.
    /// Do not change the code below this section.
    /// //////////////////////////////////////////////////////////////////

    // groupName is the name of the group that contains the airlock blocks
    readonly string groupName = "Airlock";

    // innerDoorSuffix and outerDoorSuffix are the suffixes that we use to
    // identify the inner and outer doors in the group.
    readonly string innerDoorSuffix = "inner door";
    readonly string outerDoorSuffix = "outer door";

    //////////////////////////////////////////////////////////////////////
    /// End of configuration section.
    /// Do not change the code below this section.
    /// //////////////////////////////////////////////////////////////////

    // Create a list of references to blocks we will need later
    public List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

    // Airlock class that contains the airlock logic
    class Airlock {
        public IMyDoor innerDoor;
        public IMyDoor outerDoor;
        public IMyAirVent vent;

        // Actions is a list of commands that we queue up to execute
        // in order. This is useful for ensuring that the airlock
        // doesn't get into a bad state.
        // We put actions into the queue when we receive a command.
        private Queue<string> actions = new Queue<string>();

        public Airlock(IMyDoor innerDoor, IMyDoor outerDoor, IMyAirVent vent) {
            this.innerDoor = innerDoor;
            this.outerDoor = outerDoor;
            this.vent = vent;
        }

        public int NumberOfRemainingActions() {
            return actions.Count;
        }

        public string GetStatus() {
            return "[AIRLOCK] "
                    + "Inner door: " + innerDoor.Status +
                    ", Outer door: " + outerDoor.Status +
                    ", Vent: " + vent.GetOxygenLevel() +
                    ", Actions: " + actions.Count +
                    " next: " + (actions.Count > 0 ? actions.Peek() : "none");
        }

        public bool CommandClose() {
            // If there are any actions in the queue, we can't start a new command
            // because we need to finish the current actions first.
            if (actions.Count != 0) {
                return false;
            }
            actions.Enqueue(Action.CloseDoors);
            actions.Enqueue(Action.LockDoors);
            return true;
        }

        public bool CommandOpenInwards() {
            // If there are any actions in the queue, we can't start a new command
            // because we need to finish the current actions first.
            if (actions.Count != 0) {
                return false;
            }
            actions.Enqueue(Action.CloseDoors);
            actions.Enqueue(Action.LockDoors);
            actions.Enqueue(Action.PressurizeAirlock);
            actions.Enqueue(Action.OpenInnerDoor);
            return true;
        }

        public bool CommandOpenOutwards() {
            // If there are any actions in the queue, we can't start a new command
            // because we need to finish the current actions first.
            if (actions.Count != 0) {
                return false;
            }
            actions.Enqueue(Action.CloseDoors);
            actions.Enqueue(Action.LockDoors);
            actions.Enqueue(Action.DepressurizeAirlock);
            actions.Enqueue(Action.OpenOuterDoor);
            return true;
        }

        public string Update() {
            if (actions.Count == 0) {
                return "[AIRLOCK] " + "No actions to perform";
            }

            string action = actions.Peek();
            string completedAction = null;
            switch (action) {
            case Action.CloseDoors:
                if (innerDoor.Status == DoorStatus.Closed && outerDoor.Status == DoorStatus.Closed) {
                    completedAction = actions.Dequeue();
                    break;
                }
                if (innerDoor.Status != DoorStatus.Closed && innerDoor.Status != DoorStatus.Closing) {
                    if (innerDoor.Enabled == false) {
                        innerDoor.Enabled = true;
                    }
                    innerDoor.CloseDoor();
                }
                if (outerDoor.Status != DoorStatus.Closed && outerDoor.Status != DoorStatus.Closing) {
                    if (outerDoor.Enabled == false) {
                        outerDoor.Enabled = true;
                    }
                    outerDoor.CloseDoor();
                }
                break;
            case Action.LockDoors:
                if (innerDoor.Enabled == false && outerDoor.Enabled == false) {
                    completedAction = actions.Dequeue();
                    break;
                }
                if (innerDoor.Enabled == true) {
                    innerDoor.Enabled = false;
                }
                if (outerDoor.Enabled == true) {
                    outerDoor.Enabled = false;
                }
                break;
            case Action.PressurizeAirlock:
                if (vent.GetOxygenLevel() == 1.0f) {
                    completedAction = actions.Dequeue();
                    break;
                }
                if (vent.Depressurize == true) {
                    vent.Depressurize = false;
                }
                break;
            case Action.DepressurizeAirlock:
                if (vent.GetOxygenLevel() <= 0.01f) {
                    completedAction = actions.Dequeue();
                    break;
                }
                if (vent.Depressurize == false) {
                    vent.Depressurize = true;
                }
                break;
            case Action.OpenInnerDoor:
                if (innerDoor.Status == DoorStatus.Open || innerDoor.Status == DoorStatus.Opening) {
                    completedAction = actions.Dequeue();
                    break;
                }
                if (innerDoor.Enabled == false) {
                    innerDoor.Enabled = true;
                }
                innerDoor.OpenDoor();
                break;
            case Action.OpenOuterDoor:
                if (outerDoor.Status == DoorStatus.Open || outerDoor.Status == DoorStatus.Opening) {
                    completedAction = actions.Dequeue();
                    break;
                }
                if (outerDoor.Enabled == false) {
                    outerDoor.Enabled = true;
                }
                outerDoor.OpenDoor();
                break;
            }
            if (completedAction != null) {
                return "[AIRLOCK] " + "Completed action: " + completedAction;
            }
            return null;
        }

    }


    // Actions is a set of actions that can be performed on the airlock
    public static class Action {
        public const string CloseDoors = "close doors";
        public const string LockDoors = "lock doors";
        public const string PressurizeAirlock = "pressurize airlock";
        public const string DepressurizeAirlock = "depressurize airlock";
        public const string OpenInnerDoor = "open inner door";
        public const string OpenOuterDoor = "open outer door";
        public const string EnableInnerDoor = "enable inner door";
        public const string EnableOuterDoor = "enable outer door";
    }

    public static class Command {
        // OpenInwards is the command to set the airlock in a state
        // where the airlock is pressurized, inner door is open,
        // and outer door is closed and locked.
        public const string OpenInwards = "open inwards";

        // OpenOutwards is the command to set the airlock in a state
        // where the airlock is depressurized, inner door is closed and locked,
        // and outer door is open.
        public const string OpenOutwards = "open outwards";

        // Close is the command to set the airlock in a state
        // where both doors are closed and locked.
        public const string Close = "close";
    }

    Airlock airlock;

    public Program() {
        // Get the group and check if it exists
        IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(groupName);
        if (group == null) {
            Echo("[ERROR] " + "Group not found: " + groupName);
            return;
        }

        // Get the blocks in the group
        group.GetBlocks(blocks);

        // Print the number of blocks in the group
        Echo("[INFO] " + "Blocks in group: " + blocks.Count);

        // Find the doors and vent in the group
        IMyDoor innerDoor = null;
        IMyDoor outerDoor = null;
        IMyAirVent vent = null;

        foreach (IMyTerminalBlock block in blocks) {
            if (block is IMyDoor) {
                IMyDoor door = block as IMyDoor;
                if (door.CustomName.ToLower().EndsWith(innerDoorSuffix)) {
                    innerDoor = door;
                } else if (door.CustomName.ToLower().EndsWith(outerDoorSuffix)) {
                    outerDoor = door;
                }
            } else if (block is IMyAirVent) {
                vent = block as IMyAirVent;
            }
        }

        // Check if we found all the blocks
        if (innerDoor == null) {
            Echo("[ERROR] " + "Inner door not found");
            return;
        }
        if (outerDoor == null) {
            Echo("[ERROR] " + "Outer door not found");
            return;
        }
        if (vent == null) {
            Echo("[ERROR] " + "Vent not found");
            return;
        }

        // Create the airlock object
        airlock = new Airlock(innerDoor, outerDoor, vent);

        // Set the update frequency
        // We initially set it to None because we don't need to update
        // every X ticks. We only need to update when we receive a command.
        Runtime.UpdateFrequency = UpdateFrequency.None;
    }

    public void Main(string argument, UpdateType updateSource) {
        Echo(airlock.GetStatus());
        // Check if the update source is a valid trigger.
        // Only then should we execute the command.
        if (updateSource == UpdateType.Terminal || updateSource == UpdateType.Trigger) {
            bool started; // Whether the command was started or not
            switch (argument) {
                case Command.OpenInwards:
                    started = airlock.CommandOpenInwards();
                    break;
                case Command.OpenOutwards:
                    started = airlock.CommandOpenOutwards();
                    break;
                case Command.Close:
                    started = airlock.CommandClose();
                    break;
                default:
                    Echo("[ERROR] " + "Invalid command: " + argument);
                    return;
            }
            if (started) {
                // Start automatic updates until we finish all tasks queued by the command.
                Runtime.UpdateFrequency = UpdateFrequency.Update10;
            } else {
                Echo("[MAIN] " + "Could not start command: " + argument);
            }
        } else if (updateSource == UpdateType.Update10) {
            string message = airlock.Update();
            // If we got a message, then at least one action was completed
            // (or there were none to begin with)
            if (message != null) {
                Echo(message);
            }
            if (airlock.NumberOfRemainingActions() == 0) {
                // No more actions to perform, so we can stop updating
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
        }
    }

    #endregion // testing
}}
