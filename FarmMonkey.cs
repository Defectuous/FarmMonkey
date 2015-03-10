using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using ArcheBuddy.Bot.Classes;

//
// Special Thanks to Voyager92 for a really Epic Non-Stop Farm/Gathering Base from which I am building this on. 
// Thread:  [Plugin] Non-Stop Farm/Gathering for multiple toons/farms and with restart support
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
        
        // START Universal Config
        
        uint[] _farms = { 16790 }; // Gather Farm ID's wtih scarecrow { 12345, 54321 }
        int _minlabor = 200;
        string _seed  = "Potato Eyes";
        string _plant = "Potato"; // Make sure plant is Mature type or not. 

        // Note: You may need to update the Amount of labor needed for Gathering & Harvesting.
        string _gather  = "Gathering: Spend 1 Labor to gather materials.";
        string _harvest = "Farming: Spend 1 Labor to harvest crops.";

        // Note: You may need to change the file.db3 name here
        // GPS Info, you need to make two points " Farm " & " Safe "
        //string _gpsfile = "\\Plugins\\FarmMonkey\\Path\\file.db3"; // Location of GPS File

        
        // END Universal Config 
        // ( Do Not Edit anything past this line unless you are confident you know what your doing )
        
        // Universal Application Information
        //private Gps gps;

        //Call on plugin start
        public void PluginRun()
        {
            ClearLogs();
            Log(Time() + "FarmMonkey: Plugin Started");
            while (true) {
                if (gameState == GameState.Ingame){
                    Log(Time() + "We are in game and ready to Farm");
                    //MoveFromSafe();
                    // Time to Harvest plants
                    Harvesting();
                    // Lets fill that field
                    Planting();
                    //FarmCheckTime(); // Check time so we can come back and harvest when it's ready
                    //MoveToSafe();
                    
                    //  Temporary Sleep to prevent to many checks
                    Random random = new Random();
                    var mseconds  = random.Next(240, 300) * 1000;
                    var seconds   = mseconds / 1000;
                    Log(Time() +  "Waiting for " + seconds.ToString() + " seconds");
                    Thread.Sleep(mseconds);

                }
            }
        }

        // Move From Safe Location
        //public void MoveFromSafe()
        //{
        //    gps.LoadDataBase(Application.StartupPath + _gpsfile);
        //    gps.GpsMove("Farm");
        //}

        // Move Back to Safe Location ( Possibley sit on nearest chair or bed )
        //public void MoveToSafe()
        //{
        //    gps.LoadDataBase(Application.StartupPath + _gpsfile);
        //    gps.GpsMove("Safe");
        //}
        
        //public void FarmCheckTime()
        //{
            // ( check latest dodad by me.name & last time )
        //}
        
        public void Harvesting()
        {
            foreach (uint farm in _farms){
                Log(Time() + "Harvesting on FarmID: "+farm);
                if (me.laborPoints >= _minlabor)
                CollectItemsAtFarm(_plant, _gather, farm);
                CollectItemsAtFarm(_plant, _harvest, farm);
                } 
        }

        public void Planting()
        {
            var seedcount = itemCount(_seed);
            if ( seedcount == 0){
            Log(Time() + "You have no seeds Stopping Plugin!");
            PluginStop();
            } else{
                foreach (uint farm in _farms)
                {
                    Log(Time() +  "Planting" + _seed + "on FarmID: " + farm);
                    PlantItemsAtFarm(_seed, farm);
                }
            }
        }
        
        public string Time() //- Get Time
        {
            string A = DateTime.Now.ToString("[hh:mm:ss] ");
            return A;
        }
        //Call on plugin stop
        public void PluginStop()
        {

        }
    }
}
