using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Entities;
using UZSG.Items;
using UZSG.UI.Players;
using UZSG.TitleScreen;
using UZSG.EOS;
using Unity.Netcode;
using UZSG.Parties;

namespace UZSG
{
    /// <summary>
    /// Developer console for invoking system commands.
    /// </summary>
    public sealed partial class Console : MonoBehaviour, IInitializeable
    {
        public Command CreateCommand(CreateCommandOptions options)
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
        Command CreateCommand(string command, string description = "", bool isDebugCommand = false)
        {
            string[] args = command.Split(" ");

            Command newCommand = new()
            {
                Name = args[0],
                Syntax = command,
                Description = description,
                IsDebugCommand = isDebugCommand,
            };

            string substr;
            foreach (string arg in args[1..])
            {
                if (arg.Contains("|"))
                {
                    /// Represents AND arguments
                    /// TODO: Currently unhandled logic
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
            LogInfo("Initializing console command registry...");

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

            CreateCommand("auth",
                          "Auth commands.")
                          .OnInvoke += CAuth;

            CreateCommand("clear",
                          "Clears the console messages.")
                          .OnInvoke += CClear;
                          
            CreateCommand("creative",
                          "Toggles creative menu ability.")
                          .OnInvoke += CCreative;

            CreateCommand("craft <item_id>",
                          "Crafts item given the item_id",
                          isDebugCommand: true)
                          .OnInvoke += CCraft;

            CreateCommand("entity <query> <id>",
                          "Queries the Entities of Id present in the current world.")
                          .OnInvoke += CEntity;
            
            CreateCommand("give <player|me> <item_id> [amount]",
                          "Gives the target player or self the item.")
                          .OnInvoke += CGive;

            CreateCommand("god",
                          "Allows creative freedom.",
                          isDebugCommand: true)
                          .OnInvoke += CGod;
            
            CreateCommand("help",
                          "Prints help message.")
                          .OnInvoke += CHelp;

            CreateCommand("host",
                          "Host a lobby.",
                          isDebugCommand: true)
                          .OnInvoke += CHost;

            CreateCommand("party <create|invite|leave>",
                          "Party commands.")
                          .OnInvoke += CParty;

            CreateCommand("place <object>",
                          "Places an object where the player is looking at.")
                          .OnInvoke += CPlace;

            // CreateCommand("msg <username> <msg>",
            //               "Message a player (in the current lobby). Will only work when in a lobby.",
            //               isDebugCommand: true)
            //               .OnInvoke += CMessage;

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
                          "Set the in-game time.")
                          .OnInvoke += CTime;
            
            CreateCommand("wbcraft <item_id>",
                          "Crafts item if player is interacting with workbench",
                          isDebugCommand: true)
                          .OnInvoke += CWbcraft;

            CreateCommand("weather <id> <durationSeconds>",
                          "Set the current weather.")
                          .OnInvoke += CWeather;

            CreateCommand("world <create|load|save> <world_name>",
                          "World commmands.")
                          .OnInvoke += CWorld;
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
        
        const string DEFAULT_DEV_AUTH_HOST = "localhost:7777";
        /// <summary>
        /// Auth commands.
        /// </summary>
        void CAuth(object sender, string[] args)
        {
            if (args[0] == "login" || args[0] == "li")
            {
                EOSSubManagers.Auth.StartLogin();
            }
            else if (args[0] == "logout" || args[0] == "lo")
            {
                EOSSubManagers.Auth.StartLogOut();
            }
            else if (args[0] == "removetoken" || args[0] == "rt")
            {
                EOSSubManagers.Auth.RemovePersistentToken();
            }
            else if (args[0] == "dev" || args[0] == "d")
            {
                if (args.Length == 2)
                {
                    EOSSubManagers.Auth.StartDevAuthLogin(DEFAULT_DEV_AUTH_HOST, args[1]);
                }
                else if (args.Length == 3)
                {
                    EOSSubManagers.Auth.StartDevAuthLogin(args[1], args[2]);
                }

                var authUi = GameObject.FindFirstObjectByType<AuthUserDisplayUI>();
                authUi.SetUIFakeLogin();
            }
        }        
        
        /// <summary>
        /// Clears the console messages.
        /// </summary>
        void CClear(object sender, string[] args)
        {
            _messages.Clear();
        }        
        
        bool _creativeIsOn = false;
        CreativeWindow creativeWindow = null;
        /// <summary>
        /// Toggles the ability to open creative window.
        /// </summary>
        void CCreative(object sender, string[] args)
        {
            // if (!Game.Main.IsOnline)
            // {
            if (!Game.World.IsInWorld) return;
            
            if (_creativeIsOn)
            {
                _creativeIsOn = false;
                localPlayer.InventoryWindow.RemoveAppendedFrame(creativeWindow);
                creativeWindow.Destruct();
                LogInfo($"Disabled creative menu.");
            }
            else
            {
                _creativeIsOn = true;
                /// TODO: Disable achievements for session
                creativeWindow = Game.UI.Create<CreativeWindow>("Creative Window");
                creativeWindow.Initialize(localPlayer);
                localPlayer.InventoryWindow.Append(creativeWindow);
                LogInfo($"Enabled creative menu.");
            }
        }

        /// <summary>
        /// Spawns an entity.
        /// </summary>
        void CCraft(object sender, string[] args)
        {
            // var newItem = new Item(Game.Items.GetData(args[0]));
            // var playerInventoryCrafter = (PlayerCrafting)_player.Crafter;
            // playerInventoryCrafter.PlayerCraftItem(newItem.Data.Recipes[0]);
        }

        void CDamage(object sender, string[] args)
        {
            
        }

        void CEntity(object sender, string[] args)
        {
            if (args[0] == "query" || args[0] == "q")
            {
                var id = args[1];
                if (Game.Entity.IsValidId(id))
                {
                    var ettyList = Game.World.CurrentWorld.GetEntitiesById(id);
                    var msg = $"Entity Id: '{id}' | Total Count: {ettyList.Count}";
                    
                    foreach (var etty in ettyList)
                    {
                        msg += $"'{etty.Id}':[{etty.GetInstanceID()}], ";
                    }

                    Game.Console.LogInfo(msg);
                }
                else
                {
                    Game.Console.LogInfo($"'{id}' is not a valid entity Id");
                }
            }
            else if (args[0] == "select" || args[0] == "s")
            {
                var id = args[1];
                if (Game.Entity.IsValidId(id))
                {
                    
                }
            }
            else
            {
                throw new KeyNotFoundException();
            }
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
                    localPlayer.Inventory.Bag.TryPutNearest(newItem);
                    Game.Console.LogInfo($"Given {localPlayer.name} {count} of '{id}'");
                }
                else /// target player Id
                {
                    throw new InvalidCommandUsageException();
                }
            }
        }

        bool _godIsOn = false;
        /// <summary>
        /// Allows creative freedom.
        /// </summary>
        void CGod(object sender, string[] args)
        {
            if (!Game.World.IsInWorld)
            {
                LogInfo($"/teleport can only be used within a world.");
                return;
            }

            var player = Game.World.CurrentWorld.GetLocalPlayer();
            if (player == null) return;

            if (_godIsOn)
            {
                player.SetGodMode(false);
                LogInfo($"Disabled god mode.");
            }
            else
            {
                player.SetGodMode(true);
                LogInfo($"Enabled god mode.");
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
                    LogInfo($"Showing page {page}");
                }
                else
                {
                    LogInfo($"Usage: " + _commandsDict[args[1]].Syntax);
                    LogInfo(_commandsDict[args[1]].Description);
                }
            }
        }
        
        /// <summary>
        /// Prints a message to the console.
        /// </summary>
        void CHost(object sender, string[] args)
        {
            if (Game.Main.CurrentScene.name != "TitleScreen") return;

            var handler = FindAnyObjectByType<HostWorldHandler>();
            handler.CreateLobby();
        }
        
        /// <summary>
        /// Create/Invite/Leave party. Only within worlds.
        /// </summary>
        void CParty(object sender, string[] args)
        {
            if (!Game.World.IsInWorld) return;

            var player = Game.World.GetWorld().GetLocalPlayer();
            if (player == null) return;

            switch (args[0])
            {
                case "create":
                {
                    player.NetworkEntity.CreateParty();
                    break;
                }
                case "invite":
                {
                    if (args.Length < 2)
                    {
                        Game.Console.LogInfo($"Usage: /party invite <username>");
                        return;
                    }

                    player.NetworkEntity.InviteToParty(args[1]);
                    break;
                }
                case "accept":
                {
                    if (args.Length < 2)
                    {
                        Game.Console.LogInfo($"Usage: /party accept <username>");
                        return;
                    }

                    if (EOSSubManagers.Lobbies.FindMemberByDisplayName(args[1], out var member))
                    {
                        if (EOSSubManagers.Transport.GetEOSTransport().TryGetClientIdMapping(member.ProductUserId, out var clientId))
                        {
                            var parties = FindObjectsByType<Party>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                            var party = Array.Find(parties, (p) => p.HostIs(clientId));
                            if (party != null)
                            {
                                party.AcceptPartyInvite();
                            }
                            else
                            {
                                Game.Console.LogInfo($"Unable to join {args[1]}'s party.");
                            }
                        }
                    }
                    break;
                }
                case "leave":
                {
                    if (player.NetworkEntity.CurrentParty == null)
                    {
                        Game.Console.LogInfo($"You're not in a party to leave one!");
                        return;
                    }

                    player.NetworkEntity.LeaveCurrentParty();
                    break;
                }
                case "kick":
                {
                    if (args.Length < 2)
                    {
                        Game.Console.LogInfo($"Usage: /party accept <username>");
                        return;
                    }

                    // player.NetworkEntity.KickFromParty(args[1]);
                    break;
                }
            }
        }  
        
        const float PLACE_RANGE = 50f;
        /// <summary>
        /// Places an object in the world, positioned on where the player is looking at.
        /// </summary>
        void CPlace(object sender, string[] args)
        {
            if (!Game.World.IsInWorld) return;

            var player = Game.World.GetWorld().GetLocalPlayer();
            if (player == null) return;

            if (Physics.Raycast(player.EyeLevel, player.Forward, out RaycastHit hit, PLACE_RANGE, groundLayerMask))
            {
                // if (groundLayerMask.Includes(hit.collider.gameObject.layer))
                // {
                    Game.Objects.PlaceNew(args[0], position: hit.point, (info) =>
                    {
                        info.Object.Rotation = Quaternion.identity; /// some objects should placed facing the player :P
                    });
                // }
            }
        }  

        /// <summary>
        /// Prints a message to the console.
        /// </summary>
        void CMessage(object sender, string targetUserId, string message)
        {
            if (!EOSSubManagers.Lobbies.IsInLobby)
            {
                Game.Console.LogInfo($"Must be in a lobby to send messages to other players.");
                return;
            }

            EOSSubManagers.P2P.SendChatMessage(targetUserId, message);
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

        const char OFFSET = '~';
        /// <summary>
        /// Spawns an entity.
        /// </summary>
        void CTeleport(object sender, string[] args)
        {
            if (!Game.World.IsInWorld)
            {
                LogInfo($"/teleport can only be used within a world.");
                return;
            }

            if (float.TryParse(args[0], out float x) || args[0].StartsWith(OFFSET))
            if (float.TryParse(args[1], out float y) || args[1].StartsWith(OFFSET))
            if (float.TryParse(args[2], out float z) || args[2].StartsWith(OFFSET))
            {
                var player = Game.World.CurrentWorld.GetLocalPlayer();
                if (player != null)
                {
                    float dx = x;
                    float dy = y;
                    float dz = z;

                    if (args[1].StartsWith(OFFSET))
                    {
                        if (float.TryParse(args[0][0..], out var fx))
                        {
                            dy = player.Position.y + fx;
                        }
                        else throw new InvalidCommandUsageException();
                    }
                    if (args[1].StartsWith(OFFSET))
                    {
                        if (float.TryParse(args[1][1..], out var fy))
                        {
                            dy = player.Position.y + fy;
                        }
                        else throw new InvalidCommandUsageException();
                    }
                    if (args[1].StartsWith(OFFSET))
                    {
                        if (float.TryParse(args[2][2..], out var fz))
                        {
                            dy = player.Position.y + fz;
                        }
                        else throw new InvalidCommandUsageException();
                    }
                    
                    player.Position = new(dx, dy, dz);
                }
            }

            throw new InvalidCommandUsageException();
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
                    Game.Console.LogInfo("TPS must be a positive integer value.");
                }
            }
        }

        void CTime(object sender, string[] args)
        {
            if (!Game.World.IsInWorld)
            {
                Game.Console.LogInfo("/time command can only be performed within worlds.");
                return;
            }
            
            if (args.Length > 1 && args[0] == "set")
            {
                int hour, minute = 0;

                if (int.TryParse(args[1], out hour))
                {
                    if (args.Length > 2 && int.TryParse(args[2], out int parsedMinute))
                    {
                        minute = parsedMinute;
                    }
                    
                    Game.World.CurrentWorld.Time.SetTime(hour, minute);
                    Game.Console.LogInfo($"Time set to {hour}:{minute:D2}");
                }
                else
                {
                    Game.Console.LogInfo("/time set arguments must be valid positive integers.");
                }
            }
            else
            {
                Game.Console.LogInfo("Usage: /time set <hour> [minute]");
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
        /// Set the current weather.
        /// </summary>
        void CWeather(object sender, string[] args)
        {
            /// TODO: add handling
            Game.World.CurrentWorld.Weather.SetWeather(args[0], int.Parse(args[1]));
            LogInfo($"Weather set to {args[0]}");
        }

        /// <summary>
        /// World commands.
        /// </summary>
        void CWorld(object sender, string[] args)
        {
            if (args[0] == "create")
            {
                // Game.World.CreateWorld(args[1]);

            }
            else if (args[0] == "load")
            {
                string worldName = args[1];

            }
            else if (args[0] == "save")
            {
                Game.World.CurrentWorld.SaveWorld();
            }
            else
            {
                PromptInvalid("world");
            }
        }
        
        #endregion
    }
}
