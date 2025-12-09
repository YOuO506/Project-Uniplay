using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game_Shooting
{
    public class ObjectManager : MonoBehaviour
    {
        public GameObject enemyBPrefab;
        public GameObject enemyLPrefab;
        public GameObject enemyMPrefab;
        public GameObject enemySPrefab;
        public GameObject itemCoinPrefab;
        public GameObject itemPowerPrefab;
        public GameObject itemBoomPrefab;
        public GameObject bulletPlayer0Prefab;
        public GameObject bulletPlayer1Prefab;
        public GameObject bulletEnemy0Prefab;
        public GameObject bulletEnemy1Prefab;
        public GameObject bulletFollowerPrefab;
        public GameObject bulletBoss0Prefab;
        public GameObject bulletBoss1Prefab;
        public GameObject explosionPrefab;

        GameObject[] enemyB;
        GameObject[] enemyS;
        GameObject[] enemyM;
        GameObject[] enemyL;

        GameObject[] itemCoin;
        GameObject[] itemPower;
        GameObject[] itemBoom;

        GameObject[] bulletPlayer0;
        GameObject[] bulletPlayer1;
        GameObject[] bulletEnemy0;
        GameObject[] bulletEnemy1;
        GameObject[] bulletFollower;
        GameObject[] bulletBoss0;
        GameObject[] bulletBoss1;

        GameObject[] explosion;

        GameObject[] targetPool;

        void Awake()
        {
            enemyB = new GameObject[1];
            enemyS = new GameObject[10];
            enemyM = new GameObject[10];
            enemyL = new GameObject[20];

            itemCoin = new GameObject[20];
            itemPower = new GameObject[10];
            itemBoom = new GameObject[10];

            bulletPlayer0 = new GameObject[100];
            bulletPlayer1 = new GameObject[100];
            bulletEnemy0 = new GameObject[100];
            bulletEnemy1 = new GameObject[100];
            bulletFollower = new GameObject[100];
            bulletBoss0 = new GameObject[50];
            bulletBoss1 = new GameObject[1000];

            explosion = new GameObject[20];

            Generate();
        }

        void Generate()
        {
            //Enemy
            for (int index = 0; index < enemyB.Length; index++)
            {
                enemyB[index] = Instantiate(enemyBPrefab);
                enemyB[index].SetActive(false);
            }

            for (int index = 0; index < enemyS.Length; index++)
            {
                enemyS[index] = Instantiate(enemySPrefab);
                enemyS[index].SetActive(false);
            }

            for (int index = 0; index < enemyM.Length; index++)
            {
                enemyM[index] = Instantiate(enemyMPrefab);
                enemyM[index].SetActive(false);
            }

            for (int index = 0; index < enemyL.Length; index++)
            {
                enemyL[index] = Instantiate(enemyLPrefab);
                enemyL[index].SetActive(false);
            }

            //Item
            for (int index = 0; index < itemCoin.Length; index++)
            {
                itemCoin[index] = Instantiate(itemCoinPrefab);
                itemCoin[index].SetActive(false);
            }

            for (int index = 0; index < itemPower.Length; index++)
            {
                itemPower[index] = Instantiate(itemPowerPrefab);
                itemPower[index].SetActive(false);
            }

            for (int index = 0; index < itemBoom.Length; index++)
            {
                itemBoom[index] = Instantiate(itemBoomPrefab);
                itemBoom[index].SetActive(false);
            }

            //Bullet
            for (int index = 0; index < bulletPlayer0.Length; index++)
            {
                bulletPlayer0[index] = Instantiate(bulletPlayer0Prefab);
                bulletPlayer0[index].SetActive(false);
            }

            for (int index = 0; index < bulletPlayer1.Length; index++)
            {
                bulletPlayer1[index] = Instantiate(bulletPlayer1Prefab);
                bulletPlayer1[index].SetActive(false);
            }

            for (int index = 0; index < bulletEnemy0.Length; index++)
            {
                bulletEnemy0[index] = Instantiate(bulletEnemy0Prefab);
                bulletEnemy0[index].SetActive(false);
            }

            for (int index = 0; index < bulletEnemy1.Length; index++)
            {
                bulletEnemy1[index] = Instantiate(bulletEnemy1Prefab);
                bulletEnemy1[index].SetActive(false);
            }

            for (int index = 0; index < bulletFollower.Length; index++)
            {
                bulletFollower[index] = Instantiate(bulletFollowerPrefab);
                bulletFollower[index].SetActive(false);
            }

            for (int index = 0; index < bulletBoss0.Length; index++)
            {
                bulletBoss0[index] = Instantiate(bulletBoss0Prefab);
                bulletBoss0[index].SetActive(false);
            }

            for (int index = 0; index < bulletBoss1.Length; index++)
            {
                bulletBoss1[index] = Instantiate(bulletBoss1Prefab);
                bulletBoss1[index].SetActive(false);
            }

            for (int index = 0; index < explosion.Length; index++)
            {
                explosion[index] = Instantiate(explosionPrefab);
                explosion[index].SetActive(false);
            }

        }

        public GameObject MakeObj(string type)
        {

            switch (type)
            {
                case "EnemyB":
                    targetPool = enemyB;
                    break;
                case "EnemyS":
                    targetPool = enemyS;
                    break;
                case "EnemyM":
                    targetPool = enemyM;
                    break;
                case "EnemyL":
                    targetPool = enemyL;
                    break;
                case "ItemCoin":
                    targetPool = itemCoin;
                    break;
                case "ItemPower":
                    targetPool = itemPower;
                    break;
                case "ItemBoom":
                    targetPool = itemBoom;
                    break;
                case "BulletPlayer0":
                    targetPool = bulletPlayer0;
                    break;
                case "BulletPlayer1":
                    targetPool = bulletPlayer1;
                    break;
                case "BulletEnemy0":
                    targetPool = bulletEnemy0;
                    break;
                case "BulletEnemy1":
                    targetPool = bulletEnemy1;
                    break;
                case "BulletFollower":
                    targetPool = bulletFollower;
                    break;
                case "BulletBoss0":
                    targetPool = bulletBoss0;
                    break;
                case "BulletBoss1":
                    targetPool = bulletBoss1;
                    break;
                case "Explosion":
                    targetPool = explosion;
                    break;
            }

            for (int index = 0; index < targetPool.Length; index++)
            {
                if (!targetPool[index].activeSelf)
                {
                    targetPool[index].SetActive(true);
                    return targetPool[index];
                }
            }

            return null;
        }

        public GameObject[] GetPool(string type)
        {
            switch (type)
            {
                case "EnemyB":
                    targetPool = enemyB;
                    break;
                case "EnemyS":
                    targetPool = enemyS;
                    break;
                case "EnemyM":
                    targetPool = enemyM;
                    break;
                case "EnemyL":
                    targetPool = enemyL;
                    break;
                case "ItemCoin":
                    targetPool = itemCoin;
                    break;
                case "ItemPower":
                    targetPool = itemPower;
                    break;
                case "ItemBoom":
                    targetPool = itemBoom;
                    break;
                case "BulletPlayer0":
                    targetPool = bulletPlayer0;
                    break;
                case "BulletPlayer1":
                    targetPool = bulletPlayer1;
                    break;
                case "BulletEnemy0":
                    targetPool = bulletEnemy0;
                    break;
                case "BulletEnemy1":
                    targetPool = bulletEnemy1;
                    break;
                case "BulletFollower":
                    targetPool = bulletFollower;
                    break;
                case "BulletBoss0":
                    targetPool = bulletBoss0;
                    break;
                case "BulletBoss1":
                    targetPool = bulletBoss1;
                    break;
                case "Explosion":
                    targetPool = explosion;
                    break;
            }

            return targetPool;
        }
    }

}
