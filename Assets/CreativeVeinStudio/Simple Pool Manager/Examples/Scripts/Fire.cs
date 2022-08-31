using System;
using CreativeVeinStudio.Simple_Pool_Manager.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeVeinStudio.Simple_Pool_Manager.Examples.Scripts
{
    public class Fire : MonoBehaviour
    {
        public Slider slider;

        public string poolName = "";
        public Transform firePos;

        public float fireRate = 1f;

        private float _timer = 0;
        private IPoolActions _spManager;

        private void Awake()
        {
            _spManager = FindObjectOfType<SpManager>();
            slider.value = fireRate;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (!(_timer <= 0)) return;
            _timer = fireRate;
            InstantiateProjectile();
        }

        private void InstantiateProjectile()
        {
            var projectile = _spManager.GetRandomPoolItem(poolName).GetComponent<Projectile_Move>();
            if (!projectile)
            {
                Debug.Log("Please create thee required Pool item");
                Debug.Break();
            }

            var projectileTrans = projectile.transform;
            projectileTrans.position = firePos.position;
            projectileTrans.rotation = firePos.rotation;
            projectile.gameObject.SetActive(true);
        }

        public void UpdateFireRate(float val)
        {
            fireRate = val;
        }
    }
}