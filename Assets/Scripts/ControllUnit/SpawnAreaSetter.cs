using System;

namespace Assets.Scripts.ControllUnit
{
    public class SpawnAreaSetter
    {
        public event Action<string> OnStartSetSpawnAreaRequested;
        private Action setFinishAction;

        public void StartSetSpawnArea(Action finishAction)
        {
            setFinishAction = finishAction;
            OnStartSetSpawnAreaRequested?.Invoke("SpawnAreaSetter");
        }

        public void FinishSetSpawnArea()
        {
            setFinishAction?.Invoke();
            setFinishAction = null;
        }
    }
}
