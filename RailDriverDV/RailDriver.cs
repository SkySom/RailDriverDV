using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PIEHid64Net;
using UnityEngine;

namespace RailDriverDV
{
    public class RailDriver : IDisposable
    {
        private readonly PIEDevice _device;
        private readonly RailDriverState _state;
        private RailDriver(PIEDevice device)
        {
            _device = device;
            _state = new RailDriverState();
        }
        
        [CanBeNull]
        public static RailDriver Setup()
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

            return new RailDriver(pieDevice);
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
        public readonly ButtonState Bell = new ButtonState(13, 0x02);
        private byte[] _lastData;
        
        public void UpdateFrom(byte[] data)
        {
            Bell.UpdateFrom(data);
            _lastData = data;
        }
        
        public override string ToString()
        {
            return "Bell: " + Bell + "\n lastData: " + RailDriver.FormatBytes(_lastData);
        }
    }

    public class ButtonState
    {
        private readonly int _index;
        private readonly byte _mask;

        private bool _current;
        private bool _prev;
        public ButtonState(int index, byte mask)
        {
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

        public override string ToString()
        {
            return "_current: " + _current;
        }
    }
}