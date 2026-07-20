using System;

namespace Assets.Scripts.ControllUnit
{
    public class SpawnAreaSetter
    {
        public event Action<string> OnStartSetSpawnAreaRequested;
        
        public Action SetFinishAction;
        public void StartSetSpawnArea(Action finishAction)
        {
            SetFinishAction = finishAction;
            OnStartSetSpawnAreaRequested?.Invoke("SpawnAreaSetter");
        }
    }
}
