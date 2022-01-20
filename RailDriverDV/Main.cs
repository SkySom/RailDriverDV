using CommandTerminal;
using JetBrains.Annotations;
using UnityEngine;
using UnityModManagerNet;

namespace RailDriverDV
{
    [EnableReloading]
    public static class Main
    {
        [CanBeNull] private static RailDriver _railDriver;

        [UsedImplicitly]
        public static bool Load(UnityModManager.ModEntry entry)
        {
            entry.OnToggle = OnToggle;
            entry.OnFixedUpdate = OnUpdate;
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry entry, bool active)
        {
            if (active)
            {
                _railDriver?.Dispose();
                var newRailDriver = RailDriver.Setup();
                if (newRailDriver == null)
                {
                    entry.Logger.Error("Failed to Setup RailDriver");
                    return false;
                }
                
                Debug.Log(newRailDriver.ProductString());

                _railDriver = newRailDriver;
            }
            else
            {
                _railDriver?.Dispose();
                _railDriver = null;
            }
            return true;
        }

        private static void OnUpdate(UnityModManager.ModEntry entry, float delta)
        {
            var lastLoco= PlayerManager.LastLoco;

            if (lastLoco == null || _railDriver == null) return;

            var state = _railDriver.GetState();
            
            
        }
    }
}