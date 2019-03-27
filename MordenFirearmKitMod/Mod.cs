﻿using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using Modding.Common;
using Modding.Blocks;
using System.Collections;

namespace ModernFirearmKitMod
{

    // If you need documentation about any of these values or the mod loader
    // in general, take a look at http://wiki.spiderlinggames.co.uk/besiege/modding-documentation/articles/introduction.html.

    public enum BlockList
    {
        火箭弹模块 = 650,
        火箭巢模块 = 651,
        机枪模块 = 652,
        指向模块 = 653
    }

    public class MordenFirearmKitBlockMod : ModEntryPoint
    {

        public static GameObject Mod;
        public static GameObject RocketPool_Idle;
        public static GameObject MachineGunBulletPool_Idle;

        public override void OnLoad()
        {
            // Your initialization code here

            Mod = new GameObject("Morden Firearm Kit Mod");
            UnityEngine.Object.DontDestroyOnLoad(Mod);
            RocketPool_Idle = new GameObject("Rocket Pool Idle");
            RocketPool_Idle.transform.SetParent(Mod.transform);
            MachineGunBulletPool_Idle = new GameObject("MachineGunBullet Pool Idle");
            MachineGunBulletPool_Idle.transform.SetParent(Mod.transform);

            AssetManager.Instance.transform.SetParent(Mod.transform);

            //增加灯关渲染数量
            //QualitySettings.pixelLightCount += 10;

        }
    } 
}
