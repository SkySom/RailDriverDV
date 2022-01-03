#nullable enable
using System.Collections;
using UnityEngine;

namespace RailDriverDV
{
    public class LocoControl: MonoBehaviour
    {
        private IEnumerator _controlCoroutine;
        public RailDriver? RailDriver
        {
            set;
            private get;
        }
        private LocoControllerBase? _controllerBase;

        public void Start()
        {
            PlayerManager.CarChanged += ChangingCars;
            _controlCoroutine = ControllingCoroutine();
            StartCoroutine(_controlCoroutine);
        }

        public void Stop()
        {
            PlayerManager.CarChanged -= ChangingCars;
            StopCoroutine(_controlCoroutine);
        }

        private IEnumerator ControllingCoroutine()
        {
            while (true)
            {
                if (_controllerBase != null && RailDriver != null)
                {
                    var state = RailDriver.GetState();
                    _controllerBase.UpdateHorn(state.Bell ? 1.0F : 0.0F);
                }
                
                yield return null;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private void ChangingCars(TrainCar trainCar)
        {
            _controllerBase = trainCar != null ? trainCar.GetComponent<LocoControllerBase>() : null;
        }
    }
}