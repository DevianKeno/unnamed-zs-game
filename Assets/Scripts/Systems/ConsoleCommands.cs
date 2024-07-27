using UnityEngine;
using UZSG.Items;

namespace UZSG.Systems
{
    /// <summary>
    /// Developer console for invoking system commands.
    /// </summary>
    public sealed partial class Console : MonoBehaviour, IInitializable
    {
        /// The args parameters represent the arguments WITHOUT the actual command.
        /// ex. "/spawn item bandage"
        /// The args would consist of [item, bandage]
        
        #region Command implementations
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
            var newItem = new Item(Game.Items.GetItemData("m4ar"));
            _player.CraftingAgent.CraftItem(newItem);
        }

        void CDamage(object sender, string[] args)
        {
            
        }

        /// <summary>
        /// Clears the console messages.
        /// </summary>
        void CFreecam(object sender, string[] args)
        {
            
        }

        /// <summary>
        /// Prints the help message to the console.
        /// </summary>
        void CHelp(object sender, string[] args)
        {
            if (commandsDict.ContainsKey(args[0]))
            {
                Log(commandsDict[args[0]].Description);
            } else
            {
                WriteLine("List of available commands");
                foreach (var c in commandsDict)
                {
                    ///TECHNICAL DEBT
                }
            }
        }
        
        /// <summary>
        /// Gives the player the item.
        /// </summary>
        void CItem(object sender, string[] args)
        {
            // Item item = new(
            // player.Inventory.TryPutNearest(item);
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
            } else
            {
                if (args[0] == "item")
                {
                    Game.Entity.SpawnItem(args[1]);
                }
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
                    Game.World.Time.SetTime(value);
                }
                else
                {
                    Game.Console.Log("Time must be a positive integer value.");
                }
            }
        }

        /// <summary>
        /// World commands.
        /// </summary>
        void CWorld(object sender, string[] args)
        {
            if (args[0] == "create")
            {
                Game.World.CreateWorld(args[1]);

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
