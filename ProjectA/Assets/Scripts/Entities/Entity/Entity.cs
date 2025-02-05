﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace Entities
{
    public abstract partial class Entity : MonoBehaviour
    {
        //1: CONSTANTS AND STATIC

        //2: SERIALIZED
        public int MaxHP;
        public int hp;
        public HealthBar healthBar;
        public AudioClip hurtSound;
        public AudioClip deathSound;

        //3: NON SERIALIZED AND ACCESSORS
        internal protected AudioSource audioSrc;
        internal protected SpriteRenderer spriteRenderer;
        public abstract Rigidbody2D Rigidbody { get; }
        public abstract float DefaultSpeed { get; }
        public abstract float CurrentSpeed { get; protected set; }

        //4: METHODS
        /// <summary>
        /// modifies damage dealt to the entity
        /// </summary>
        public delegate void DamageModifier(ref int damage);
        public DamageModifier damageModifiers = null;

        /// <summary>
        /// Modifies the speed temporarily
        /// </summary>
        /// <param name="currentSpeed"></param>
        /// <returns></returns>
        public delegate float SpeedModifier(float currentSpeed);
        private LinkedList<SpeedModifier> speedModifiers;
        private float CalculateSpeed()
        {
            float speed = DefaultSpeed;
            foreach (var mod in speedModifiers)
            {
                speed = mod(speed);
            }
            return speed;
        }
        /// <summary>
        /// Removes a modifier. May or may not have a problem with duplicate modifiers.
        /// </summary>
        /// <param name="modifier"></param>
        public void RemoveModifier(SpeedModifier modifier)
        {
            speedModifiers.Remove(modifier);
            UpdateSpeed();
        }
        public void UpdateSpeed(SpeedModifier modifier)
        {
            speedModifiers.AddLast(modifier);
            UpdateSpeed();
        }
        public void UpdateSpeed()
        {
            CurrentSpeed = CalculateSpeed();
        }

        /// <summary>
        /// Called when damage is done to an entity.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="force"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public virtual bool DealDamage(int damage, float force, Vector2 from)
        {
            //status effects
            IterateStatusEffects(s => s.OnDamage(this, damage));

            //apply damage modifiers
            damageModifiers?.Invoke(ref damage);

            hp -= damage;

            //prevent overheal
            hp = Mathf.Clamp(hp, 0, MaxHP);

            //updates the healthbar
            healthBar.SetHealth(hp, MaxHP);

            //hit marker
            CameraReference.Instance.InstantiateHitMarker(damage, transform.position);

            if (hp <= 0) {
                Die();
            }
            else {
                StartCoroutine("DamageFlash");
                //If not dead play hurt
                if (hurtSound != null)
                {
                    audioSrc.PlayOneShot(hurtSound);
                }
            }

            //apply impulse
            ApplyImpulse(force, from);

            return true;
        }
        /// <summary>
        /// Called when knockback is applied to an entity, usually accompanied by damage.
        /// </summary>
        /// <param name="force"></param>
        /// <param name="from"></param>
        protected virtual void ApplyImpulse(float force, Vector2 from)
        {
            Vector2 disp = (Vector2)transform.position - from;
            Rigidbody.AddForce(disp.normalized * force, ForceMode2D.Impulse);
        }
        //Note: currently not being used yet
        /// <summary>
        /// Heals entity
        /// </summary>
        /// <param name="health"></param>
        /// <returns></returns>
        public virtual bool Heal(ref int health) { hp += health; return true; }
        /// <summary>
        /// Kills the entity
        /// </summary>
        public virtual void Die()
        {
            if (hp <= 0)
            {
                gameObject.SetActive(false);
                if (deathSound != null)
                {
                    audioSrc.PlayOneShot(deathSound);
                }
            }
        }

        //5: MONOBEHAVIOUR
        public virtual void Awake() { }
        public virtual void Start()
        {
            if (healthBar != null) healthBar.SetHealth(hp, MaxHP);
            spriteRenderer  = GetComponent<SpriteRenderer>();
            audioSrc        = GetComponent<AudioSource>();

            //currentspeed is assigned at start automatically
            CurrentSpeed = DefaultSpeed;
        }
        public virtual void Update()
        {
            //update the status effect
            IterateStatusEffects(s => s.OnUpdate(this));
        }

        protected IEnumerator DamageFlash()
        {
            spriteRenderer.material.SetColor("_FlashColor", Color.white); 
            spriteRenderer.material.SetFloat("_FlashAmount", 1);
            yield return new WaitForSeconds(.1f);
            spriteRenderer.material.SetColor("_FlashColor", Color.red);
            yield return new WaitForSeconds(.1f);
            spriteRenderer.material.SetFloat("_FlashAmount", 0);
        }
        protected IEnumerator FadeAway(float time)
        {
            float timeStep = .1f;
            Color c = spriteRenderer.material.GetColor("_Color");
            for (float i = 1; i >= 0f; i -= (timeStep / time))
            {
                spriteRenderer.material.SetColor("_Color",new Color(c.r, c.g, c.b, i * c.a));
                yield return new WaitForSeconds(timeStep);
            }
            gameObject.SetActive(false);
        }
        //etc.
    }
}
