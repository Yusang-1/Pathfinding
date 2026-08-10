using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts.CrowdSimulation.UI;
using Unity.Entities;

namespace Assets.Scripts.ECS
{
    public class ECSUnitSpawner : MonoBehaviour
    {
        [SerializeField] private CrowdUnitAuthoring unitPrefab;
        [SerializeField] private UISpawnUnit uiSpawnUnit;

        private void Start()
        {
            uiSpawnUnit.OnSpawnUnitRequested += SpawnUnit;
        }

        public void SpawnUnit()
        {
            var unit = Instantiate<CrowdUnitAuthoring>(unitPrefab);
            var subscene = SceneManager.GetSceneByName("Test Sub Scene");
            SceneManager.MoveGameObjectToScene(unit.gameObject, subscene);
        }
    }
}

