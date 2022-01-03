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
        [CanBeNull] private static GameObject _rootObject;

        [UsedImplicitly]
        public static bool Load(UnityModManager.ModEntry entry)
        {
            entry.OnToggle = OnToggle;
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

                if (_rootObject != null) return true;
                
                _rootObject = new GameObject();
                Object.DontDestroyOnLoad(_rootObject);
                _rootObject.AddComponent<LocoControl>();
                var locoControl = _rootObject.GetComponent<LocoControl>();
                locoControl.RailDriver = _railDriver;
                locoControl.Start();
            }
            else
            {
                if (_rootObject != null)
                {
                    var locoControl = _rootObject.GetComponent<LocoControl>();
                    locoControl.Stop();
                    Object.Destroy(_rootObject);
                }
                
                _railDriver?.Dispose();
                _railDriver = null;
            }
            return true;
        }
    }
}