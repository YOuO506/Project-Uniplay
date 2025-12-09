using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game_Shooting
{
    public class Enemy : MonoBehaviour
    {
        public float speed;
        public int health;
        public int enemyScore;
        public Sprite[] sprits;

        public string enemyName;

        public float maxShotDelay;
        public float curShotDelay;

        public GameObject bulletObj0;
        public GameObject bulletObj1;
        public GameObject itemCoin;
        public GameObject itemPower;
        public GameObject itemBoom;
        public GameObject player;
        public ObjectManager objectManager;
        public GameManager gameManager;

        SpriteRenderer spriteRenderer;
        Animator anim;

        public int patternIndex;
        public int curPatternCount;
        public int[] maxPatternCount;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (enemyName == "B")
            {
                anim = GetComponent<Animator>();
            }
        }

        void OnEnable()
        {
            switch (enemyName)
            {
                case "B":
                    health = 200;
                    Invoke("Stop", 1.5f);
                    break;
                case "L":
                    health = 40;
                    break;
                case "M":
                    health = 10;
                    break;
                case "S":
                    health = 3;
                    break;
            }
        }

        void Stop()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            Rigidbody2D rigid = GetComponent<Rigidbody2D>();
            rigid.velocity = Vector2.zero;

            Invoke("Think", 2);
        }

        void Think()
        {
            patternIndex = patternIndex == 3 ? 0 : patternIndex + 1;
            curPatternCount = 0;

            switch (patternIndex)
            {
                case 0:
                    FireFoward();
                    break;
                case 1:
                    FireShot();
                    break;
                case 2:
                    FireArc();
                    break;
                case 3:
                    FireAround();
                    break;
            }
        }

        void FireFoward()
        {
            if (health <= 0)
                return;

            //Fire 4 Bullet Foward
            GameObject bulletL = objectManager.MakeObj("BulletBoss0");
            bulletL.transform.position = transform.position + Vector3.left * 0.3f;
            GameObject bulletLL = objectManager.MakeObj("BulletBoss0");
            bulletLL.transform.position = transform.position + Vector3.left * 0.45f;
            GameObject bulletR = objectManager.MakeObj("BulletBoss0");
            bulletR.transform.position = transform.position + Vector3.right * 0.3f;
            GameObject bulletRR = objectManager.MakeObj("BulletBoss0");
            bulletRR.transform.position = transform.position + Vector3.right * 0.45f;

            Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();

            rigidL.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
            rigidLL.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
            rigidR.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
            rigidRR.AddForce(Vector2.down * 8, ForceMode2D.Impulse);

            //Pattern Counting
            curPatternCount++;

            if (curPatternCount < maxPatternCount[patternIndex])
            {
                Invoke("FireFoward", 2);
            }
            else
            {
                Invoke("Think", 3);
            }
        }
        void FireShot()
        {
            if (health <= 0)
                return;

            //Fire 5 Random Shotgun Bullet to Player
            for (int index = 0; index < 5; index++)
            {
                GameObject bullet = objectManager.MakeObj("BulletEnemy1");
                bullet.transform.position = transform.position;

                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();

                Vector2 dirVec = player.transform.position - transform.position;
                Vector2 ranVec = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0f, 2f));
                dirVec += ranVec;
                rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
            }

            //Pattern Counting
            curPatternCount++;

            if (curPatternCount < maxPatternCount[patternIndex])
            {
                Invoke("FireShot", 3.5f);
            }
            else
            {
                Invoke("Think", 3);
            }
        }
        void FireArc()
        {
            if (health <= 0)
                return;

            //Fire Arc Contibue Fire
            GameObject bullet = objectManager.MakeObj("BulletEnemy0");
            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.identity;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();

            Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 10 * curPatternCount / maxPatternCount[patternIndex]), -1);
            rigid.AddForce(dirVec.normalized * 5, ForceMode2D.Impulse);

            //Pattern Counting
            curPatternCount++;

            if (curPatternCount < maxPatternCount[patternIndex])
            {
                Invoke("FireArc", 0.15f);
            }
            else
            {
                Invoke("Think", 3);
            }
        }
        void FireAround()
        {
            if (health <= 0)
                return;

            //Fire Around
            int roundNumA = 50;
            int roundNumB = 40;
            int roundNum = curPatternCount % 2 == 0 ? roundNumA : roundNumB;

            for (int index = 0; index < roundNum; index++)
            {
                GameObject bullet = objectManager.MakeObj("BulletBoss1");
                bullet.transform.position = transform.position;
                bullet.transform.rotation = Quaternion.identity;

                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();

                Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 2 * index / roundNum)
                                            , Mathf.Sin(Mathf.PI * 2 * index / roundNum));
                rigid.AddForce(dirVec.normalized * 2, ForceMode2D.Impulse);

                Vector3 rotVec = Vector3.forward * 360 * index / roundNum + Vector3.forward * 90;
                bullet.transform.Rotate(rotVec);
            }

            //Pattern Counting
            curPatternCount++;

            if (curPatternCount < maxPatternCount[patternIndex])
            {
                Invoke("FireAround", 0.7f);
            }
            else
            {
                Invoke("Think", 3);
            }
        }


        void Update()
        {
            if (enemyName == "B")
            {
                return;
            }
            Fire();
            Reload();
        }

        void Fire()
        {
            //Enemy Fire
            if (curShotDelay < maxShotDelay)
            {
                return;
            }

            if (enemyName == "S")
            {
                GameObject bullet = objectManager.MakeObj("BulletEnemy0");
                bullet.transform.position = transform.position;

                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();

                Vector3 dirVec = player.transform.position - transform.position;
                rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
            }
            else if (enemyName == "L")
            {
                GameObject bulletL = objectManager.MakeObj("BulletEnemy1");
                bulletL.transform.position = transform.position + Vector3.left * 0.3f;
                GameObject bulletR = objectManager.MakeObj("BulletEnemy1");
                bulletR.transform.position = transform.position + Vector3.right * 0.3f;

                Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();

                Vector3 dirVecL = player.transform.position - (transform.position + Vector3.left * 0.3f);
                Vector3 dirVecR = player.transform.position - (transform.position + Vector3.right * 0.3f);

                rigidL.AddForce(dirVecL.normalized * 4, ForceMode2D.Impulse);
                rigidR.AddForce(dirVecR.normalized * 4, ForceMode2D.Impulse);
            }

            curShotDelay = 0; //초기화
        }

        void Reload()
        {
            curShotDelay += Time.deltaTime;
        }

        public void OnHit(int dmg)
        {
            if (health <= 0)
                return;

            health -= dmg;
            if (enemyName == "B")
            {
                anim.SetTrigger("OnHit");
            }
            else
            {
                spriteRenderer.sprite = sprits[1];
                Invoke("ReturnSprite", 0.1f);
            }

            if (health <= 0)
            {
                Player playerLogic = player.GetComponent<Player>();
                playerLogic.score += enemyScore;

                //Random Ratio Item Drop
                int ran = enemyName == "B" ? 0 : Random.Range(0, 10);

                if (ran < 3)
                {
                    //Not Item 30%
                    Debug.Log("Not Item");
                }
                else if (ran < 6)
                {
                    //Coin 30%
                    GameObject itemCoin = objectManager.MakeObj("ItemCoin");
                    itemCoin.transform.position = transform.position;
                }
                else if (ran < 8)
                {
                    //Power 20%
                    GameObject itemPower = objectManager.MakeObj("ItemPower");
                    itemPower.transform.position = transform.position;
                }
                //else if (ran < 10)
                //{
                //    //Boom 20%
                //    GameObject itemBoom = objectManager.MakeObj("ItemBoom");
                //    itemBoom.transform.position = transform.position;
                //}
                else if (ran < 10)
                {
                    // 액션(폭탄) 장착 시에만 드랍
                    int actionPref = PlayerPrefs.GetInt("UsingShootingAction", -1);
                    Debug.Log("[Enemy] UsingShootingAction = " + actionPref);

                    // 장착된 아이템이 "Boom"일 때만 드랍
                    if (actionPref == 9)
                    {
                        GameObject itemBoom = objectManager.MakeObj("ItemBoom");
                        itemBoom.transform.position = transform.position;
                    }
                    else
                    {
                        Debug.Log("[Enemy] Boom 미장착 상태 → 드랍 스킵");
                    }
                }

                CancelInvoke();
                gameObject.SetActive(false);
                transform.rotation = Quaternion.identity;
                gameManager.CallExplosion(transform.position, enemyName);

                //Boss Kill
                //if (enemyName == "B")
                    //gameManager.StageEnd();
            }
        }

        void ReturnSprite()
        {
            spriteRenderer.sprite = sprits[0];
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "BorderBullet" && enemyName != "B")
            {
                gameObject.SetActive(false);
                transform.rotation = Quaternion.identity;
            }

            else if (collision.gameObject.tag == "PlayerBullet")
            {
                Bullet bullet = collision.gameObject.GetComponent<Bullet>();
                OnHit(bullet.dmg);

                collision.gameObject.SetActive(false);
                //Destroy(collision.gameObject);
            }
        }
    }
}

