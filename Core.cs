using BepInEx;
using BepInEx.Configuration;
using EFT;
using EFT.Hideout;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.SessionEnd;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Windows;
namespace LogoReplace
{
    [BepInPlugin("com.eft.logoreplace", "LogoReplace", "1.0.0")]
    public class LogoReplacePlugin : BaseUnityPlugin
    {
        // UI 组件引用
        public static string dllPath = Assembly.GetExecutingAssembly().Location;
        public static string pluginDir = Path.GetDirectoryName(dllPath);
        public static Texture2D VanillaPvPLogoTexture = null;
        public static Texture2D VanillaPvELogoTexture = null;
        public static Texture2D LogoTexture = null;
        public static Texture2D VanillaBGTexture1 = null;
        public static Texture2D VanillaBGTexture2 = null;
        public static Texture2D VanillaBGTexture3 = null;
        public static Texture2D VanillaBGTexture4 = null;
        public static Texture2D BGTexture1 = null;
        public static Texture2D BGTexture2 = null;
        public static Texture2D BGTexture3 = null;
        public static Texture BGTexture4 = null;
        public static Texture2D GirlPvELogoTexture = LoadFromFile("logo_pve.png");
        public static Texture2D GirlPvPLogoTexture = LoadFromFile("logo_pvp.png");
        public static Texture2D Story01LogoTexture = LoadFromFile("logo_story_1.png");
        public static Texture2D Story01BG1 = LoadFromFile("Ending_1_part4.png");
        public static Texture2D Story01BG2 = LoadFromFile("Ending_1_part1.png");
        public static Texture2D Story01BG3 = LoadFromFile("Ending_1_part2.png");
        public static Texture2D Story01BG4 = LoadFromFile("Ending_1_part3.png");
        public static Texture2D Story02LogoTexture = LoadFromFile("logo_story_2.png");
        public static Texture2D Story02BG1 = LoadFromFile("Ending_2_part4.png");
        public static Texture2D Story02BG2 = LoadFromFile("Ending_2_part1.png");
        public static Texture2D Story02BG3 = LoadFromFile("Ending_2_part2.png");
        public static Texture2D Story02BG4 = LoadFromFile("Ending_2_part3.png");
        public static Texture2D Story03LogoTexture = LoadFromFile("logo_story_3.png");
        public static Texture2D Story03BG1 = LoadFromFile("Ending_3_part4.png");
        public static Texture2D Story03BG2 = LoadFromFile("Ending_3_part1.png");
        public static Texture2D Story03BG3 = LoadFromFile("Ending_3_part2.png");
        public static Texture2D Story03BG4 = LoadFromFile("Ending_3_part3.png");
        public static Texture2D Story04LogoTexture = LoadFromFile("logo_story_4.png");
        public static Texture2D Story04BG1 = LoadFromFile("Ending_4_part4.png");
        public static Texture2D Story04BG2 = LoadFromFile("Ending_4_part1.png");
        public static Texture2D Story04BG3 = LoadFromFile("Ending_4_part2.png");
        public static Texture2D Story04BG4 = LoadFromFile("Ending_4_part3.png");
        public static Texture2D MCLogoTexture = LoadFromFile("logo_minecraft.png");
        public static Texture2D MCBG1 = LoadFromFile("Minecraft_part4.png");
        public static Texture2D MCBG2 = LoadFromFile("Minecraft_part1.png");
        public static Texture2D MCBG3 = LoadFromFile("Minecraft_part2.png");
        public static Texture2D MCBG4 = LoadFromFile("Minecraft_part3.png");
        private void Awake()
        {
            var harmony = new Harmony("com.eft.logoreplace");
            harmony.PatchAll();
            UsePVPLogo = base.Config.Bind<bool>(
                "设置",
                "修改主菜单为PVP样式",
                true,
                new ConfigDescription(
                    "是否将主菜单Logo切换到PVP样式"
                )
            );
            ChangeLogo = base.Config.Bind<bool>(
                "设置",
                "启用少女版主菜单Logo",
                true,
                new ConfigDescription(
                    "是否启用少女版主菜单Logo"
                )
            );
            DisableTopGlow = base.Config.Bind<bool>(
                "设置",
                "禁用顶部打光",
                true,
                new ConfigDescription(
                    "禁用顶部打光"
                )
            );
            BackgroundSelect = Config.Bind(
                "设置",
                "背景样式选择",
                "森林",
                new ConfigDescription(
                    "选择背景样式",
                    new AcceptableValueList<string>(
                        "森林", "黯淡天际", "人类黄昏", "风暴前沿", "风轻云淡", "方块世界"
                    )
                )
            );
            //一切就绪, 该做材质了
            //订阅事件处理Logo变化
            UsePVPLogo.SettingChanged += ChangeLogoTypeEvent;
            UsePVPLogo.SettingChanged += ChangeLogoStyleEvent;
            UsePVPLogo.SettingChanged += ChangeLogoEvent;
            ChangeLogo.SettingChanged += ChangeLogoTypeEvent;
            ChangeLogo.SettingChanged += ChangeLogoEvent;
            DisableTopGlow.SettingChanged += ChangeLogoStyleEvent;
            BackgroundSelect.SettingChanged += ChangeBackgroundEvent;
            BackgroundSelect.SettingChanged += ChangeLogoEvent;
        }
        //Logo外观变化事件
        public static void ChangeLogoTypeEvent(object sender, EventArgs e)
        {
            ChangeLogoTexture();
        }
        //刷新Logo材质
        public static void RefreshLogoTexture()
        {
            if (_meshRendererPvE == null || _meshRendererPvP == null) return;
            _meshRendererPvE.sharedMaterial.mainTexture = LogoTexture;
        }
        //Logo材质变化事件
        public static void ChangeLogoEvent(object sender, EventArgs e)
        {
            RefreshLogoTexture();
        }
        public static void ChangeLogoTexture()
        {
            var changelogo = ChangeLogo.Value;
            var usepvplogo = UsePVPLogo.Value;
            if (bgIsWood)
            {
                LogoTexture = usepvplogo ? changelogo ? GirlPvPLogoTexture : VanillaPvPLogoTexture : changelogo ? GirlPvELogoTexture : VanillaPvELogoTexture;
            }
        }
        //刷新Logo状态
        public static void RefreshLogoState()
        {
            if (_logoPvE == null || _logoPvP == null || _topGlowPvE == null || _topGlowPvP == null) return;

            bool usePvpLogo = UsePVPLogo.Value;
            bool disableGlow = DisableTopGlow.Value;

            _topGlowPvE.SetActive(!disableGlow && !usePvpLogo);
            _topGlowPvP.SetActive(!disableGlow && usePvpLogo);
        }
        //Logo类型变化事件
        public static void ChangeLogoStyleEvent(object sender, EventArgs e)
        {
            RefreshLogoState();
        }
        public static void ChangeBackgroundEvent(object sender, EventArgs e)
        {
            ChangeBackground();
            //森林坏了, 怪事
            //以及初始logo依然有问题并且会导致订阅事件出错
            //color不对? 不可能啊
            //是不是Standard无法识别Emi?
            //森林肯定在哪里出问题了
        }
        public static void ChangeBackground()
        {
            var bgSelect = BackgroundSelect.Value;
            bgIsWood = false;
            switch (bgSelect)
            {
                case "森林":
                    {
                        SetWoodBackground();
                    }
                    break;
                case "黯淡天际":
                    {
                        SetStory01Background();
                    }
                    break;
                case "人类黄昏":
                    {
                        SetStory02Background();
                    }
                    break;
                case "风暴前沿":
                    {
                        SetStory03Background();
                    }
                    break;
                case "风轻云淡":
                    {
                        SetStory04Background();
                    }
                    break;
                case "方块世界":
                    {
                        SetMCBackground();
                    }
                    break;
                default:
                    {
                        SetWoodBackground();
                    }
                    break;
            }
        }
        //默认的森林背景
        public static void SetWoodBackground()
        {
            bgIsWood = true;
            if (!NullCheck()) return;
            //材质
            SetBGTexture(VanillaBGTexture1, VanillaBGTexture2, VanillaBGTexture3, VanillaBGTexture4);
            SetBasicBGData();
            _pointLightTransform.position = (Vector3)_pointLightPosition;
            _pointLight.intensity = _pointLightIntensity;
            _pointLight.range = _pointLightRange;
            //不对不对, 这里思路不对....
            //对吗?
            //陷入沉思
            //对的对的
            ChangeLogoTexture();
        }
        //风轻云淡(救世主
        public static void SetStory04Background()
        {
            if (!NullCheck()) return;
            SetBGTexture(Story04BG1, Story04BG2, Story04BG3, Story04BG4);
            SetBasicBGData();
            SetLight();
            LogoTexture = Story04LogoTexture;
            if (_bg.GetComponent<AnimationBG>() == null)
            {
                _bg.AddComponent<AnimationBG>();
            }
            //_videoPlayer.source = VideoSource.Url;
            //_videoPlayer.url = Path.Combine(pluginDir, "test.mp4");

            //_videoPlayer.renderMode = VideoRenderMode.APIOnly;

            //_videoPlayer.isLooping = true;
            //_videoPlayer.targetTexture = _renderTexture;
            //_bgMaterial[1].mainTexture = _renderTexture;
            //SetBGTexture(Story04BG1, Story04BG2, Story04BG3);
        }
        //人类黄昏(堕入黑暗
        public static void SetStory02Background()
        {
            if (!NullCheck()) return;
            SetBGTexture(Story02BG1, Story02BG2, Story02BG3, Story02BG4);
            SetBasicBGData();
            SetLight();
            LogoTexture = Story02LogoTexture;
        }
        //风暴前沿(灯塔
        public static void SetStory03Background()
        {
            if (!NullCheck()) return;
            SetBGTexture(Story03BG1, Story03BG2, Story03BG3, Story03BG4);
            SetBasicBGData();
            SetLight();
            LogoTexture = Story03LogoTexture;
        }
        //黯淡天际(幸存
        public static void SetStory01Background()
        {
            if (!NullCheck()) return;
            SetBGTexture(Story01BG1, Story01BG2, Story01BG3, Story01BG4);
            SetBasicBGData();
            SetLight();
            LogoTexture = Story01LogoTexture;
        }
        //MC
        public static void SetMCBackground()
        {
            if (!NullCheck()) return;
            SetBGTexture(MCBG1, MCBG2, MCBG3, MCBG4);
            SetBasicBGData();
            SetLight();
            LogoTexture = MCLogoTexture;
        }
        //打光不确定需不需要微调, 暂且打包
        public static void SetLight()
        {
            _pointLightTransform.position = new Vector3(-0.1953f, -998.6731f, 0.8506f);
            _pointLight.intensity = 3f;
            _pointLight.range = 9f;
        }
        //继续优化调用结构, 基础数据设置
        public static void SetBasicBGData()
        {
            //背景色
            SetBGColor();
            //变换
            SetBGTransform();
            //挂件
            SetPendant();
            //后期特效
            SetAfterEffect();
        }
        //背景变换
        public static void SetBGTransform()
        {
            //我知道了, 这里可能NPE了
            //果然
            //我讨厌隐式转换
            _bgTransform.localEulerAngles = bgIsWood ? (Vector3)_bgEulerAngle : new Vector3(270f, 225f, 0f);
            _bgTransform.localPosition = bgIsWood ? (Vector3)_bgPosition : new Vector3(0f, -4.85f, 0f);
            _bgTransform.localScale = bgIsWood ? (Vector3)_bgScale : new Vector3(5.9f, 5.4f, 18.3f);
        }
        //空值检查, 防止默认非森林背景
        public static bool NullCheck()
        {
            if (_bgMaterial == null || _bgTransform == null || _bloom == null || _branch == null || _pointLightPosition == null) return false;
            return true;
        }
        //设置背景纹理
        public static void SetBGTexture(Texture2D tex1, Texture2D tex2, Texture2D tex3, Texture2D tex4)
        {
            BGTexture1 = tex1;
            BGTexture2 = tex2;
            BGTexture3 = tex3;
            BGTexture4 = tex4;
            _bgMaterial[0].SetTexture("_EmissionMap", BGTexture1);
            _bgMaterial[1].SetTexture("_EmissionMap", BGTexture4);
            _bgMaterial[2].SetTexture("_EmissionMap", BGTexture3);
            _bgMaterial[3].SetTexture("_EmissionMap", BGTexture2);
            _bgMaterial[0].mainTexture = BGTexture1;
            _bgMaterial[1].mainTexture = BGTexture4;
            _bgMaterial[2].mainTexture = BGTexture3;
            _bgMaterial[3].mainTexture = BGTexture2;
            //需要处理MainTex
        }
        public static void SetBGTexture(Texture2D tex1, Texture2D tex2, Texture2D tex3)
        {
            BGTexture1 = tex1;
            BGTexture2 = tex2;
            BGTexture3 = tex3;
            _bgMaterial[0].SetTexture("_EmissionMap", BGTexture1);
            _bgMaterial[2].SetTexture("_EmissionMap", BGTexture3);
            _bgMaterial[3].SetTexture("_EmissionMap", BGTexture2);
            _bgMaterial[0].mainTexture = BGTexture1;
            _bgMaterial[2].mainTexture = BGTexture3;
            _bgMaterial[3].mainTexture = BGTexture2;
            //需要处理MainTex
        }
        //设置背景颜色
        public static void SetBGColor()
        {
            foreach (var mat in _bgMaterial)
            {
                mat.color = bgIsWood ? Color.black : Color.white;
            }
        }
        //开关后期特效
        public static void SetAfterEffect()
        {
            _bloom.enabled = bgIsWood;
            _bloomAndFlare.enabled = bgIsWood;
            _colorCorrection.enabled = bgIsWood;
            _prismEffect.enabled = bgIsWood;
        }
        //开关挂件
        public static void SetPendant()
        {
            _branch.SetActive(bgIsWood);
            _christmaBall.SetActive(bgIsWood);
        }
        //核心方法, 从流载入Tex2D
        public static Texture2D LoadFromFile(string path, int width = 2, int height = 2)
        {
            string pathes = Path.Combine(pluginDir, path);
            if (!File.Exists(pathes))
            {
                Debug.LogError($"File not found: {pathes}");
                return null;
            }
            byte[] bytes = File.ReadAllBytes(pathes);
            var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.LoadImage(bytes);
            return tex;
        }
        //尝试一下从流创建VideoPlayer
        public static VideoPlayer LoadVideoFromFile(string path, int width = 1920, int height = 1080)
        {
            string pathes = Path.Combine(pluginDir, path);
            //这里应该是MonoBehaviour
            //昨晚没太睡好, 有点注意力涣散
            //明天再写
            return null;
        }
        public class AnimationBG : MonoBehaviour
        {
            //....不太好搞
            //总之先试试固定硬编码
            public string filePath = "test.mp4"; // 视频文件路径
            public MeshRenderer targetRenderer; // 用于显示视频的 MeshRenderer

            private VideoPlayer videoPlayer;
            private RenderTexture renderTexture;
            void Start()
            {
                // 创建并设置 RenderTexture
                renderTexture = new RenderTexture(2048, 2048, 16); // 根据视频分辨率设置大小
                renderTexture.Create();

                string pathes = Path.Combine(pluginDir, filePath);

                targetRenderer = this.gameObject.GetComponent<MeshRenderer>();

                // 获取 VideoPlayer 组件
                videoPlayer = gameObject.AddComponent<VideoPlayer>();

                // 设置 VideoPlayer 的视频源为本地文件路径
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = pathes;  // 使用本地视频文件路径

                videoPlayer.renderMode = VideoRenderMode.APIOnly;

                // 将视频渲染到 RenderTexture 上
                videoPlayer.targetTexture = renderTexture;

                // 设置 VideoPlayer 自动循环播放
                videoPlayer.isLooping = true;

                videoPlayer.Play();
                Console.WriteLine("LogoReplace: VidepPlay");
                // 等待 VideoPlayer 准备好后开始播放

                // 将 RenderTexture 设置为 MeshRenderer 的第二个材质（materials[1]）
                if (targetRenderer != null)
                {
                    Console.WriteLine("LogoReplace: SetVideo");
                    _bgMaterial[1].mainTexture = videoPlayer.targetTexture;
                    _bgMaterial[1].SetTexture("_EmissionMap", videoPlayer.targetTexture);
                    //targetRenderer.sharedMaterials[1].SetTexture("_EmissionMap", renderTexture);
                }
                else
                {
                    Console.WriteLine("LogoReplace: SetVideo Failed");
                }
            }
        }
        [HarmonyPatch(typeof(EnvironmentUI), nameof(EnvironmentUI.ShowEnvironment))]
        static class Patch_ShowEnvironment
        {
            static void Postfix(EnvironmentUI __instance, bool value)
            {
                var changeLogo = ChangeLogo.Value;
                var disableTopGlow = DisableTopGlow.Value;
                var usePvPLogo = UsePVPLogo.Value;
                //缓存实例(这玩意好像没用了....
                //就是没用了
                //_environmentUIInstance = __instance;
                try
                {
                    //PvE处理
                    var decal_pve = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/logo_decal/decal_plane_pve");
                    if (decal_pve == null)
                    {
                        Debug.LogWarning("LogoGO 未找到");
                        return;
                    }
                    _logoPvE = decal_pve.gameObject;
                    var mr_pve = decal_pve.GetComponent<MeshRenderer>();
                    if (mr_pve != null && mr_pve.sharedMaterial != null)
                    {
                        _meshRendererPvE = mr_pve;
                        if (VanillaPvELogoTexture == null)
                        {
                            VanillaPvELogoTexture = (Texture2D)mr_pve.sharedMaterial.mainTexture;
                        }
                        mr_pve.sharedMaterial.mainTexture = LogoTexture;
                        Debug.Log("MeshRenderer logo Texture 已替换");
                    }
                    else
                    {
                        Debug.LogWarning("MeshRenderer 未找到或 Material 为空");
                    }
                    //PvP处理
                    //logo部分需要完全重做, 放弃PvElogo, 直接转换材质
                    var decal_pvp = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/logo_decal/decal_plane");
                    if (decal_pvp == null)
                    {
                        Debug.LogWarning("LogoGO 未找到");
                        return;
                    }
                    _logoPvP = decal_pvp.gameObject;
                    var mr_pvp = decal_pvp.GetComponent<MeshRenderer>();
                    if (mr_pvp != null && mr_pvp.sharedMaterial != null)
                    {
                        _meshRendererPvP = mr_pvp;
                        if (VanillaPvPLogoTexture == null)
                        {
                            VanillaPvPLogoTexture = (Texture2D)mr_pvp.sharedMaterial.mainTexture;
                        }
                        //mr_pvp.sharedMaterial.mainTexture = LogoTexture;
                        Debug.Log("MeshRenderer logo Texture 已替换");
                    }
                    else
                    {
                        Debug.LogWarning("MeshRenderer 未找到或 Material 为空");
                    }
                    ChangeLogoTexture();
                    //初始化Logo切换
                    //decal_pve.gameObject.SetActive(!usePvPLogo);
                    //decal_pvp.gameObject.SetActive(usePvPLogo);
                    //查找顶部打光
                    var topglowpve = __instance.transform.Find("Common/Glow Canvas/TopGlowPve");
                    var topglowpvp = __instance.transform.Find("Common/Glow Canvas/TopGlowRegular");
                    if (topglowpve != null)
                    {
                        _topGlowPvE = topglowpve.gameObject;
                        topglowpve.gameObject.SetActive(!usePvPLogo && disableTopGlow);
                    }
                    if (topglowpvp != null)
                    {
                        _topGlowPvP = topglowpvp.gameObject;
                        topglowpvp.gameObject.SetActive(usePvPLogo && disableTopGlow);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"替换 logo 时出错：{e}");
                }
                //处理主菜单背景
                try
                {
                    var selectBG = BackgroundSelect.Value;
                    //主菜单全景环
                    var panorama = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/panorama");
                    if (panorama == null)
                    {
                        Console.WriteLine("BGGO 未找到");
                        return;
                    }
                    _bg = panorama.gameObject;
                    var bggo = panorama.GetComponent<MeshRenderer>();
                    var bggotm = panorama.GetComponent<Transform>();
                    _bgTransform = bggotm;
                    if (bggo != null && bggo.sharedMaterials != null)
                    {
                        //建立引用
                        _bgMaterial = bggo.sharedMaterials;
                        //备份原始变换数据
                        if (_bgEulerAngle == null)
                        {
                            _bgEulerAngle = bggotm.localEulerAngles;
                            _bgPosition = bggotm.localPosition;
                            _bgScale = bggotm.localScale;
                        }
                        if (VanillaBGTexture1 == null)
                        {
                            VanillaBGTexture1 = (Texture2D)bggo.sharedMaterials[0].GetTexture("_EmissionMap");
                            VanillaBGTexture4 = (Texture2D)bggo.sharedMaterials[1].GetTexture("_EmissionMap");
                            VanillaBGTexture3 = (Texture2D)bggo.sharedMaterials[2].GetTexture("_EmissionMap");
                            VanillaBGTexture2 = (Texture2D)bggo.sharedMaterials[3].GetTexture("_EmissionMap");
                        }
                        foreach (var mat in _bgMaterial)
                        {
                            mat.shader = Shader.Find("Standard");
                        }
                        Console.WriteLine("MeshRenderer BG Texture 已替换");
                    }
                    else
                    {
                        Console.WriteLine("MeshRenderer 未找到或 Material 为空");
                    }
                    //后期处理
                    var camera = __instance.transform.Find("EnvironmentUISceneWood/WoodsCameraContainer/MainMenuCamera");
                    if (camera != null)
                    {
                        var bloom = camera.GetComponent("UnityStandardAssets.ImageEffects.Bloom") as MonoBehaviour;
                        var bloom2 = camera.GetComponent("UnityStandardAssets.ImageEffects.BloomAndFlares") as MonoBehaviour;
                        var color = camera.GetComponent("UnityStandardAssets.ImageEffects.ColorCorrectionCurves") as MonoBehaviour;
                        var prism = camera.GetComponent<PrismEffects>();
                        _bloom = bloom;
                        _bloomAndFlare = bloom2;
                        _colorCorrection = color;
                        _prismEffect = prism;
                    }
                    //树枝和圣诞球
                    var topright = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/BranchContainer/Pine_branch_animated/Pine_branch");
                    var topright2 = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/BranchContainer/Pine_branch_animated/PineHub/PineBone/PineHub004Bone001Bone001");
                    if (topright != null)
                    {
                        _branch = topright.gameObject;
                        //topright.gameObject.SetActive(false);
                    }
                    if (topright2 != null)
                    {
                        _christmaBall = topright2.gameObject;
                        //topright2.gameObject.SetActive(false);
                    }
                    //打光
                    var pointLights = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/BranchContainer")
                    .GetComponentsInChildren<Transform>(true)
                    .Where(t => t.name == "Point light")
                    .ToArray()[2];
                    //pointLights.transform.position = new Vector3(-0.1953f, -998.6731f, 0.8506f);
                    _pointLightTransform = pointLights.transform;
                    //备份变换数据
                    if (_pointLightPosition == null)
                    {
                        _pointLightPosition = pointLights.transform.position;
                    }
                    var light = pointLights.GetComponent<Light>();
                    _pointLight = light;
                    if (_pointLightIntensity == 0f)
                    {
                        _pointLightIntensity = light.intensity;
                        _pointLightRange = light.range;
                    }
                }
                catch (Exception err)
                {
                    Debug.LogError(err);
                }
                try
                {
                    //复位一次数据;
                    //树枝有问题
                    ChangeBackground();
                    RefreshLogoTexture();
                }
                catch (Exception err)
                {
                    Debug.LogError(err);
                }
            }
        }
        internal static ConfigEntry<bool> UsePVPLogo { get; set; }
        internal static ConfigEntry<bool> ChangeLogo { get; set; }
        internal static ConfigEntry<bool> DisableTopGlow { get; set; }
        internal static ConfigEntry<string> BackgroundSelect;
        public static GameObject _logoPvP;
        public static GameObject _topGlowPvP;
        public static MeshRenderer _meshRendererPvP;
        public static GameObject _logoPvE;
        public static GameObject _topGlowPvE;
        public static MeshRenderer _meshRendererPvE;
        public static GameObject _bg;
        public static VideoPlayer _videoPlayer = new VideoPlayer();
        public static RenderTexture _renderTexture = new RenderTexture(2048, 2048, 16);
        public static Transform _bgTransform;
        public static Vector3? _bgPosition = null;
        public static Vector3? _bgEulerAngle = null;
        public static Vector3? _bgScale = null;
        public static Material[] _bgMaterial;
        public static MonoBehaviour _bloom;
        public static MonoBehaviour _bloomAndFlare;
        public static MonoBehaviour _colorCorrection;
        public static MonoBehaviour _prismEffect;
        public static GameObject _branch;
        public static GameObject _christmaBall;
        public static Transform _pointLightTransform;
        public static Vector3? _pointLightPosition = null;
        public static Light _pointLight;
        public static float _pointLightIntensity = 0f;
        public static float _pointLightRange = 0f;
        public static bool bgIsWood = true;
    }
}