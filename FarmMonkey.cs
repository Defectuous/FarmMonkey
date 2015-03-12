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

namespace ArcheAgeFarmMonkey
{
    public class FarmMonkey : Core
    {
        public static string GetPluginAuthor()
        { return "Defectuous"; }
        public static string GetPluginVersion()
        { return "1.1.3.0"; }
        public static string GetPluginDescription()
        { return "FarmMonkey: Continuous Multi Farm Harvest & Planting Plugin"; }
        
        // START Universal Config
        
        uint[] _farms = { 12345, 54321 }; // Gather Farm ID's wtih scarecrow { 12345, 54321 }
        int _minlabor = 200;  // Minimum Labor for harvesting.
        string _seed  = "Barley Seed";
        string _plant = "Barley"; // Make sure plant ends up Mature or not. 
        
        // Note: You may need to update the Amount of labor needed for Gathering & Harvesting.
        string _gather  = "Gathering: Spend 1 Labor to gather materials.";
        string _harvest = "Farming: Spend 1 Labor to harvest crops.";
        
        // This gps file needs 2 points " Safe " & " Farm "
        string _gpsfile = "\\plugins\\FarmMonkey\\Path\\file.db3";
        
        // Set to true if you have a gps file for moveing to and from the safe.
        private bool _enablegps = false; 
        
        // END Universal Config 
        // ( Do Not Edit anything past this line unless you are confident you know what your doing )
        
        // Universal Application Information
        private Gps gps;
        
        //Call on plugin start
        public void PluginRun()
        {
            ClearLogs();
            Log(Time() + "FarmMonkey: Plugin Started");
            
            while (true) {
                if (gameState == GameState.Ingame){
                    Log(Time() + "We are in game and ready to Farm");
                    // Lets get back to the Farms
                    if ( _enablegps == true){ MoveToFarm(); }
                    
                    // Time to Harvest plants
                    Harvesting();
                    // Lets fill that field with seeds
                    Planting();
                    
                    // Time to head back to the safe spot
                    if ( _enablegps == true){ MoveToSafe(); }
                    
                    
                    //  Temporary Sleep to prevent to many checks
                    Random random = new Random();
                    var mseconds  = random.Next(240, 300) * 1000;
                    var seconds   = mseconds / 1000;
                    Log(Time() +  "Waiting " + seconds.ToString() + " seconds to check seeds");
                    Thread.Sleep(mseconds);

                }
            }
        }

        public void MoveToFarm()        
        {         
           gps = new Gps(this); 
           gps.LoadDataBase(Application.StartupPath + _gpsfile); 
           gps.GpsMove("Farm");                
         }
        
       public void MoveToSafe()        
        {         
           gps = new Gps(this); 
           gps.LoadDataBase(Application.StartupPath + _gpsfile); 
           gps.GpsMove("Safe");                
         }
        
        public void Harvesting()
        {
            var _labor = me.laborPoints;
            if (_labor > _minlabor){
                Log("Current Labor:" + _labor);
                foreach (uint farm in _farms){
                    Log(Time() + "Harvesting " + _plant + "(s) on FarmID: " + farm);
                    CollectItemsAtFarm(_plant, _gather, farm);
                    CollectItemsAtFarm(_plant, _harvest, farm);
                    } 
            }
        }

        public void Planting()
        {
            var seedcount = itemCount(_seed);
            if ( seedcount == 0){
                Log(Time() + "Seed Count:" + seedcount + _seed);
                Log(Time() + "You have no seeds!");
                } else{
                    foreach (uint farm in _farms)
                    {
                        Log(Time() + "Seed Count: " + seedcount + _seed);
                        Log(Time() +  "Planting" + _seed + "(s) on FarmID: " + farm);
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
