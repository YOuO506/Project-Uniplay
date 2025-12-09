using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;

namespace Game_Shooting
{
    public class Follwer : MonoBehaviour
    {
        public float maxShotDelay;
        public float curShotDelay;

        public ObjectManager objectManager;

        public Vector3 followPos;
        public int followDelay;
        public Transform parent;
        public Queue<Vector3> parentPos;

        void Awake()
        {
            parentPos = new Queue<Vector3>();
        }

        void Update()
        {
            Watch();
            Follow();
            Fire();
            Reload();
        }

        void Watch()
        {
            //Input Pos
            if (!parentPos.Contains(parent.position))
            {
                parentPos.Enqueue(parent.position);
            }

            //OutPut Pos
            if (parentPos.Count > followDelay)
            {
                followPos = parentPos.Dequeue();
            }
            else if (parentPos.Count < followDelay)
            {
                followPos = parent.position;
            }
        }

        void Follow()
        {
            transform.position = followPos;
        }

        void Fire()
        {
            //Player Fire
            if (!Input.GetButton("Fire1"))
            {
                return;
            }

            if (curShotDelay < maxShotDelay)
            {
                return;
            }

            GameObject bullet = objectManager.MakeObj("BulletFollower");
            bullet.transform.position = transform.position;

            Rigidbody2D rigidd = bullet.GetComponent<Rigidbody2D>();
            rigidd.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

            curShotDelay = 0; //√ ±‚»≠
        }

        void Reload()
        {
            curShotDelay += Time.deltaTime;
        }
    }

}
