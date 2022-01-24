using UnityEngine;
using UnityModManagerNet;

namespace RailDriverDV
{
    public class Settings : UnityModManager.ModSettings
    {
        public readonly ReverserSetting Reverser = new ReverserSetting();
        
        public void Draw()
        {
            GUILayout.BeginVertical();
            
            Reverser.Draw();
            
            GUILayout.EndVertical();
        }

        public override void Save(UnityModManager.ModEntry entry)
        {
            Save(this, entry);
        }
    }

    public class ReverserSetting
    {
        private 
        public void Draw()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Reverse Calibrations");
            
            GUILayout.EndVertical();
        }
    }
}