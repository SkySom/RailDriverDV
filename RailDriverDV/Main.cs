using JetBrains.Annotations;
using UnityEngine;
using UnityModManagerNet;

namespace RailDriverDV
{
    [EnableReloading]
    public static class Main
    {
        [CanBeNull] private static RailDriver _railDriver;

        private static Settings _settings = new Settings();

        [UsedImplicitly]
        public static bool Load(UnityModManager.ModEntry entry)
        {
            _settings = UnityModManager.ModSettings.Load<Settings>(entry);

            entry.OnToggle = OnToggle;
            entry.OnFixedUpdate = OnUpdate;
            entry.OnGUI = OnGUI;
            entry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry entry, bool active)
        {
            if (active)
            {
                _railDriver?.Dispose();
                var newRailDriver = RailDriver.Setup(_settings);
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

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            _settings.Draw(_railDriver?.GetState());
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            _settings.Save(modEntry);
        }

        private static void OnUpdate(UnityModManager.ModEntry entry, float delta)
        {
            var lastLoco = PlayerManager.LastLoco;

            if (lastLoco == null || _railDriver == null) return;

            var locoControl = lastLoco.GetComponent<LocoControllerBase>();

            var state = _railDriver.GetState();

            if (state.IsChanged())
            {
                Debug.Log(state.ToString());
            }

            if (state.Sand.IsChanged() && state.Sand.IsButtonDown())
            {
                locoControl.UpdateSand(locoControl.IsSandOn() ? ToggleDirection.DOWN : ToggleDirection.UP);
            }

            if (state.Horn.IsChanged())
            {
                locoControl.UpdateHorn(state.Horn.Location());
            }

            if (state.Reverser.IsChanged())
            {
                if (state.Reverser.InMiddle())
                {
                    locoControl.SetReverser(0F);
                }
                else if (state.Reverser.Location() < state.Reverser.GetCalibration().Middle)
                {
                    locoControl.SetReverser(1F);
                }
                else
                {
                    locoControl.SetReverser(-1F);
                }
            }

            if (state.Throttle.IsChanged())
            {
                locoControl.SetThrottle(state.Throttle.GetDifference());
            }

            if (state.TrainBrake.IsChanged())
            {
                locoControl.SetBrake(1F - state.TrainBrake.GetDifference());
            }

            if (state.IndependentBrake.IsChanged())
            {
                locoControl.SetIndependentBrake(1F - state.IndependentBrake.GetDifference());
            }
        }
    }
}