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

        private RailDriver(PIEDevice device)
        {
            _device = device;
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
            return RailDriverState.FromBytes(data);
        }
        
        private static string FormatBytes(IEnumerable<byte> bytes)
        {
            return string.Join(" ", bytes.Select(each => each.ToString("X2")));
        }
    }
    


    public class RailDriverState
    {
        public bool Bell
        {
            private set;
            get;
        }


        public static RailDriverState FromBytes(byte[] bytes)
        {
            var state = new RailDriverState
            {
                Bell = (bytes[13] & 0x02) != 0
            };

            return state;
        }
    }
}