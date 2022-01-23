namespace RailDriverDV
{
    public interface ILocoWrapper
    {
        void SetRunning(bool running);
    }
    
    public class ShunterLocoWrapper: ILocoWrapper
    {
        private readonly LocoControllerShunter _shunter;

        public ShunterLocoWrapper(LocoControllerShunter shunter)
        {
            _shunter = shunter;
        }

        public void SetRunning(bool running)
        {
            if (running != _shunter.GetEngineRunning())
            {
                _shunter.SetEngineRunning(running);
            }
        }
    }
}