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
            var lastLoco = PlayerManager.LastLoco;

            if (lastLoco == null || _railDriver == null) return;

            var locoControl = lastLoco.GetComponent<LocoControllerBase>();
            ILocoWrapper locoWrapper = null;

            if (locoControl is LocoControllerShunter shunter)
            {
                locoWrapper = new ShunterLocoWrapper(shunter);
            }

            if (locoWrapper == null)
            {
                return;
            }

            var state = _railDriver.GetState();

            if (state.IsChanged())
            {
                Debug.Log(state.ToString());
            }

            if (state.Bell.IsChanged())
            {
                locoControl.UpdateHorn(state.Bell.IsButtonDown() ? 1.0F : 0.0F);
            }

            if (state.Power.IsChanged())
            {
                if (state.Power.IsButtonDown())
                {
                    locoWrapper.SetRunning(true);
                }
            }
            
            
        }
    }
}