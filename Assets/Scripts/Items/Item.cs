﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Items
{
    //The Item class defines general 
    [Serializable]
    public abstract class Item : MonoBehaviour , IClickable
    {
        public static readonly int massConstant = 100;

        public abstract Sprite Sprite { get; }
        public Entity Owner { get; protected set; } = null;
        public int Mass { get; protected set; }

        public Item(int mass)
        {
            this.Mass = mass;
        }

        public void DropAt(Vector3 position)
        {
            DropItem(out bool success);
            if (!success) return;
            position.z = 0;
            transform.position = position;
            gameObject.SetActive(true);
        }


        //abstract
        /*
        public abstract void Eat(out int saturation);
        public abstract void Apply(Item other);
        public abstract void Throw(float momentum, ref int damage, Collision2D collision);
        public abstract void Wield(out bool success);
        public abstract void UnWield(out bool success);
        */
        public abstract void Fire(Transform player);
        public abstract void AltFire(Transform player);
        public virtual void DropItem(out bool success)
        {
            success = true;
        }

        void IClickable.OnClick()
        {
            //pick up the item
            int slot = Inventory.GetOpenSlot();

            //check to see if theres an open slot
            if (slot != -1)
            {
                //make the item disappear
                gameObject.SetActive(false);

                //put the item in inventory
                Inventory.AssignTo(this, slot);
            }
            else
            {

            }

        }
    }
}
