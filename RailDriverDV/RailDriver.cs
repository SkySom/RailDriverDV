using System;
using System.Collections.Generic;
using System.Linq;
using DV.CabControls.Spec;
using JetBrains.Annotations;
using PIEHid64Net;
using UnityEngine;

namespace RailDriverDV
{
    public class RailDriver : IDisposable
    {
        private readonly PIEDevice _device;
        private readonly RailDriverState _state;

        private RailDriver(PIEDevice device, Settings settings)
        {
            _device = device;
            _state = new RailDriverState(settings);
        }

        [CanBeNull]
        public static RailDriver Setup(Settings settings)
        {
            var pieDevice = PIEDevice.EnumeratePIE()
                .FirstOrDefault(device => device.HidUsagePage == 0xc && device.Pid == 210);

            if (pieDevice == null)
            {
                return null;
            }

            Debug.Log("Setup: " + pieDevice.SetupInterface());
            pieDevice.suppressDuplicateReports = true;
            pieDevice.callNever = true;

            return new RailDriver(pieDevice, settings);
        }

        public void Dispose()
        {
            _device.CloseInterface();
        }

        public string ProductString()
        {
            return _device.ProductString;
        }

        public RailDriverState GetState()
        {
            var data = new byte[_device.ReadLength];
            _device.ReadLast(ref data);
            _state.UpdateFrom(data);
            return _state;
        }

        public static string FormatBytes(IEnumerable<byte> bytes)
        {
            return string.Join(" ", bytes.Select(each => each.ToString("X2")));
        }
    }

    public class RailDriverState
    {
        public readonly ButtonState Sand = new ButtonState("Sand", 12, 0x80);

        public readonly SwitchState Horn = new SwitchState("Horn", 13, 0x4, 0x8);

        public readonly LeverState Reverser;
        public readonly LeverState Throttle;
        public readonly LeverState TrainBrake;
        public readonly LeverState IndependentBrake;

        private readonly IList<ButtonState> _buttonStates;
        private readonly IList<SwitchState> _switchStates;
        private readonly IList<LeverState> _leverStates;

        private byte[] _lastData;

        public RailDriverState(Settings settings)
        {
            _buttonStates = new List<ButtonState>(new[]
            {
                Sand
            });
            _switchStates = new List<SwitchState>(new[]
            {
                Horn
            });

            Reverser = new LeverState("Reverser", 1, settings.ReverserCal);
            Throttle = new LeverState("Throttle", 2, settings.ThrottleCal);
            TrainBrake = new LeverState("Train Brake", 3, settings.TrainBrakeCal);
            IndependentBrake = new LeverState("Independent Brake", 4, settings.IndependentBrakeCal);
            _leverStates = new List<LeverState>(new[]
            {
                Reverser,
                Throttle,
                TrainBrake,
                IndependentBrake
            });
        }

        public void UpdateFrom(byte[] data)
        {
            foreach (var buttonState in _buttonStates)
            {
                buttonState.UpdateFrom(data);
            }

            foreach (var switchState in _switchStates)
            {
                switchState.UpdateFrom(data);
            }

            foreach (var leverState in _leverStates)
            {
                leverState.UpdateFrom(data);
            }

            _lastData = data;
        }

        public override string ToString()
        {
            return "{\n" +
                   string.Join(",\n", _buttonStates.Select(each => "  " + each.GetName() + ":" + each).ToArray()) +
                   ",\n" +
                   string.Join(",\n", _switchStates.Select(each => "  " + each.GetName() + ":" + each).ToArray()) +
                   ",\n" +
                   string.Join(",\n", _leverStates.Select(each => "  " + each.GetName() + ":" + each).ToArray()) +
                   "\n}";
        }

        public bool IsChanged()
        {
            return _buttonStates.Select(buttonState => buttonState.IsChanged()).Any() ||
                   _switchStates.Select(switchState => switchState.IsChanged()).Any() ||
                   _leverStates.Select(leverState => leverState.IsChanged()).Any();
        }
    }

    public class ButtonState
    {
        private readonly string _name;
        private readonly int _index;
        private readonly byte _mask;

        private bool _current;
        private bool _prev;

        public ButtonState(string name, int index, byte mask)
        {
            _name = name;
            _index = index;
            _mask = mask;
        }

        public void UpdateFrom(byte[] data)
        {
            _prev = _current;
            _current = (data[_index] & _mask) != 0;
        }

        public bool IsChanged()
        {
            return _current != _prev;
        }

        public bool IsButtonDown()
        {
            return _current;
        }

        public string GetName()
        {
            return _name;
        }

        public override string ToString()
        {
            return "{Name: " + _name + ", Current: " + _current + "}";
        }
    }

    public class SwitchState
    {
        private readonly string _name;
        private readonly int _index;
        private readonly byte _upMask;
        private readonly byte _downMask;

        private int _current;
        private int _prev;

        public SwitchState(string name, int index, byte upMask, byte downMask)
        {
            _name = name;
            _index = index;
            _upMask = upMask;
            _downMask = downMask;
        }

        public void UpdateFrom(byte[] data)
        {
            _prev = _current;
            if ((data[_index] & _upMask) != 0)
            {
                _current = 1;
            }
            else if ((data[_index] & _downMask) != 0)
            {
                _current = -1;
            }
            else
            {
                _current = 0;
            }
        }

        public bool IsChanged()
        {
            return _current != _prev;
        }

        public int Location()
        {
            return _current;
        }

        public string GetName()
        {
            return _name;
        }

        public override string ToString()
        {
            return "{Name: " + _name + ", Current: " + _current + "}";
        }
    }

    public class LeverState
    {
        private readonly string _name;
        private readonly int _index;
        private readonly LeverCalibration _calibration;

        private byte _current;
        private byte _unchecked;
        private byte _prev;

        public LeverState(string name, int index, LeverCalibration calibration)
        {
            _name = name;
            _index = index;
            _calibration = calibration;
        }

        public void UpdateFrom(byte[] data)
        {
            _prev = _current;
            _current = data[_index];
            _unchecked = _current;
            if (_current > _calibration.Max)
            {
                _current = _calibration.Max;
            }
            else if (_current < _calibration.Min)
            {
                _current = _calibration.Min;
            }
        }

        public bool IsChanged()
        {
            return _current != _prev;
        }

        public byte Location()
        {
            return _current;
        }

        public LeverCalibration GetCalibration()
        {
            return _calibration;
        }

        public float GetDifference()
        {
            if (Math.Abs(_calibration.Max - Location()) < 3)
            {
                return 1F;
            }

            if (Math.Abs(_calibration.Min - Location()) < 3)
            {
                return 0F;
            }

            var actualMax = _calibration.Max - _calibration.Min;
            var actualLocation = Location() - _calibration.Min;
                
            return actualLocation / (float) actualMax;
        }

        public bool InMiddle()
        {
            if (_calibration.HasMiddle())
            {
                return Math.Abs(Location() - _calibration.Middle) < 15;
            }

            return false;
        }

        public byte UncheckedLocation()
        {
            return _unchecked;
        }

        public string GetName()
        {
            return _name;
        }

        public override string ToString()
        {
            return "{Name: " + _name + ", Current: " + _current + ", Difference: " + GetDifference() + ", Unchecked: " +
                   _unchecked + ", Calibration: " + _calibration + "}";
        }
    }
}