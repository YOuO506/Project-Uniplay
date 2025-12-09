using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game_Run
{
    public class Sounder : MonoBehaviour
    {
        public enum Sfx { Jump, Land, Hit, Reset }
        public AudioClip[] clips;

        AudioSource audios;

        void Awake()
        {
            audios = GetComponent<AudioSource>();
        }

        public void PlaySound(Sfx sfx)
        {
            audios.clip = clips[(int)sfx];
            audios.Play();
        }
    }
}

