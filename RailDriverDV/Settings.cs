using System.Xml.Serialization;
using JetBrains.Annotations;
using UnityEngine;
using UnityModManagerNet;

namespace RailDriverDV
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [XmlAttribute]
        public readonly LeverCalibration ReverserCal = new LeverCalibration(
            driver => driver.Reverser,
            "Forward",
            "Reverse",
            "Neutral"
        );
        
        [XmlAttribute]
        public readonly LeverCalibration ThrottleCal = new LeverCalibration(
            driver => driver.Throttle,
            "Zero Throttle",
            "Full Throttle",
            null
        );
        
        [XmlAttribute]
        public readonly LeverCalibration TrainBrakeCal = new LeverCalibration(
            driver => driver.TrainBrake,
            "Full Brake",
            "Released",
            null
        );
        
        [XmlAttribute]
        public readonly LeverCalibration IndependentBrakeCal = new LeverCalibration(
            driver => driver.IndependentBrake,
            "Full Brake",
            "Released",
            null
        );

        public void Draw([CanBeNull] RailDriverState railDriver)
        {
            GUILayout.BeginVertical();
            if (railDriver != null)
            {
                ReverserCal.Draw(railDriver);
                ThrottleCal.Draw(railDriver);
                TrainBrakeCal.Draw(railDriver);
                IndependentBrakeCal.Draw(railDriver);
            }
            else
            {
                GUILayout.Label("RailDriver must be connected to Calibrate.");
            }


            GUILayout.EndVertical();
        }

        public override void Save(UnityModManager.ModEntry entry)
        {
            Save(this, entry);
        }

        public void OnChange()
        {
        }
    }

    public class LeverCalibration
    {
        private readonly StateGetter _stateGetter;
        private readonly string _minName;
        private readonly string _maxName;
        private readonly string _middleName;

        [XmlAttribute] public byte Min { set; get; }
        [XmlAttribute] public byte Max { set; get; }

        [XmlAttribute] public byte Middle { set; get; }

        public LeverCalibration(StateGetter stateGetter, string minName, string maxName,
            [CanBeNull] string middleName)
        {
            _stateGetter = stateGetter;
            _minName = minName;
            _maxName = maxName;
            _middleName = middleName;
            Min = byte.MinValue;
            Max = byte.MaxValue;
        }

        public void Draw(RailDriverState railDriver)
        {
            var leverState = _stateGetter(railDriver);
            GUILayout.BeginVertical();
            GUILayout.Label(leverState.GetName() + " Calibrations");
            if (GUILayout.Button("Calibrate " + _minName))
            {
                Min = leverState.UncheckedLocation();
            }
            
            if (_middleName != null)
            {
                if (GUILayout.Button("Calibrate " + _middleName))
                {
                    Middle = leverState.UncheckedLocation();
                }
            }
            
            if (GUILayout.Button("Calibrate " + _maxName))
            {
                Max = leverState.UncheckedLocation();
            }



            if (GUILayout.Button("Reset"))
            {
                Min = byte.MinValue;
                Max = byte.MaxValue;
                Middle = byte.MinValue;
            }

            GUILayout.EndVertical();
        }

        public bool HasMiddle()
        {
            return _middleName != null;
        }

        public override string ToString()
        {
            return "{ Min: " + Min + ", Max: " + Max + "}";
        }
    }

    public delegate LeverState StateGetter(RailDriverState railDriverState);
}