using UnityEngine;
using UZSG.Items;

namespace UZSG.Systems
{
    public sealed partial class Console : MonoBehaviour, IInitializable
    {
        #region Command implementations
        /// <summary>
        /// Clears the console messages.
        /// </summary>
        void CClear(object sender, string[] args)
        {
            Messages.Clear();
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
            if (_commands.ContainsKey(args[0]))
            {
                Log(_commands[args[0]].Description);
            } else
            {
                WriteLine("List of available commands");
                foreach (var c in _commands)
                {

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
            Game.Entity.Spawn(args[0]);
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
