﻿using Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class Player : Entity
{
    public static Player Instance { get; private set; }

    private void Start()
    {
        if (Instance != null) Debug.LogError("Multiple players detected");
        Instance = this;
    }

    int exp;
}
