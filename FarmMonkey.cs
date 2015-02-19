using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using ArcheBuddy.Bot.Classes;

namespace FarmMonkey{
   public class FarmMonkey : Core
   {
       public static string GetPluginAuthor()
       { return "Defectuous"; }
       public static string GetPluginVersion()
       { return "1.0.0.0"; }
       public static string GetPluginDescription()
       { return "Farm Monkey: Archeage Archbuddy Farming Plugin"; }
       // Special Thanks to the following Archbuddy Forum members
       // AtomicBomb, Voyager92,
       
       //Call on plugin start
       public void PluginRun()
       {
        
       // Get Family & Guild Names
       
       // Inventory ( Seeds, Animals, Saplings, Feed, Repair Materials )
       
       // Configuration ( Done Through GUI )
	   
	   // Check GPS & Move to Farm Location(s)
	   
	   // Collect & Verify Farm ID's ( Priority Family and Guild Farms )
	   
	   // Harvest ( Plants, Harvest Fruit Tree's )
       
       // Chop Non Fruit Tree's
       
       // Repair Pens & Coops
       
       // Manage Animals (Feed or Medicate, then Feed, Harvest, or Slaughter)
       
       // Plant Seeds & Tree's, Set Animals. ( Plant in Quadrants )
	   // Move to Last planted seed 5 meters distance
       
       // AFK Manager ( Random Timer & Random action such as jump, turn, side step, log off on timer )
	   // 
       
       
       }
       //Call on plugin stop
       public void PluginStop()
       {
       }
   }
}
