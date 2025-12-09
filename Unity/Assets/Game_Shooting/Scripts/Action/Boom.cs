using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game_Shooting
{
    public class Boom : MonoBehaviour
    {
        private Player player;

        void Awake()
        {
            player = GetComponent<Player>();
            if (player == null) player = GetComponentInParent<Player>();
        }

        void OnEnable()
        {
            if (player != null)
            {
                player.boomEnabled = true;   // ÆøÅº »ç¿ë °¡´É
                Debug.Log("[Boom] ÆøÅº ±â´É È°¼ºÈ­µÊ");
            }
        }

        void OnDisable()
        {
            if (player != null)
            {
                player.boomEnabled = false;  // ÆøÅº »ç¿ë ºÒ°¡
                Debug.Log("[Boom] ÆøÅº ±â´É ºñÈ°¼ºÈ­µÊ");
            }
        }
    }
}