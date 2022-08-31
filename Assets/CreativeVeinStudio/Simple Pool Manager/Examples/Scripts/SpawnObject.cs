using CreativeVeinStudio.Simple_Pool_Manager.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeVeinStudio.Simple_Pool_Manager.Examples.Scripts
{
    public class SpawnObject : MonoBehaviour
    {
        public Slider slider;

        [Range(0.5f, 3)] public float spawnSpeed = 5;
        public Transform[] positions;

        private const string PoolName = "Enemy";
        private float _timer;
        private GameObject _enemy;

        private MoveToPos _enemyMove;
        private Transform _enemyTrans;
        private Transform _currentTrans;
        private bool _canSpawn;

        private IPoolActions _spManager;

        private void Awake()
        {
            slider.value = (10 / spawnSpeed);
            _spManager = FindObjectOfType<SpManager>();
        }

        private void Update()
        {
            if (!_canSpawn) return;

            _timer += Time.deltaTime;
            if (!(_timer > spawnSpeed)) return;
            _timer = 0;
            _enemy = _spManager.GetRandomPoolItem(PoolName);
            if (!_enemy) return;
            _enemyMove = _enemy.GetComponent<MoveToPos>();
            _enemyTrans = _enemyMove.transform;

            _currentTrans = transform;
            _enemyTrans.position = _currentTrans.position;
            _enemyTrans.rotation = _currentTrans.rotation;
            _enemyMove.SetMoveToPos(positions[Random.Range(0, positions.Length)]);
            _enemyMove.gameObject.SetActive(true);
        }

        public void SetSpawnSpeed(float val)
        {
            spawnSpeed = (10 / val) > 1 ? 1 : (10 / val);
        }

        public void CanSpawn(bool val)
        {
            _canSpawn = val;
        }
    }
}