﻿using Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CooldownTimer;
using static Boomerang;
using Entities;

namespace Items
{
    public class Unarmed : Weapon2
    {
        public static readonly string itemName = "Unarmed";
        public override string Name => itemName;
        public static Sprite sprite;
        public override Sprite Sprite => sprite;
        public override AnimationControllerID AnimationControllerID => AnimationControllerID.unarmed;

        public float radius;
        public float arcRadians; //sq
        public int damage;
        public float force;
        public float lungeForce;

        private Cooldown comboReset;
        //integral is approximately one but because Unity essencially does remann sum, it is likely more
        //should be protected static member of Weapon
        Converter lungeConverter = f => f < .1f ? 0f : Mathf.Max(0, (2 - Mathf.Pow(1.5f * f, 2))); 
        //Initalized in start

        public override void AltFire(Transform player, bool down)
        {
            if (down)
                UnityEngine.Debug.Log("You misplace your weapon");
        }

        public override void Fire(Transform player, bool down)
        {
            if (!down) return;
            if (!IsReady) return;
            if (!Player.Instance.playerMove.PathEnd) return;

            //need to call the specific coroutine on this object
            Owner.StartCoroutine(AttackSequence());
            
            SetUseTime();
        }

        protected override void DropItem(out bool success)
        {
            success = false;
        }

        public Unarmed() : base(1)
        {

        }

        //common
        private void HitScan() //overlap circle not hitscan but whatever
        {
            Transform player = Player.Instance.gameObject.transform;
            var collisions = Physics2D.OverlapCircleAll(player.position, radius);
            foreach (var col in collisions)
            {
                if (col.gameObject.CompareTag("Monster"))
                {
                    //check the arc
                    //this needs fix
                    Vector2 disp = col.transform.position - player.position;
                    Vector2 cDisp = CameraReference.Instance.camera.ScreenToWorldPoint(Input.mousePosition) - player.position;
                    double arc = Mathf.Atan2(disp.x, disp.y) - Math.Atan2(cDisp.x, cDisp.y);
                    if (Math.Abs(arc) < Mathf.Abs(arcRadians))
                    {
                        //deal damage
                        Entity entity = col.GetComponent<Entity>();
                        bool success = entity.DealDamage(damage, force, player.position);
                    }
                }
            }
        }
        //common
        private IEnumerator AttackSequence()
        {
            Vector2 lookDir = (CameraReference.Instance.camera.ScreenToWorldPoint(Input.mousePosition) - Player.Instance.gameObject.transform.position).normalized;

            Animator anim = Player.Instance.GetComponent<Animator>();
            anim.SetTrigger("Attack");
            if (comboReset.IsReady)
            {
                anim.SetInteger("Combo", 0);
            }
            else if (anim.GetInteger("Combo") == 0)
            {
                anim.SetInteger("Combo", 1);
            }
            else
            {
                anim.SetInteger("Combo", 0);
            }
            comboReset.Use(1f);
            anim.SetFloat("xInput", lookDir.x);
            anim.SetFloat("yInput", lookDir.y);

            Path lungePath = LinePath(lungeConverter, lookDir);
            Player.Instance.playerMove.SetPath(Boomerang.Mult(Player.Instance.playerMove.defaultSpeed, lungePath), .25f);

            yield return new WaitForSeconds(.1f);
            HitScan();
            yield return new WaitForSeconds(.1f);
            HitScan();
        }
    }
}
