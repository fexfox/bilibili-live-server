using CreativeVeinStudio.Simple_Pool_Manager.Interfaces;
using UnityEngine;

namespace CreativeVeinStudio.Simple_Pool_Manager
{
    public class DeSpawnAfterTime : MonoBehaviour
    {
        public float deSpawnTime = 2;
        private float _timer = 0;

        private IPoolActions _spManager;

        private void Awake()
        {
            _spManager = FindObjectOfType<SpManager>();
        }

        private void OnDisable()
        {
            _timer = 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("Wall"))
            {
                _spManager.DisablePoolObject(gameObject);
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (!(_timer > deSpawnTime)) return;
            _timer = 0;
            // Allows you to disable the pool object you are using
            // in this case the object is the same projectile it retrieved
            _spManager.DisablePoolObject(gameObject);
        }
    }
}