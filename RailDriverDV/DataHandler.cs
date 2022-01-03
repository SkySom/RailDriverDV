using System;
using CommandTerminal;
using PIEHid64Net;
using UnityEngine;

namespace RailDriverDV
{
    public class DataHandler: PIEDataHandler, PIEErrorHandler
    {
        public void HandlePIEHidData(byte[] data, PIEDevice sourceDevice, int error)
        {
            throw new NotImplementedException();
        }

        public void HandlePIEHidError(PIEDevice sourceDevices, long error)
        {
            throw new NotImplementedException();
        }
    }
}