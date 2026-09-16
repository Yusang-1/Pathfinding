using System;

namespace Assets.Scripts.ControllUnit
{
    public class SpawnAreaSetter
    {
        public event Action<ActionMaps> OnStartSetSpawnAreaRequested;
        private Action setFinishAction;

        public void StartSetSpawnArea(Action finishAction)
        {
            setFinishAction = finishAction;
            OnStartSetSpawnAreaRequested?.Invoke(ActionMaps.SpawnAreaSetter);
        }

        public void FinishSetSpawnArea()
        {
            setFinishAction?.Invoke();
            setFinishAction = null;
        }
    }
    
    public class SpawnAreaSetter2
    {
        public event Action<ActionMaps> OnStartSetSpawnAreaRequested;
        private Action setFinishAction;

        public void StartSetSpawnArea(Action finishAction)
        {
            setFinishAction = finishAction;
            OnStartSetSpawnAreaRequested?.Invoke(ActionMaps.SpawnAreaSetter);
        }

        public void FinishSetSpawnArea()
        {
            setFinishAction?.Invoke();
            setFinishAction = null;
        }
    }
}
