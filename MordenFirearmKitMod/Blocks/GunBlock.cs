﻿using System;
using System.Collections.Generic;
using spaar.ModLoader;
using TheGuysYouDespise;
using UnityEngine;
using System.Collections;

namespace MordenFirearmKitMod
{

    partial class MordenFirearmKitMod
    {
        
        //机枪模块
        public Block MachineGun = new Block()
            ///ID of the Block
            .ID(652)

            ///Name of the Block
            .BlockName("MachineGun")

            ///Load the 3d model information
            .Obj(new List<Obj> { new Obj("/MordenFirearmKitMod/Barrel.obj", //Mesh name with extension (only works for .obj files)
                                         "/MordenFirearmKitMod/Butt.png", //Texture name with extension
                                         new VisualOffset(Vector3.one * 0.65f, //Scale
                                                          new Vector3(0,0,1.55f), //Position                                                         
                                                          Vector3.zero))//Rotation
            })

            ///For the button that we will create setup the visual offset needed
            .IconOffset(new Icon(Vector3.one * 0.65f,  //Scale
                              Vector3.zero,  //Position
                              new Vector3(  37.5f,   90f,   30f))) //Rotation

            ///Script, Components, etc. you want to be on your block.
            .Components(new Type[] {typeof(GunScript),
              })

            ///Properties such as keywords for searching and setting up how how this block behaves to other elements.
            .Properties(new BlockProperties().SearchKeywords(new string[] { "Gun","机枪"})
     //.Burnable(3f)
     //.CanBeDamaged(3)
     )

            ///Mass of the block 0.5 being equal to a double wooden block
            .Mass(0.3f)

            ///Display the collider while working on the block if you wish, then replace "true" with "false" when done looking at the colliders.
#if DEBUG
            .ShowCollider(true)
#endif
            ///Setup the collier of the block, which can consist of several different colliders.
            ///Therefore we have this CompoundCollider,
            .CompoundCollider(new List<ColliderComposite> {

                                ColliderComposite.Capsule(  0.2f,
                                                            3f,
                                                            Direction.Z,
                                                            new Vector3(0f, 0f, 1.65f),
                                                            new Vector3(0f, 0f, 0f))
              
                                
                              //,ColliderComposite.Box(new Vector3(0.35f, 0.35f, 0.15f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)).Trigger().Layer(2).IgnoreForGhost(),//   <---Example: Box Trigger on specific Layer
            })
            
            ///Make sure a block being placed on another block can intersect with it.
            .IgnoreIntersectionForBase()

            ///Load resources that will be needed for the block.
            .NeededResources(new List<NeededResource> {
                                new NeededResource(ResourceType.Audio, //Type of resource - types available are Audio, Texture, Mesh, and AssetBundle
                                                   "/MordenFirearmKitMod/MachineGun.ogg"),
                                new NeededResource(ResourceType.Mesh,
                                                   "/MordenFirearmKitMod/Bullet.obj")
            })

           ///Setup where on your block you can add other blocks.
           .AddingPoints(new List<AddingPoint> {
                              (AddingPoint) new BasePoint(false,true)         //The base point is unique compared to the other adding points, the two booleans represent whether you can add to the base, and whether it sticks automatically.
                                               .Motionable(true,false,false) //Set each of these booleans to "true" to Let the block rotate around X, Y, Z accordingly
                                               .SetStickyRadius(0.5f),        //Set the radius of which the base point will connect to others
           //                  //new AddingPoint(new Vector3(0f, 0f, 0.5f), new Vector3(-90f, 0f, 0f),true).SetStickyRadius(0.3f), <---Example: Top sticky adding point
           })
           ;

    }


    public class GunScript: BlockScript
    {

        //功能页菜单
        MMenu PageMenu;

        #region 基本组件及相关变量

        //开火
        MKey Fire;

        //武器类型菜单
        MMenu CaliberMenu;

        //威力
        MSlider StrengthSlider;
            
        //射速滑条
        MSlider FireRateSlider;

        //载弹量滑条
        MSlider BulletLimitSlider;

        //武器类型
        public enum caliber { 机枪, 机炮, 速射炮 }

        #endregion



        #region 私有变量
        GameObject Bullet;


        #endregion

        #region 功能组件


        #endregion

        public override void SafeAwake()
        {
            Fire = AddKey("发射", "Launch", KeyCode.M);

            CaliberMenu = AddMenu("Caliber", 0, new List<string>() {caliber.机枪.ToString(),caliber.机炮.ToString(),caliber.速射炮.ToString() });

            StrengthSlider = AddSlider("威力", "Strength", 1f, 0f, 2f);

            FireRateSlider = AddSlider("射速", "FireRate", 0.05f, 0f, 0.5f);

            BulletLimitSlider = AddSlider("载弹量", "BulletLimit", 50f, 0f, 500f);

        }



        protected virtual IEnumerator UpdateMapper()
        {
            if (BlockMapper.CurrentInstance == null)
                yield break;
            while (Input.GetMouseButton(0))
                yield return null;
            BlockMapper.CurrentInstance.Copy();
            BlockMapper.CurrentInstance.Paste();
            yield break;
        }

        public override void OnSave(XDataHolder stream)
        {
            base.OnSave(stream);
            SaveMapperValues(stream);
        }

        public override void OnLoad(XDataHolder stream)
        {
            base.OnLoad(stream);
            LoadMapperValues(stream);
        }


        protected override void OnSimulateStart()
        {
          
            if (CaliberMenu.Value == (int)caliber.机枪)
            {
                MGLinit();
            }



        }

        private void MGLinit()
        {
            MGLauncher MGL = gameObject.AddComponent<MGLauncher>();
            MGL.gunAudioClip = resources["/MordenFirearmKitMod/MachineGun.ogg"].audioClip;
            MGL.gunParticleTexture = resources["/MordenFirearmKitMod/RocketSmoke.png"].texture;
            MGL.Trigger = Fire;         
            MGL.FireRate = FireRateSlider.Value;
            MGL.KnockBack = StrengthSlider.Value * 0.2f;
            MGL.bulletLimit = (int)BulletLimitSlider.Value;

            #region 构造机枪子弹

            MGL.Bullet = Bullet = new GameObject("子弹");Bullet.SetActive(false);
            Bullet.transform.localScale = Vector3.one * 0.15f * Mathf.Min(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            Bullet.AddComponent<MeshCollider>().sharedMesh = Bullet.AddComponent<MeshFilter>().mesh = resources["/MordenFirearmKitMod/Bullet.obj"].mesh;
            Bullet.AddComponent<MeshRenderer>().material.color = Color.gray;
            //Bullet.AddComponent<Rigidbody>();
            //Bullet.AddComponent<DestroyIfEditMode>();
            Bullet.AddComponent<BulletScript>().Strength = StrengthSlider.Value;
            //Bullet.AddComponent<SphereCollider>().transform.localScale = Bullet.transform.localScale * 0.25f;
            MeshCollider mc =  Bullet.GetComponent<MeshCollider>();
            mc.convex = true;mc.transform.localScale = Bullet.transform.localScale * 0.25f;
            #endregion

            //MGL.Bullet = Bullet;

        }
            
    }

    //机枪发射器
    public class MGLauncher : LauncherScript
    {
        
        //通用组件
        GameObject GenericObject;

        //亮光组件
        Light gunLight;

        //音频组件
        AudioSource gunAudio;

        //粒子组件
        ParticleSystem gunParticles;

        //加特林转速
        float RotationRate;

        //加特林转速限制
        float RotationRateLimit = 60;

        //音频剪辑数据
        internal AudioClip gunAudioClip;

        //贴图数据
        internal Texture gunParticleTexture;

        //机枪可视组件
        GameObject GunVis;

        public override void Awake()
        {
            base.Awake();

            RotationRate = 0f;

            GunPoint = new Vector3(0, 0.1f, 3.5f);

            GenericObject = new GameObject();
            GenericObject.transform.parent = transform;
            GenericObject.transform.localPosition = GunPoint;
            GenericObject.transform.localEulerAngles = Vector3.zero;

            gunLight = GenericObject.AddComponent<Light>();
            gunAudio = GenericObject.AddComponent<AudioSource>();
            gunParticles = GenericObject.AddComponent<ParticleSystem>();

            CJ.angularXMotion = ConfigurableJointMotion.Locked;

           

            foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
            {
                if (mf.name == "Vis")
                {
                    GunVis = mf.gameObject; break;
                }
            }

        }

        public override void Start()
        {
            base.Start();

            gunLight.range = 10;
            gunLight.type = LightType.Spot;
            gunLight.spotAngle = 135;
            gunLight.color = new Color32(250, 135, 0, 255);
            gunLight.intensity = 100f;
            //gunLight.shadows = LightShadows.Hard;
            gunLight.enabled = false;

            gunAudio.clip = gunAudioClip;
            gunAudio.playOnAwake = false;
            gunAudio.loop = true;
            //gunAudio.enabled = true;

            gunParticles.playOnAwake = false;
            gunParticles.Stop();
            gunParticles.loop = false;
            gunParticles.startSize = 5;
            gunParticles.startSpeed = 4;
            gunParticles.maxParticles = 25;
            gunParticles.startLifetime = 0.1f;
            gunParticles.startColor = new Color32(250, 135, 0, 255);

            ParticleSystem.EmissionModule em = gunParticles.emission;
            em.rate = new ParticleSystem.MinMaxCurve(100);
            em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 8, 30) });
            em.enabled = true;

            ParticleSystem.ShapeModule sm = gunParticles.shape;
            sm.shapeType = ParticleSystemShapeType.Cone;
            sm.radius = 0.01f;
            sm.angle = 4.65f;
            sm.randomDirection = false;
            sm.enabled = true;

            ParticleSystem.VelocityOverLifetimeModule volm = gunParticles.velocityOverLifetime;
            volm.z = 2;
            volm.space = ParticleSystemSimulationSpace.Local;
            volm.enabled = true;

            ParticleSystem.ColorOverLifetimeModule colm = gunParticles.colorOverLifetime;
            colm.color = new Gradient()
            {
                alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(255, gunParticles.startLifetime * 0.65f), new GradientAlphaKey(70, gunParticles.startLifetime) },

                colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, gunParticles.startLifetime * 0.35f), new GradientColorKey(new Color32(250, 135, 0, 255), gunParticles.startLifetime * 0.65f), new GradientColorKey(new Color(0, 0, 0), gunParticles.startLifetime) }
            };
            colm.enabled = true;

            ParticleSystem.SizeOverLifetimeModule solm = gunParticles.sizeOverLifetime;
            solm.separateAxes = false;
            solm.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe[] { new Keyframe(0, 0.15f), new Keyframe(0.17f, 0.9f), new Keyframe(0.25f, 0.8f), new Keyframe(1, 0) }));
            solm.enabled = true;

            ParticleSystemRenderer psr = gunParticles.GetComponent<ParticleSystemRenderer>();
            psr.renderMode = ParticleSystemRenderMode.Billboard;
            psr.normalDirection = 1;
            psr.material = new Material(Shader.Find("Particles/Additive"));
            psr.material.mainTexture = gunParticleTexture;
            psr.sortMode = ParticleSystemSortMode.None;
            psr.sortingFudge = 0;
            psr.minParticleSize = 0;
            psr.maxParticleSize = 1;
            psr.alignment = ParticleSystemRenderSpace.View;
            psr.pivot = Vector3.zero;
            psr.motionVectors = true;
            psr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            psr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
        }

        public override void Update()
        {

            if (CJ == null)
            {
                return;
            }

            if (RotationRate != 0)
            {
                gunAudio.volume = RotationRate / (Vector3.Distance(transform.position, Camera.main.transform.position) * RotationRateLimit);
                gunAudio.pitch = RotationRate / RotationRateLimit;
                gunAudio.Play();
            }
            else
            {
                gunAudio.Stop();
            }

            if (Trigger.IsDown && bulletNumber > 0)
            {
                RotationRate = Mathf.MoveTowards(RotationRate , RotationRateLimit, 1 * Time.timeScale);
                if (RotationRate == RotationRateLimit)
                {
                    shootable = true;
                }
                else
                {
                    shootable = false;
                }
            }
            else
            {
                shootable = false;
                RotationRate = Mathf.MoveTowards(RotationRate, 0, Time.deltaTime * 10);
            }

            GunVis.transform.Rotate(new Vector3(0 , RotationRate, 0) * Time.timeScale);
            gunLight.enabled = false;
            gunParticles.Stop();

            base.Update();
        }

        public override void shoot()
        {
            base.shoot();
            gunLight.enabled = true;
            gunParticles.Play();

        }

    }


    ////发射器类
    //public class LauncherScript : MonoBehaviour
    //{


    //    //子弹
    //    //public Bullet bullet;

    //    //散布
    //    //public float Diffuse;

    //    //弹药量上限
    //    public int bulletLimit { get; set; }

    //    //实际弹药量
    //    public int bulletNumber { get; private set; }

    //    //射速
    //    public float FireRate;

    //    //后座力
    //    public float KnockBack = 1;

    //    //扳机
    //    public MKey Trigger;

    //    //质量
    //    //public float Mass = 0.5f;

    //    ///<summary>子弹组件</summary>
    //    public GameObject Bullet;

    //    //子弹网格
    //    //public Mesh bulletMesh;

    //    //发射时间间隔
    //    internal float timer;

    //    //允许发射
    //    public bool shootable = false;

    //    //随机延时
    //    //public float randomDelay = 0.1f;

    //    //枪口位置
    //    public Vector3 GunPoint;

    //    //枪的刚体组件
    //    public Rigidbody rigidbody;

    //    //枪的关节组件
    //    public ConfigurableJoint CJ;

    //    public GameObject GunVis;

    //    public virtual void Awake()
    //    {
    //        foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
    //        {
                
    //            if (mf.name == "Vis")
    //            {
    //                GunVis = mf.gameObject;break;
    //            }
    //        }

    //        rigidbody = GetComponent<Rigidbody>();
    //        rigidbody.mass = 0.5f;

    //        CJ = GetComponent<ConfigurableJoint>();
    //    }


    //    public virtual void Start()
    //    {
    //        bulletNumber = bulletLimit;
    //    }

    //    public virtual void Update()
    //    {

    //        if (StatMaster.GodTools.InfiniteAmmoMode)
    //        {
    //            bulletNumber = bulletLimit;
    //        }

    //        if (Trigger.IsDown && bulletNumber > 0 && shootable)
    //        {

    //            if (timer >= FireRate && Time.timeScale != 0)
    //            {
    //                timer = 0;
    //                shoot();

    //                return;
    //            }
    //            else
    //            {
    //                timer += Time.deltaTime;
    //            }
    //        }
            
    //    }

    //    public virtual void shoot()
    //    {

    //        bulletNumber--;          

    //        rigidbody.AddForce(-transform.forward * KnockBack * 2000f);

    //        //Bullet.GetComponent<BulletScript>().Velocity = rigidbody.velocity * ;

    //        Instantiate(Bullet,transform.TransformPoint(GunPoint),transform.rotation);

    //    }

    //}


    ////子弹类
    //public class BulletScript : MonoBehaviour
    //{

    //    #region 物理参数

    //    /// <summary>威力</summary>
    //    public float Force;

    //    //口径
    //    public float Caliber;

    //    //后坐力
    //    public float Recoil { get; private set; }

    //    //射程
    //    public float Distance { get; }

    //    /// <summary>动能</summary>
    //    public float KineticEnergy;

    //    //初速
    //    public float MuzzleVelocity { get; }

    //    ////阻力
    //    //public float Drag { get; }

    //    //质量
    //    public float Mass { get; }

    //    #endregion


    //    #region 属性变量

    //    //类型
    //    public BulletType bulletType;

    //    //子弹种类
    //    public enum BulletType { 高爆弹, 拽光弹, 穿甲弹 }

    //    //public GameObject bullet;

    //    public Rigidbody rigidbody;

    //    public Mesh mesh;

    //    public Texture texture;

    //    public Vector3 GunPoint;

    //    private MeshFilter mFilter;

    //    private MeshRenderer mRenderer;

    //    #endregion

    //    private void Awake()
    //    {
    //        rigidbody = gameObject.AddComponent<Rigidbody>();
    //        mFilter = gameObject.AddComponent<MeshFilter>();
    //        mRenderer = gameObject.AddComponent<MeshRenderer>();
    //        gameObject.AddComponent<DestroyIfEditMode>();

    //    }

    //    private void Start()
    //    {

    //        bullet_init();

    //    }

    //    private void Update()
    //    {
    //        rigidbody.AddRelativeForce(new Vector3(0,0,1) * 1000);
    //    }



    //    public void bullet_init()
    //    {

    //        mFilter.mesh = mesh;

    //        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

    //        GameObject collider = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        collider.transform.parent = gameObject.transform;
    //        collider.transform.position = gameObject.transform.position;
    //        collider.transform.localEulerAngles = new Vector3(90,0,0);
    //        transform.localScale = collider.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);


    //        //gameObject.transform.position = GunPoint;

    //    }

    //    public void bullet_init(Vector3 gunPoint,Vector3 rotation)
    //    {

    //        mFilter.mesh = mesh;

    //        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

    //        GameObject collider = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    //        collider.transform.parent = transform;


    //        gameObject.transform.localScale = collider.transform.localScale = new Vector3(0.25f, 0.35f, 0.25f);
    //        gameObject.transform.position = gunPoint;
    //        gameObject.transform.Rotate(rotation);
    //    }


    //    public void bullet_Destroy()
    //    {
    //        Destroy(gameObject);
    //    }

    //    private void OnCollisionEnter(Collision collision)
    //    {

    //        if (collision.gameObject.name != "MachineGun")
    //        {
    //            //StartCoroutine(Rocket_Explodey(transform.position));
    //        }


    //    }

    //    //爆炸事件
    //    public IEnumerator Rocket_Explodey(Vector3 point)
    //    {

    //        yield return new WaitForFixedUpdate();

    //        //爆炸范围
    //        float radius = 5;

    //        //爆炸位置
    //        Vector3 position_hit = point;


    //        GameObject explo = (GameObject)GameObject.Instantiate(PrefabMaster.BlockPrefabs[54].gameObject, position_hit, transform.rotation);
    //        explo.transform.localScale = Vector3.one * 0.01f;
    //        ControllableBomb ac = explo.GetComponent<ControllableBomb>();
    //        ac.radius = 2 + radius;
    //        ac.power = 30f * radius;
    //        ac.randomDelay = 0.00001f;
    //        ac.upPower = 0f;
    //        ac.StartCoroutine_Auto(ac.Explode());
    //        explo.AddComponent<TimedSelfDestruct>();
    //        explo.transform.localScale = Vector3.one * 0.1f;
    //        Destroy(gameObject);


    //    }

    //    public float getKineticEnergy()
    //    {
    //        return 0.5f * rigidbody.mass * rigidbody.velocity.sqrMagnitude;
    //    }


    //    public float getBuzzleVelocity(float force)
    //    {
    //        return Mathf.Sqrt(2 * force / rigidbody.mass);
    //    }

    //    public float getMass(float caliber)
    //    {
    //        return 0.5f * Mathf.Sqrt(caliber);
    //    }



    //}


    //public class BulletScript : MonoBehaviour
    //{

    //    public float Strength;

    //    private void Start()
    //    {
    //        GetComponent<Rigidbody>().velocity =  transform.forward * 300 * Strength ;
    //    }

    //}
   
}
