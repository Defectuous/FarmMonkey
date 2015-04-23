using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using ArcheBuddy.Bot.Classes;

///
//
// Special Thanks to Voyager92 for a really Epic Non-Stop Farm/Gathering Base from which I am building this on. 
// Thread:  [Plugin] Non-Stop Farm/Gathering for multiple toons/farms and with restart support
//
///

namespace ArcheAgeFarmMonkey
{
    public class FarmMonkey : Core
    {
        public static string GetPluginAuthor()
        { return "Defectuous"; }
        public static string GetPluginVersion()
        { return "1.1.10.13"; }
        public static string GetPluginDescription()
        { return "FarmMonkey: Continuous Multi Farm Harvest & Planting Plugin"; }
        
        // START Universal Config
        
        string _seed     = "Aloe Seed"; // Seeds to plant
        string _plant    = "Aloe"; // Make sure plant ends up Mature or just the plant name
        string _seedling = "Aloe Seedling";
        uint[] _farms = { 12345, 54321 }; // Gather Farm ID's wtih scarecrow { 12345, 54321 }
        private bool _oldtimer = false; // To enable the older timer rather than plant timers
        
        // Tweak as necessary 
        int _minlabor = 200;  // Minimum Labor for harvesting.

        // Note: You may need to update the Amount of labor needed for Gathering & Harvesting.
        string _gather  = "Gathering: Spend up to 3 Labor to gather materials.";
        string _harvest = "Farming: Spend up to 3 Labor to harvest crops.";
        
        // to enable only Planting or Harvesting. Both enabled by default.
        private bool _enableharvest = false;
        private bool _enableplant = true;
        
        // Buy Section
        private bool _enablebuyseed = false; // To buy Seeds you need a gps point called Seed
        string _buyvendor = "Vendor Name"; //Buy Vendor Name
        int _mingold      = 50000;   // 50000 = 5Gold 00Silver 00Copper
        int _minseed      = 50; // Minimum Seed Count before purchasing more
        int _maxseed      = 1000; // Maximum number of seeds to have at a time        
        
        
        // Enable Doors
        private bool _enabledoor = false; // Set to true if youe safe Zone requires you to go through a door & you have door0 & door1 set in the gps file
        
        // Rest Section
        private bool _enablerest = false; // Enable to sit in chair or lay in bed
        string _restitem = "Chair"; // Name of Chair or bed 
        string _resttext = "Sit in a chair or lay on a bed.";
        
        // Mail Functions ( Will not send money )
        private bool _enablemail = false; // Enable to send mail
        public bool _fastmail = true; // To send with fast mail rather then slow
        string _mailto   = "Null"; // Name to who your mail will go to
        string _mailsub  = "For Processing"; // Subject of that mail
        string _mailtext = "From Farming"; // Mail Body Required
        int _amtmail     = 1000; // When your inventory reaches this amount it will mail at the end of the harvest.
        
        // This gps file needs the following points Safe, Farm, Seed, Mail
        string _gpsfile = "\\plugins\\FarmMonkey\\Path\\file.db3";
        
        // Set to true if you have a gps file for moveing to and from the safe, mail, buy, farm.
        private bool _enablegps = false;
        private bool _deathcheck = false;
        
        // do not change this unless you plan to lower it
        int _buyseedamt = 100; // Do not raise this above 100
        // END Universal Config 
        // ( Do Not Edit anything past this line unless you are confident you know what your doing !!! )
        
        // Universal Application Information

        private Gps gps;
        Random random = new Random();
        
        //Call on plugin start
        public void PluginRun()
        {
            ClearLogs();
            Log(Time() + "[ FarmMonkey: Plugin Started ]");
            Log(Time() + "How to Read Farm Monkey logging.");
            Log(Time() + "[ INFO: equals relivant information ]");
            Log(Time() + "[ WARNING: Needs to be Looked into, not bot threatening. ]");
            Log(Time() + "[ ERROR: Bada Bad, post error message forums tob e resolved.  ]");
            
            while (true) {
                if (gameState == GameState.Ingame){
                    // Start Threads here ( farming & movement )
                    Log(Time() + "Lets Start Farming");

                    // Death Check ( Am i really dead ? )
                    if ( _enablegps == true && _deathcheck == true && !me.isAlive()){ 
                        Log(Time() + "[ WARNING: We have died, there must be a reason for this check into that would you ]");
                        
                        // Res timer is Buggy due to continued deaths raises the time
                        Log(Time() + "INFO: Waiting 18 Seconds to resurection");
                        
                        Thread.Sleep(18000);
                        
                        ResToRespoint();
                        Log(Time() +  "INFO: Time to Ressurect");
                        while (!me.isAlive()){
                            Log(Time() + "INFO: Waiting 8 seconds to try again");
                            Thread.Sleep(8000);
                            ResToRespoint();
                         }
                        MoveToSafe();
                    }  
                    // Lets get back to the Farms
                    if (_enablegps == true && me.isAlive()){ MoveToFarm(); }else{Log(Time() + "[ INFO: GPS DISABLED MoveToFarm ]"); }
                    
                    // Time to Harvest plants
                    if (_enableharvest == true){ Harvesting(); }else{Log(Time() + "[ INFO: _enableharvest Set to False ]"); }
                    
                    // Mail Function
                    if (_enablemail == true && _enablegps == true){ Mailer(); }else{Log(Time() + "[ INFO: _enablemail Set to False ]"); }
                    
                    // Buy Seeds before planting
                    if (_enablebuyseed == true && _enablegps == true){ BuySeeds(); }else{Log(Time() + "[ INFO: _enablebuyseed Set to False ]"); }
                    
                    // Lets fill that field with seeds
                    if (_enableplant == true){ Planting(); }else{Log(Time() + "[ INFO: _enableplant Set to False ]"); }
                    
                    // Time to head back to the safe spot
                    if (_enablegps == true){ MoveToSafe(); }else{Log(Time() + "[ INFO: GPS DISABLED MoveToSafe ]"); }
                    
                    Stock();
                    Timer();
                }
            }
        }

        // Moving Routines
        public void MoveToFarm()        
        {         
           gps = new Gps(this);
           Log(Time() + "Loading GPS File"); 
           gps.LoadDataBase(Application.StartupPath + _gpsfile); 
           //
           if (_enabledoor == true){
            Log(Time() + "We need to Open / Close the Door");
            gps.GpsMove("Door1");
            // Time to Open the Door
            DoodadObject doors = getNearestDoodad("Door");
            Log(Time() + "[ INFO: " + string.Format("Name: {0} - ID: {1}", doors.name, doors.phaseId) + " ]");
            if (doors != null && ComeTo(doors, 2))
                {
                    if (doors.phaseId == 11163)
                        {
                            UseDoodadSkill(16828, doors);
                            Thread.Sleep(1000);
                            gps.GpsMove("Door0");
                        } else { gps.GpsMove("Door0"); }
                        // Time to Close the Door
                        if (doors.phaseId == 11165)
                    {
                        UseDoodadSkill(16828, doors);
                        Thread.Sleep(1000);
                    }
                }
        }
           //
           Log(Time() +  "Moving to Farm");
           gps.GpsMove("Farm");                
        }
        
        public void MoveToSafe()        
        {         
            gps = new Gps(this); 
            Log(Time() + "Loading GPS File"); 
            gps.LoadDataBase(Application.StartupPath + _gpsfile);
            if (_enabledoor == true){
            Log(Time() + "We need to Open / Close the Door");
            gps.GpsMove("Door0");
            // Time to Open the Door
            DoodadObject doors = getNearestDoodad("Door");
            Log(Time() + "INFO: " +  string.Format("Name: {0} - ID: {1}", doors.name, doors.phaseId));
            if (doors != null && ComeTo(doors, 2))
                {
                    if (doors.phaseId == 11163)
                        {
                            UseDoodadSkill(16828, doors);
                            Thread.Sleep(1000);
                            gps.GpsMove("Door1");
                        } else { gps.GpsMove("Door1"); }
                    // Time to Close the Door
                    if (doors.phaseId == 11165)
                    {
                        UseDoodadSkill(16828, doors);
                        Thread.Sleep(1000);
                    }
            }
            //
            Log(Time() + "Moving to Safe");
            gps.GpsMove("Safe");
            Log(Time() +  "Safe Spot has been Reached");           
            if (_enablerest == true){ 
                Relax();
                Log(Time() +  "Rest Time");
            }
            }else{Log(Time() + "INFO: _enabledoor Set to False");}  
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
                }else{ Log(Time() + "INFO: Your Labor is under " + _minlabor); }
        }

        public void Planting()
        {
            var seedcount = itemCount(_seed);
            if ( seedcount == 0){
                Log(Time() + "INFO: You have no seeds!");
                } else{
                    foreach (uint farm in _farms)
                    {
                        //SetPlantMotionType(PlantMotionType.Standart);
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
                    Log(Time() + "INFO: Unable to Purchase due to lack of funds"); 
                    break;
                    } else {
                        Log(Time() + "Lets go buy seeds");
                        gps = new Gps(this); 
                        gps.LoadDataBase(Application.StartupPath + _gpsfile); 
                        gps.GpsMove("Seed");
                        SetTarget(_buyvendor);
                        Log(Time() + "Seed Stock: " + seedcount);
                        if ( seedcount <= _minseed && seedcount <= _maxseed){
                            BuyItems(_seed, _buyseedamt);
                            var seedcount2 = itemCount(_seed);
                            Log(Time() + "Updated Seed Cound: " + seedcount2);
                            
                            var mseconds = random.Next(2000, 5000);
                            var seconds  = mseconds / 1000;
                            Log(Time() +  "Checking seed stock");
                            Thread.Sleep(mseconds);
                        
                        } else {
                            Log(Time() + "[ Seed Purchase Completed ]");
                            break;
                    }
                }
            }
        }
        
        public void Mailer()
        {
            Log(Time() + "[ Starting the Mail Process ]");
            gps = new Gps(this); 
            gps.LoadDataBase(Application.StartupPath + _gpsfile); 
            Log(Time() + "INFO: Moving to Mailbox");
            gps.GpsMove("Mail");
            List<Item> mailitems = new List<Item>();
                {
                    List<Item> Inventory = getInvItems(_plant);
                    foreach (Item item in Inventory)
                    {
                        if (mailitems.Count <= 8)
                        break;
                        mailitems.Add(item);
                        
                    }
                }
                var mailcount = itemCount(_plant);
            
            if (mailcount >= _amtmail){
                Log(Time() + "INFO: Sending " + _plant + " to " + _mailto);
                    SendMail( _mailto, _mailsub, _mailtext, _fastmail, 0, mailitems);

                }else{
                    Log(Time() + "[ Mail Processes Finished ]");
                }
        }
            
        public void Relax()
        {
            DoodadObject restspot;
            restspot = getNearestDoodad(_restitem);
            UseDoodadSkill(_resttext, restspot, true, 0);
        }
        
        public void Timer()
        {
            if ( _oldtimer == true ){
                //  Temporary Sleep to prevent to many checks
                var mseconds = random.Next(240, 300) * 1000;        
                var seconds  = mseconds / 1000;     
                Log(Time() +  "INFO: Waiting " + seconds.ToString() + " seconds to check seeds");     
                Thread.Sleep(mseconds); 
                }else{
                    var plants      = getNearestDoodad(_seedling);    
                    var seconds     = (plants.growthTime); // Gives time in seconds
                    var minutes     = (seconds / 60); // Turns it into Minutes
                    var mseconds    = (seconds * 1000); // Turns it into Miliseconds 
                    var addmseconds = random.Next(60,180) * 1000; // Adding in some random time to look more human
                    var finaltime   = (mseconds + addmseconds); // It's the Final Countdown ( Insert Music !!!!)
                    var logtime     = (finaltime / 1000);
                    
                    Log(Time() + "INFO: Plant Growth Time: " + minutes + " Minutes");
                    Log(Time() + "INFO: Resting " + (logtime / 60) + " Minutes");
                    Thread.Sleep(finaltime);
                }
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
        
            Log(Time() + "[ Farm Markey Inventory Update ]");
            Log(Time() + "INFO: " + _seed + " Count: " + seedcount);
            Log(Time() + "INFO: " + _plant + " Count: " + plantcount);


        }    
            
        //Call on plugin stop
        public void PluginStop()
        {
            Log(Time() + "[ FarmMonkey: Plugin Stopped ]");
        }
    }
}
