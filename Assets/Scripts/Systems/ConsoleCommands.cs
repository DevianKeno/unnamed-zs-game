using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Crafting;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Items;

using static UZSG.Systems.CommandPermissionLevel;
using static UZSG.Systems.CommandLocationConstraint;

namespace UZSG.Systems
{
    /// <summary>
    /// Developer console for invoking system commands.
    /// </summary>
    public sealed partial class Console : MonoBehaviour, IInitializeable
    {
        Command CreateCommand(CreateCommandOptions options)
        {
            // Create a new command using the options
            Command newCommand = new()
            {
                Name = options.Name,
                Syntax = options.Syntax,
                Description = options.Description,
                Aliases = options.Aliases,
                PermissionLevel = options.PermissionLevel,
                LocationConstraint = options.LocationConstraint,
                Callbacks = options.Callbacks
            };

            // Parse the syntax into arguments if not provided
            string[] args = options.Syntax.Split(" ");
            string substr;
            foreach (string arg in args[1..])
            {
                if (arg.Contains("|"))
                {
                    // Represents AND arguments
                    // Currently unhandled logic
                    string[] split = arg.Split("|");
                }

                if (arg.StartsWith("<") && arg.EndsWith(">")) // Required argument
                {
                    substr = arg[1..^1];
                    newCommand.Arguments.Add(new()
                    {
                        Type = Command.ArgumentType.Required
                    });
                }
                else if (arg.StartsWith("[") && arg.EndsWith("]")) // Optional argument
                {
                    substr = arg[1..^1];
                    newCommand.Arguments.Add(new()
                    {
                        Type = Command.ArgumentType.Optional
                    });
                }
            }

            // Add the command to the dictionary
            _commandsDict[options.Name] = newCommand;

            return newCommand;
        }

        /// <summary>
        /// Create a new Console command.
        /// </summary>
        Command CreateCommand(string command, string description = "")
        {
            string[] args = command.Split(" ");

            Command newCommand = new()
            {
                Name = args[0],
                Syntax = command,
                Description = description
            };

            string substr;
            foreach (string arg in args[1..])
            {
                if (arg.Contains("|"))
                {
                    /// Represents AND arguments
                    /// Currently unhandled logic
                    string[] split = arg.Split("|");
                }

                if (arg.StartsWith("<") && arg.EndsWith(">")) /// Required argument
                {
                    substr = arg[1..^1];
                    newCommand.Arguments.Add(new()
                    {
                        Type = Command.ArgumentType.Required
                    });

                } else if (arg.StartsWith("[") && arg.EndsWith("]")) /// Optional argument
                {
                    substr = arg[1..^1];
                    newCommand.Arguments.Add(new()
                    {
                        Type = Command.ArgumentType.Optional
                    });
                }
            }

            _commandsDict[args[0]] = newCommand; 
            return newCommand;           
        }

        void InitializeCommands()
        {
            Log("Initializing console command registry...");

            /// Arguments enclosed in <> are required, [] are optional
            
            CreateCommand("attribute <target> <attribute>",
                          "Modify an attribute of a given object.")
                          .OnInvoke += CAttr;

#region TEST

            /// /attribute devi stamina value 100
            // CreateCommand(new CreateCommandOptions()
            // {
            //     Name = "attribute",
            //     Description = "Modify an attribute of a given object.",
            //     Syntax = "attribute <target> <attribute> <property> <value>",
            //     Aliases = { "attr" }, 
            //     PermissionLevel = OperatorOnly,
            //     LocationConstraint = WorldOnly,
            //     Callbacks = { CAttr, /*CClear*/ }
            // });

#endregion

            CreateCommand("clear",
                          "Clears the console messages.")
                          .OnInvoke += CClear;

            CreateCommand("freecam",
                          "Toggle Free Look Camera.")
                          .OnInvoke += CFreecam;

            CreateCommand("craft <item_id>",
                          "Crafts item given the item_id")
                          .OnInvoke += CCraft;

            CreateCommand("entity <query> <id>",
                          "Queries the Entities of Id present in the current world.")
                          .OnInvoke += CEntity;
            
            CreateCommand("give <player|me> <item_id> [amount]",
                          "Gives the player the item.")
                          .OnInvoke += CGive;
            
            CreateCommand("help",
                          "Prints help message.")
                          .OnInvoke += CHelp;

            CreateCommand("say <message>",
                          "Send a message.")
                          .OnInvoke += CSay;
            
            CreateCommand("spawn <entity_id> [x] [y] [z]",
                          "Spawns an entity.")
                          .OnInvoke += CSpawn;

            CreateCommand("teleport <x> <y> <z>",
                          "Teleport to coordinates.")
                          .OnInvoke += CTeleport;

            CreateCommand("tick <set|freeze> [value]",
                          "Manipulate in-game tick system.")
                          .OnInvoke += CTick;
                          
            CreateCommand("time <set> <value>",
                          "")
                          .OnInvoke += CTime;
            
            CreateCommand("wbcraft <item_id>",
                        "Crafts item if player is interacting with workbench")
                        .OnInvoke += CWbcraft;

            CreateCommand("world <create|load> <world_name>",
                          "")
                          .OnInvoke += CWorld;

            // /spawn item "bandage" 1

            // CreateCommand("tick <freeze|set> <value>",
            //               "Control the game's tick rate.").AddCallback(Command_Tick);
        }


        #region Command implementations

        /// The args parameters represent the arguments WITHOUT the actual command.
        /// ex. "/spawn item bandage"
        /// The args would consist of ["item", "bandage"]

        /// <summary>
        /// Clears the console messages.
        /// </summary>
        void CAttr(object sender, string[] args)
        {
            throw new NotImplementedException();
        }        
        
        /// <summary>
        /// Clears the console messages.
        /// </summary>
        void CClear(object sender, string[] args)
        {
            Messages.Clear();
        }        
        
        /// <summary>
        /// Spawns an entity.
        /// </summary>
        void CCraft(object sender, string[] args)
        {
            var newItem = new Item(Game.Items.GetData(args[0]));
            var playerInventoryCrafter = (PlayerCrafting)_player.Crafter;
            playerInventoryCrafter.PlayerCraftItem(newItem.Data.Recipes[0]);
        }

        void CDamage(object sender, string[] args)
        {
            
        }

        void CEntity(object sender, string[] args)
        {
            if (args[0] == "query")
            {
                var id = args[1];
                if (Game.Entity.IsValidId(id))
                {
                    var ettyList = Game.World.CurrentWorld.GetEntitiesById(id);
                    var msg = $"Entity Id: '{id}' | Count: {ettyList.Count}\n";
                    foreach (var etty in ettyList)
                    {
                        msg += $"{etty.Id}, ";
                    }
                }

                return;
            }
            
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Clears the console messages.
        /// </summary>
        void CFreecam(object sender, string[] args)
        {
            
        }
        
        /// <summary>
        /// Gives the player the item.
        /// </summary>
        void CGive(object sender, string[] args)
        {
            if (args.Length > 1)
            {
                var target = args[0];
                var id = args[1];
                int count = 1;
                if (args.Length > 2)
                {
                    count = string.IsNullOrEmpty(args[2]) ? 1 : int.Parse(args[2]);
                }
                var newItem = new Item(Game.Items.GetData(id), count);

                if (target == "me") /// target self
                {
                    _player.Inventory.Bag.TryPutNearest(newItem);
                    Game.Console.Log($"Given {_player.name} {count} of '{id}'");
                }
                else /// target player Id
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Prints the help message to the console.
        /// </summary>
        void CHelp(object sender, string[] args)
        {
            if (args.Length <= 1)
            {
                Game.Console.WriteLine("List of available commands: ");
                string message = "";
                foreach (var c in _commandsDict)
                {
                    message += $"{c.Value.Name}, "; /// This will have an extra comma, pls fix
                }
                Game.Console.Write(message);
            }
            else
            {
                if (int.TryParse(args[1], out int page))
                {
                    Log($"Showing page {page}");
                }
                else
                {
                    Log($"Usage: " + _commandsDict[args[1]].Syntax);
                    Log(_commandsDict[args[1]].Description);
                }
            }
        }
        
        /// <summary>
        /// Prints a message to the console.
        /// </summary>
        void CSay(object sender, string[] args)
        {
            WriteLine(string.Join(" ", args));
        }
        
        /// <summary>
        /// Spawns an entity.
        /// </summary>
        void CSpawn(object sender, string[] args)
        {
            if (args.Length == 1)
            {
                Game.Entity.Spawn(args[0]);
            }
            else
            {
                if (float.TryParse(args[1], out float x) &&
                    float.TryParse(args[2], out float y) &&
                    float.TryParse(args[3], out float z))
                {
                    var position = new Vector3(x, y, z);
                    Game.Entity.Spawn<Player>(args[0], position);
                    return;
                }
                throw new System.Exception();
            }
        }

        /// <summary>
        /// Spawns an entity.
        /// </summary>
        void CTeleport(object sender, string[] args)
        {
            if (int.TryParse(args[1], out int x))
            if (int.TryParse(args[2], out int y))
            if (int.TryParse(args[3], out int z))
            {
                
            }
        }

        /// <summary>
        /// Spawns an entity.
        /// </summary>
        void CTick(object sender, string[] args)
        {
            if (args[0] == "freeze")
            {
                Game.Tick.SetFrozen(!Game.Tick.IsFrozen);
            }
            else if (args[0] == "set")
            {
                if (int.TryParse(args[1], out int value))
                {
                    Game.Tick.SetTPS(value);
                }
                else
                {
                    Game.Console.Log("TPS must be a positive integer value.");
                }
            }
        }

        void CTime(object sender, string[] args)
        {
            if (args[0] == "set")
            {
                if (int.TryParse(args[1], out int value))
                {
                    Game.World.CurrentWorld.Time.SetTime(value);
                }
                else
                {
                    Game.Console.Log("Time must be a positive integer value.");
                }
            }
        }

        /// <summary>
        /// Creates access to external workbench
        /// </summary>
        void CWbcraft(object sender, string[] args)
        {
            // var wbcraft = (WorkbenchCrafter)_player.ExternalCrafter;
            // var newItem = new Item(Game.Items.GetData(args[0]));
            // if (wbcraft == null){
            //     print("Player is not interacting with workbench");
            //     return;
            // }
            // wbcraft.WorkbenchCraftItem(_player, newItem.Data.Recipes[0]);
        }

        /// <summary>
        /// World commands.
        /// </summary>
        void CWorld(object sender, string[] args)
        {
            if (args[0] == "create")
            {
                // Game.World.CreateWorld(args[1]);

            } else if (args[0] == "load")
            {
                string worldName = args[1];

            } else
            {
                PromptInvalid("world");
            }
        }
        
        #endregion
    }
}
