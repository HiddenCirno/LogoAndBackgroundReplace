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
        public static Texture2D PvELogoTexture = null;
        public static Texture2D PvPLogoTexture = null;
        public static Texture2D VanillaBGTexture1 = null;
        public static Texture2D VanillaBGTexture2 = null;
        public static Texture2D VanillaBGTexture3 = null;
        public static Texture2D VanillaBGTexture4 = null;
        public static Texture2D BGTexture1 = null;
        public static Texture2D BGTexture2 = null;
        public static Texture2D BGTexture3 = null;
        public static Texture2D BGTexture4 = null;
        public static Texture2D GirlPvELogoTexture = LoadFromFile("logo_pve.png");
        public static Texture2D GirlPvPLogoTexture = LoadFromFile("logo_pvp.png");
        public static Texture2D Story_4_LogoTexture = LoadFromFile("logo_story_4.png");
        public static Texture2D Story_4_BG1 = LoadFromFile("Ending_4_part4.png");
        public static Texture2D Story_4_BG2 = LoadFromFile("Ending_4_part1.png");
        public static Texture2D Story_4_BG3 = LoadFromFile("Ending_4_part2.png");
        public static Texture2D Story_4_BG4 = LoadFromFile("Ending_4_part3.png");
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
                    "选择图形质量预设",
                    new AcceptableValueList<string>(
                        "森林", "黯淡天际", "人类黄昏", "风暴前沿", "风轻云淡", "方块世界"
                    )
                )
            );
            switch (BackgroundSelect.Value)
            {
                case "森林":
                    {
                        PvELogoTexture = GirlPvELogoTexture;
                        PvPLogoTexture = GirlPvPLogoTexture;
                    }
                    break;
            }
            //订阅事件处理Logo变化
            UsePVPLogo.SettingChanged += ChangeLogoStyleEvent;
            ChangeLogo.SettingChanged += ChangeLogoTypeEvent;
            DisableTopGlow.SettingChanged += ChangeLogoStyleEvent;
        }
        //Logo外观变化事件
        public static void ChangeLogoTypeEvent(object sender, EventArgs e)
        {
            if (sender is ConfigEntry<bool> configEntry)
            {
                bool newValue = configEntry.Value;
                if (_meshRendererPvE == null || _meshRendererPvP == null) return;
                PvELogoTexture = newValue == true ? GirlPvELogoTexture : VanillaPvELogoTexture;
                PvPLogoTexture = newValue == true ? GirlPvPLogoTexture : VanillaPvPLogoTexture;
                _meshRendererPvE.sharedMaterial.mainTexture = PvELogoTexture;
                _meshRendererPvP.sharedMaterial.mainTexture = PvPLogoTexture;

            }
        }
        //刷新Logo状态
        public static void RefreshLogoState()
        {
            if (_logoPvE == null || _logoPvP == null || _topGlowPvE == null || _topGlowPvP == null) return;

            bool usePvpLogo = UsePVPLogo.Value;
            bool disableGlow = DisableTopGlow.Value;

            _logoPvE.SetActive(!usePvpLogo);
            _logoPvP.SetActive(usePvpLogo);
            _topGlowPvE.SetActive(!disableGlow && !usePvpLogo);
            _topGlowPvP.SetActive(!disableGlow && usePvpLogo);
        }
        //Logo类型变化事件
        public static void ChangeLogoStyleEvent(object sender, EventArgs e)
        {
            RefreshLogoState();
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
        [HarmonyPatch(typeof(EnvironmentUI), nameof(EnvironmentUI.ShowEnvironment))]
        static class Patch_ShowEnvironment
        {
            static void Postfix(EnvironmentUI __instance, bool value)
            {
                var changeLogo = ChangeLogo.Value;
                var disableTopGlow = DisableTopGlow.Value;
                var usePvPLogo = UsePVPLogo.Value;
                //缓存实例(这玩意好像没用了....
                _environmentUIInstance = __instance;
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
                        mr_pve.sharedMaterial.mainTexture = changeLogo ? PvELogoTexture : VanillaPvELogoTexture;
                        Debug.Log("MeshRenderer logo Texture 已替换");
                    }
                    else
                    {
                        Debug.LogWarning("MeshRenderer 未找到或 Material 为空");
                    }
                    //PvP处理
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
                        mr_pvp.sharedMaterial.mainTexture = changeLogo ? PvPLogoTexture : VanillaPvPLogoTexture;
                        Debug.Log("MeshRenderer logo Texture 已替换");
                    }
                    else
                    {
                        Debug.LogWarning("MeshRenderer 未找到或 Material 为空");
                    }
                    //初始化Logo切换
                    decal_pve.gameObject.SetActive(!usePvPLogo);
                    decal_pvp.gameObject.SetActive(usePvPLogo);
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
                    var bggo = panorama.GetComponent<MeshRenderer>();
                    var bggotm = panorama.GetComponent<Transform>();
                    _bgTransform = bggotm;
                    if (bggo != null && bggo.sharedMaterials != null)
                    {
                        //建立引用
                        _bgMaterial = bggo.sharedMaterials;
                        //备份原始变换数据
                        _bgEulerAngle = bggotm.localEulerAngles;
                        _bgPosition = bggotm.localPosition;
                        _bgScale = bggotm.localScale;
                        if (VanillaBGTexture1 == null)
                        {
                            VanillaBGTexture1 = (Texture2D)bggo.sharedMaterials[0].GetTexture("_EmissionMap");
                            VanillaBGTexture4 = (Texture2D)bggo.sharedMaterials[1].GetTexture("_EmissionMap");
                            VanillaBGTexture3 = (Texture2D)bggo.sharedMaterials[2].GetTexture("_EmissionMap");
                            VanillaBGTexture2 = (Texture2D)bggo.sharedMaterials[3].GetTexture("_EmissionMap");
                        }
                        switch (selectBG)
                        {
                            case "森林":
                                {
                                    foreach (var mat in bggo.sharedMaterials)
                                    {
                                        mat.shader = Shader.Find("Standard");
                                        mat.color = Color.black;
                                    }
                                }
                                break;
                            default:
                                {

                                }
                                break;
                        }
                        //bggo.sharedMaterials[0].SetTexture("_EmissionMap", BGTexture1);
                        //bggo.sharedMaterials[1].SetTexture("_EmissionMap", BGTexture4);
                        //bggo.sharedMaterials[2].SetTexture("_EmissionMap", BGTexture3);
                        //bggo.sharedMaterials[3].SetTexture("_EmissionMap", BGTexture2);
                        //bggo.sharedMaterials[0].mainTexture = BGTexture1;
                        //bggo.sharedMaterials[1].mainTexture = BGTexture4;
                        //bggo.sharedMaterials[2].mainTexture = BGTexture3;
                        //bggo.sharedMaterials[3].mainTexture = BGTexture2;
                        //foreach (var mat in bggo.sharedMaterials)
                        //{
                            //mat.shader = Shader.Find("Standard");
                            //mat.color = Color.white;
                        //}
                        //bggotm.localEulerAngles = new Vector3(270f, 225f, 0f);
                        //bggotm.localPosition = new Vector3(0f, -4.85f, 0f);
                        //bggotm.localScale = new Vector3(5.9f, 5.4f, 18.3f);
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
                    var light = pointLights.GetComponent<Light>();
                    _pointLight = light;
                    //light.intensity = 3f;
                    _pointLightIntensity = light.intensity;
                    //light.range = 9f;
                    _pointLightRange = light.range;
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
        public static EnvironmentUI _environmentUIInstance;
        public static GameObject _logoPvP;
        public static GameObject _topGlowPvP;
        public static MeshRenderer _meshRendererPvP;
        public static GameObject _logoPvE;
        public static GameObject _topGlowPvE;
        public static MeshRenderer _meshRendererPvE;
        public static Transform _bgTransform;
        public static Vector3 _bgPosition;
        public static Vector3 _bgEulerAngle;
        public static Vector3 _bgScale;
        public static Material[] _bgMaterial;
        public static MonoBehaviour _bloom;
        public static MonoBehaviour _bloomAndFlare;
        public static MonoBehaviour _colorCorrection;
        public static MonoBehaviour _prismEffect;
        public static GameObject _branch;
        public static GameObject _christmaBall;
        public static Transform _pointLightTransform;
        public static Vector3 _pointLightPosition;
        public static Light _pointLight;
        public static float _pointLightIntensity;
        public static float _pointLightRange;
        public static bool bgIsWood = true;

    }
}