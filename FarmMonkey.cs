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
        { return "1.1.10.0"; }
        public static string GetPluginDescription()
        { return "FarmMonkey: Continuous Multi Farm Harvest & Planting Plugin"; }
        
        // START Universal Config
        
        string _seed  = "Sunflower Seed"; // Seeds to plant
        string _plant = "Sunflower"; // Make sure plant ends up Mature or just the plant name
        uint[] _farms = { 12127, 12066 }; // Gather Farm ID's wtih scarecrow { 12345, 54321 }
        
        // Tweak as necessary 
        int _minlabor = 200;  // Minimum Labor for harvesting.

        // Note: You may need to update the Amount of labor needed for Gathering & Harvesting.
        string _gather  = "Gathering: Spend 1 Labor to gather materials.";
        string _harvest = "Farming: Spend 1 Labor to harvest crops.";
        
        // to enable only Planting or Harvesting. Both enabled by default.
        private bool _enableharvest = true;
        private bool _enableplant = true;
        
        // Buy Section
        private bool _enablebuyseed = false; // To buy Seeds you need a gps point called Seed
        int _mingold  = 50000;   // 50000 = 5Gold 00Silver 00Copper
        int _minseed  = 50; // Minimum Seed Count before purchasing more
        int _maxseed  = 1000; // Maximum number of seeds to have at a time        
        
        // Rest Section
        private bool _enablerest = false; // Enable to sit in chair or lay in bed
        string _restitem = "Chair"; // Name of Chair or bed
        
        // This gps file needs the following points Safe, Farm, Seed, Mail
        string _gpsfile = "\\plugins\\FarmMonkey\\Path\\file.db3";
        
        // Set to true if you have a gps file for moveing to and from the safe.
        private bool _enablegps = false;
        private bool _deathcheck = false;
        
        // do not change this unless you plan to lower it
        int _buyseedamt = 100; // Do not raise this above 100
        // END Universal Config 
        // (Do Not Edit anything past this line unless you are confident you know what your doing)
        
        // Universal Application Information
        private Gps gps;
        Random random = new Random();
        
        //Call on plugin start
        public void PluginRun()
        {
            ClearLogs();
            Log(Time() + "FarmMonkey: Plugin Started");
            
            while (true) {
                if (gameState == GameState.Ingame){
                    // Start Threads here ( farming & movement )
                    Log(Time() + "Time to Farm");

                    // Death Check ( Am i really dead ? )
                    if ( _enablegps == true && _deathcheck == true && !me.isAlive()){ 
                        Log(Time() + "We have died, there must be a reason for this check into that would you");
                        
                        // Res timer is Buggy due to continued deaths raises the time
                        Log(Time() + "Waiting 18 Seconds to resurection");
                        Thread.Sleep(18000);
                        
                        ResToRespoint();
                        Log(Time() +  "Time to Ressurect");
                        while (!me.isAlive()){
                            Log(Time() +  "Waiting 8 seconds to try again");
                            Thread.Sleep(8000);
                            ResToRespoint();
                         }
                        MoveToSafe();
                    }  
                    // Lets get back to the Farms
                    if (_enablegps == true && me.isAlive()){ MoveToFarm();}
                    
                    // Time to Harvest plants
                    if (_enableharvest == true){ Harvesting(); }
                    
                    // Mail Function
                    
                    // Buy Seeds before planting
                    if (_enablebuyseed == true && _enablegps == true){ BuySeeds(); }
                    
                    // Lets fill that field with seeds
                    if (_enableplant == true){ Planting(); }
                    
                    // Time to head back to the safe spot
                    if ( _enablegps == true){ MoveToSafe(); }
                    
                    //  Temporary Sleep to prevent to many checks
                    var mseconds = random.Next(240, 300) * 1000;
                    var seconds  = mseconds / 1000;
                    Log(Time() +  "Waiting " + seconds.ToString() + " seconds to check seeds");
                    Stock();
                    Thread.Sleep(mseconds);

                }
            }
        }

        // Moving Routines
        public void MoveToFarm()        
        {         
           gps = new Gps(this);
           Log(Time() + "Loading GPS File"); 
           gps.LoadDataBase(Application.StartupPath + _gpsfile); 
           Log(Time() +  "Moving to Farm");
           gps.GpsMove("Farm");                
         }
        
       public void MoveToSafe()        
        {         
           gps = new Gps(this); 
           Log(Time() + "Loading GPS File"); 
           gps.LoadDataBase(Application.StartupPath + _gpsfile); 
           Log(Time() + "Moving to Safe");
           gps.GpsMove("Safe");
           Log(Time() +  "Safe Spot has been Reached");           
           if (_enablerest == true){ 
                Relax();
                Log(Time() +  "Rest Time");
                }
        }
       
       // Farming Routines
        public void Harvesting()
        {
            var _labor = me.laborPoints;
            if (_labor > _minlabor){
                Log(Time() + "Current Labor:" + _labor);
                foreach (uint farm in _farms){
                    Log(Time() + "Harvesting " + _plant + "(s) on FarmID: " + farm);
                    CollectItemsAtFarm(_plant, _gather, farm);
                    CollectItemsAtFarm(_plant, _harvest, farm);
                    } 
                }else{ Log(Time() +"Your Labor is under " + _minlabor); }
        }

        public void Planting()
        {
            var seedcount = itemCount(_seed);
            if ( seedcount == 0){
                Log(Time() + "You have no seeds!");
                } else{
                    foreach (uint farm in _farms)
                    {
                        var seedcount2 = itemCount(_seed);
                        Log(Time() + _seed + " Count: " + seedcount2);
                        Log(Time() +  "Planting " + _seed + "(s) on FarmID: " + farm);
                        PlantItemsAtFarm(_seed, farm);
                        MoveToFarm();
                    }
                }
        }
        
        // Utility Stuff
        public void BuySeeds()
        {
            while (true){
                
                var _mygold   = me.goldCount;
                var seedcount = itemCount(_seed);
                if (_mygold <= _mingold){ 
                    Log(Time() + "Unable to Purchase due to lack of funds"); 
                    Log(Time() + "######################################");
                    break;
                    } else {
                        Log(Time() + "######################################");
                        Log(Time() + "Lets go buy seeds");
                        gps = new Gps(this); 
                        gps.LoadDataBase(Application.StartupPath + _gpsfile); 
                        gps.GpsMove("Seed");
                        Log(Time() + "######################################");
                        Log(Time() + "Seed Stock: " + seedcount);
                        if ( seedcount <= _minseed && seedcount <= _maxseed){
                            BuyItems(_seed, _buyseedamt);
                            var seedcount2 = itemCount(_seed);
                            Log(Time() + "Updated Seed Cound: " + seedcount2);
                            
                            var mseconds = random.Next(50, 300);
                            var seconds  = mseconds / 1000;
                            Log(Time() +  "Checking seed stock");
                            Thread.Sleep(mseconds);
                        
                        } else {
                            Log(Time() + "Seed Purchase Completed");
                            Log(Time() + "######################################");
                            break;
                    }
                }
            }
        }

        public void Relax()
        {
            DoodadObject _restitem;
            _restitem = getNearestDoodad(_RestingSpot);
            UseDoodadSkill(_RestText, _restitem, true, 0);
        }
            
        public string Time()
        {
            string A = DateTime.Now.ToString("[hh:mm:ss] ");
            return A;
        }
    
        public void Stock()
            {
            var seedcount  = itemCount(_seed);
            var plantcount = itemCount(_plant);
        
            Log(Time() + "################################");
            Log(Time() + "# Farm Markey Inventory Update #");
            Log(Time() + "################################");
            Log(Time() + _seed + " Count: " + seedcount);
            Log(Time() + _plant + " Count: " + plantcount);
            Log(Time() + "################################");
        }    
            
        //Call on plugin stop
        public void PluginStop()
        {

        }
    }
}
