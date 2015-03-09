using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using ArcheBuddy.Bot.Classes;

//
// Special Thanks for a really 
//
//
//
//

namespace FarmMonkeyBeta
{
    public class FarmMonkey : Core
    {
        public static string GetPluginAuthor()
        { return "Defectuous"; }
        public static string GetPluginVersion()
        { return "1.0.0.0"; }
        public static string GetPluginDescription()
        { return "FarmMonkey: Continuous Multi Farm Harvest & Planting Plugin"; }

        // Start Universal Config
        
        uint[] _farms = { 12345, 54321 };
        int _minlabor   = 200;
        string _seed    = "Rice Seed";
        string _plant   = "Rice"; // Make sure plant is Mature type or not. 
        string _gather  = "Gathering: Spend 1 Labor to gather materials.";
        string _harvest = "Farming: Spend 1 Labor to harvest crops.";
        //string _gpsfile = "\\Plugins\\FarmMonkey\\file.db3"; // Location of GPS File

        
        // End Universal Config ( Do Not Edit anything past this line )
        
        // Universal Application Information
        //int _labor = me.laborPoints;     // Not necessary I think.
        //string _myname = me.name;   // Not necessary I think.

        //Call on plugin start
        public void PluginRun()
        {
            ClearLogs();
            Log("FarmMonkey: Plugin Started");
            while (true) {    
                if (gameState == GameState.Ingame){
                    Log("We are in game and ready to Farm");
                    //MoveFromSafe();
                    // Time to Harvest plants
                    Harvesting();
                    // Lets fill that field
                    PLanting();
                    //MovetoSafe();
                }
            }
        }


        //public void MoveFromSafe()
        //{

        //}

        //public void MoveToSafe()
        //{

        //}
        
        public void Harvesting()
        {
            foreach (uint farm in _farms){
                Log("Harvesting: "+farm);
                if (me.laborPoints >= _minlabor)
                CollectItemsAtFarm(_plant, _gather, farm);
                CollectItemsAtFarm(_plant, _harvest, farm);
                } 
        }

        public void PLanting()
        {
            foreach (uint farm in _farms)
            {
                Log("Planting: " + farm);
                PlantItemsAtFarm(_seed, farm);
            }
        }
        //Call on plugin stop
        public void PluginStop()
        {

        }
    }
}
