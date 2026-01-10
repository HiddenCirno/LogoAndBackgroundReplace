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
        public static Texture2D VanillaPVPLogo = null;
        public static Texture2D VanillaPVELogo = null;
        public static Texture2D PVELogoTexture = LoadFromFile("logo_pve.png");
        public static Texture2D PVPLogoTexture = LoadFromFile("logo_pvp.png");
        public static Texture2D TestFloor = LoadFromFile("AndSurvivedFloor.png");
        public static Texture2D BG1 = LoadFromFile("Ending_part4_Merge.png");
        public static Texture2D BG2 = LoadFromFile("Ending_part1_Merge.png");
        public static Texture2D BG3 = LoadFromFile("Ending_part2_Merge.png");
        public static Texture2D BG4 = LoadFromFile("Ending_part3_Merge_New.png");
        public static Texture2D FG1 = LoadFromFile("Ending_part4_Front.png");
        public static Texture2D FG2 = LoadFromFile("Ending_part1_Front.png");
        public static Texture2D FG3 = LoadFromFile("Ending_part2_Front.png");
        public static Texture2D FG4 = LoadFromFile("Ending_part3_Front.png");
        public static Sprite TestFloorSprite = SimpleCreateSprite(TestFloor);
        private void Awake()
        {
            //string logoFile = Path.Combine(pluginDir, "logo_pve.png");
            //byte[] bytes = File.ReadAllBytes(logoFile);
            //var tex = new Texture2D(2, 2);
            //tex.LoadImage(bytes);
            //NewLogoTexture = tex;
            // 2. 安装 Harmony Patch
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
                "启用Logo美化",
                true,
                new ConfigDescription(
                    "是否启用Logo美化"
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
        }

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
            //tex.globalMipMapLimit = 0;
            tex.LoadImage(bytes);
            return tex;
        }
        // 3. 对 EnvironmentUI.Awake 做 Postfix
        [HarmonyPatch(typeof(EnvironmentUI), nameof(EnvironmentUI.ShowEnvironment))]
        static class Patch_ShowEnvironment
        {
            static void Postfix(EnvironmentUI __instance, bool value)
            {
                try
                {
                    // 找到 Logo 的 GameObject
                    var decal_pve = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/logo_decal/decal_plane_pve");
                    if (decal_pve == null)
                    {
                        Debug.LogWarning("LogoGO 未找到");
                        return;
                    }
                    // MeshRenderer 存在则直接替换贴图
                    var mr_pve = decal_pve.GetComponent<MeshRenderer>();
                    if (mr_pve != null && mr_pve.sharedMaterial != null)
                    {
                        if (VanillaPVELogo == null)
                        {
                            VanillaPVELogo = (Texture2D)mr_pve.sharedMaterial.mainTexture;
                        }
                        mr_pve.sharedMaterial.mainTexture = ChangeLogo.Value ? PVELogoTexture : VanillaPVELogo;
                        Debug.Log("MeshRenderer logo Texture 已替换");
                    }
                    else
                    {
                        Debug.LogWarning("MeshRenderer 未找到或 Material 为空");
                    }
                    var decal_pvp = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/logo_decal/decal_plane");
                    if (decal_pvp == null)
                    {
                        Debug.LogWarning("LogoGO 未找到");
                        return;
                    }
                    // MeshRenderer 存在则直接替换贴图
                    var mr_pvp = decal_pvp.GetComponent<MeshRenderer>();
                    if (mr_pvp != null && mr_pvp.sharedMaterial != null)
                    {
                        if (VanillaPVPLogo == null)
                        {
                            VanillaPVPLogo = (Texture2D)mr_pvp.sharedMaterial.mainTexture;
                        }
                        mr_pvp.sharedMaterial.mainTexture = ChangeLogo.Value ? PVPLogoTexture : VanillaPVPLogo;
                        Debug.Log("MeshRenderer logo Texture 已替换");
                    }
                    else
                    {
                        Debug.LogWarning("MeshRenderer 未找到或 Material 为空");
                    }
                    if (UsePVPLogo.Value)
                    {
                        decal_pve.gameObject.SetActive(false);
                        decal_pvp.gameObject.SetActive(true);
                        var topglowpve = __instance.transform.Find("Common/Glow Canvas/TopGlowPve");
                        var topglowpvp = __instance.transform.Find("Common/Glow Canvas/TopGlowRegular");
                        if (topglowpve != null)
                        {
                            topglowpve.gameObject.SetActive(false);
                        }
                        if (topglowpvp != null && !DisableTopGlow.Value)
                        {
                            topglowpvp.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        decal_pve.gameObject.SetActive(true);
                        decal_pvp.gameObject.SetActive(false);
                        var topglowpve = __instance.transform.Find("Common/Glow Canvas/TopGlowPve");
                        var topglowpvp = __instance.transform.Find("Common/Glow Canvas/TopGlowRegular");
                        if (topglowpve != null && !DisableTopGlow.Value)
                        {
                            topglowpve.gameObject.SetActive(true);
                        }
                        if (topglowpvp != null)
                        {
                            topglowpvp.gameObject.SetActive(false);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"替换 logo 时出错：{e}");
                }
                try
                {
                    var panorama = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/panorama");
                    if (panorama == null)
                    {
                        Console.WriteLine("BGGO 未找到");
                        return;
                    }
                    // MeshRenderer 存在则直接替换贴图
                    var bggo = panorama.GetComponent<MeshRenderer>();
                    var bggotm = panorama.GetComponent<Transform>();
                    if (bggo != null && bggo.sharedMaterials != null)
                    {
                        bggo.sharedMaterials[0].SetTexture("_EmissionMap", BG1);
                        bggo.sharedMaterials[1].SetTexture("_EmissionMap", BG4);
                        bggo.sharedMaterials[2].SetTexture("_EmissionMap", BG3);
                        bggo.sharedMaterials[3].SetTexture("_EmissionMap", BG2);
                        bggo.sharedMaterials[0].mainTexture = BG1;
                        bggo.sharedMaterials[1].mainTexture = BG4;
                        bggo.sharedMaterials[2].mainTexture = BG3;
                        bggo.sharedMaterials[3].mainTexture = BG2;
                        foreach (var mat in bggo.sharedMaterials)
                        {
                            //mat.mainTexture = TestBG;
                            //mat.SetTexture("_MainTex", BG4);
                            //mat.SetTexture("_EmissionMap", BG4);
                            //.SetColor("_EmissionColor", Color.white * 2f);
                            mat.shader = Shader.Find("Standard");
                            mat.color = Color.white;
                            //mat.EnableKeyword("_MAIN_TEX");
                            //mat.enabledKeywords
                        }
                        bggotm.localEulerAngles = new Vector3(270f, 225f, 0f);
                        bggotm.localPosition = new Vector3(0f, -4.85f, 0f);
                        bggotm.localScale = new Vector3(5.9f, 5.4f, 18.3f);
                        Console.WriteLine("MeshRenderer BG Texture 已替换");
                    }
                    else
                    {
                        Console.WriteLine("MeshRenderer 未找到或 Material 为空");
                    }
                    var camera = __instance.transform.Find("EnvironmentUISceneWood/WoodsCameraContainer/MainMenuCamera");
                    if (camera != null)
                    {
                        var bloom = camera.GetComponent("UnityStandardAssets.ImageEffects.Bloom") as MonoBehaviour;
                        var bloom2 = camera.GetComponent("UnityStandardAssets.ImageEffects.BloomAndFlares") as MonoBehaviour;
                        var color = camera.GetComponent("UnityStandardAssets.ImageEffects.ColorCorrectionCurves") as MonoBehaviour;
                        bloom.enabled = false;
                        bloom2.enabled = false;
                        color.enabled = false;
                        camera.GetComponent<PrismEffects>().enabled = false;
                        //camera.GetComponent("UnityStandardAssets.ImageEffects.Bloom").gameObject.SetActive(false);
                    }
                    var topright = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/BranchContainer/Pine_branch_animated/Pine_branch");
                    var topright2 = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/BranchContainer/Pine_branch_animated/PineHub/PineBone/PineHub004Bone001Bone001");
                    if (topright != null)
                    {
                        topright.gameObject.SetActive(false);
                    }
                    if (topright2 != null)
                    {
                        topright2.gameObject.SetActive(false);
                    }
                    var pointLights = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/BranchContainer")
                    .GetComponentsInChildren<Transform>(true)
                    .Where(t => t.name == "Point light")
                    .ToArray()[2];
                    pointLights.transform.position = new Vector3(-0.1953f, - 998.6731f, 0.8506f);
                    var light = pointLights.GetComponent<Light>();
                    light.intensity = 3f;
                    light.range = 9f;
                    /*
                    var pdcopy = __instance.transform.Find("EnvironmentUISceneWood/WoodsLayout/panorama_front");
                    if (pdcopy == null)
                    {
                        pdcopy = Instantiate(panorama, panorama.transform.parent);
                    }
                    FG1.filterMode = FilterMode.Trilinear;
                    FG1.mipMapBias = 0f;
                    FG1.ignoreMipmapLimit = true;
                    FG2.filterMode = FilterMode.Trilinear;
                    FG2.mipMapBias = 0f;
                    FG2.ignoreMipmapLimit = true;
                    FG3.filterMode = FilterMode.Trilinear;
                    FG3.mipMapBias = 0f;
                    FG3.ignoreMipmapLimit = true;
                    FG4.filterMode = FilterMode.Trilinear;
                    FG4.mipMapBias = 0f;
                    FG4.ignoreMipmapLimit = true;
                    pdcopy.name = "panorama_front";
                    var bggocopy = pdcopy.GetComponent<MeshRenderer>();
                    var bggotmcopy = pdcopy.GetComponent<Transform>();
                    if (bggocopy != null && bggocopy.materials != null)
                    {
                        bggocopy.materials[0].SetTexture("_EmissionMap", FG1);
                        bggocopy.materials[1].SetTexture("_EmissionMap", FG4);
                        bggocopy.materials[2].SetTexture("_EmissionMap", FG3);
                        bggocopy.materials[3].SetTexture("_EmissionMap", FG2);
                        bggocopy.materials[0].mainTexture = FG1;
                        bggocopy.materials[1].mainTexture = FG4;
                        bggocopy.materials[2].mainTexture = FG3;
                        bggocopy.materials[3].mainTexture = FG2;
                        foreach (var mat in bggocopy.materials)
                        {
                            // 设置 Shader 为标准 Shader
                            //mat.shader = Shader.Find("Standard");

                            // 设置材质颜色为白色，确保透明部分不受颜色影响
                            mat.color = Color.white;

                            // 启用透明度
                            //mat.shader = Shader.Find("Standard");
                            mat.shader = Shader.Find("Standard");  // 使用 Standard Shader

                            // 设置透明渲染模式
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);  // 设置源混合模式为 SrcAlpha
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);  // 目标混合模式为 OneMinusSrcAlpha
                            mat.SetInt("_ZWrite", 0);  // 禁用深度写入
                            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);  // 关闭背面剔除
                            mat.SetFloat("_Mode", 3);  // 透明模式（3表示透明）
                            mat.renderQueue = 3000;  // 设置透明物体的渲染队列（3000是标准透明队列）

                            // 启用透明关键字
                            mat.EnableKeyword("_ALPHATEST_ON");
                            mat.EnableKeyword("_ALPHABLEND_ON");
                            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            // 确保材质的纹理正确，且纹理具有 Alpha 通道
                            // 你可以在此处设置 _MainTex 和 _EmissionMap 等纹理
                            //mat.mainTexture = yourPNGTexture;  // 设置你的 PNG 纹理
                            //mat.SetTexture("_EmissionMap", yourPNGTexture);  // 如果需要启用发光贴图
                        }
                        //bggotmcopy.localEulerAngles = new Vector3(270f, 225f, 0f);
                        //bggotmcopy.localPosition = new Vector3(0f, -4.85f, 0f);
                        bggotmcopy.position = new Vector3(0f, -1004.65f, 0f);
                        bggotmcopy.localScale = new Vector3(5.85f, 5.35f, 17.4f);
                        Console.WriteLine("MeshRenderer BG Texture 已替换");
                    }
                    */
                    //Vector3 desiredWorldPosition = new Vector3(1.8308f, -999.299f, 1.564f);
                    //Vector3 desiredLocalPosition = new Vector3(1.8308f, 0.701f, 1.564f);

                    // 首先设置世界位置
                    //pointLights.transform.position = desiredWorldPosition;

                    // 然后计算出父物体相对该世界位置的局部位置
                    //Vector3 parentPosition = pointLights.transform.parent != null ? pointLights.transform.parent.position : Vector3.zero;
                    //Vector3 localPos = pointLights.transform.InverseTransformPoint(desiredWorldPosition);

                    // 设置局部位置
                    //pointLights.transform.localPosition = new Vector3(localPos.x, desiredLocalPosition.y, localPos.z);
                }
                catch (Exception err)
                {
                    Debug.LogError(err);
                }
            }
        }
        /* 转生图标替换, 暂时弃用
        [HarmonyPatch(typeof(ChatSpecialIconSettings), nameof(ChatSpecialIconSettings.GetPrestigeLevelIconData))]
        static class PrestigeSpritePatch
        {
            static bool Prefix(ChatSpecialIconSettings __instance, int prestigeLevel, ref PrestigeIconsData __result)
            {
                PrestigeIconsData result;
                if (__instance.PrestigeIcons.TryGetValue(prestigeLevel, out result))
                {
                    switch (prestigeLevel)
                    {
                        case 1:
                            {
                                //Sprite.Create
                                var sprite = CreatePrestigeIconsData(PrestigeSprite.Level1_32x, PrestigeSprite.Level1_132x, PrestigeSprite.Level1_512x);
                                result.ConformationIcon = sprite.ConformationIcon;
                                result.BigIcon = sprite.BigIcon;
                                result.SmallIcon = sprite.SmallIcon;
                            }
                            break;
                        case 2:
                            {
                                //Sprite.Create
                                var sprite = CreatePrestigeIconsData(PrestigeSprite.Level2_32x, PrestigeSprite.Level2_132x, PrestigeSprite.Level2_512x);
                                result.ConformationIcon = sprite.ConformationIcon;
                                result.BigIcon = sprite.BigIcon;
                                result.SmallIcon = sprite.SmallIcon;
                            }
                            break;
                        case 3:
                            {
                                //Sprite.Create
                                var sprite = CreatePrestigeIconsData(PrestigeSprite.Level3_32x, PrestigeSprite.Level3_132x, PrestigeSprite.Level3_512x);
                                result.ConformationIcon = sprite.ConformationIcon;
                                result.BigIcon = sprite.BigIcon;
                                result.SmallIcon = sprite.SmallIcon;
                            }
                            break;
                        case 4:
                            {
                                //Sprite.Create
                                var sprite = CreatePrestigeIconsData(PrestigeSprite.Level4_32x, PrestigeSprite.Level4_132x, PrestigeSprite.Level4_512x);
                                result.ConformationIcon = sprite.ConformationIcon;
                                result.BigIcon = sprite.BigIcon;
                                result.SmallIcon = sprite.SmallIcon;
                            }
                            break;
                    }
                    __result = result;
                    return false;
                }
                Debug.LogError(string.Format("There is no icon for prestige level {0}", prestigeLevel));
                __result = null;
                return false;
            }
        }
        */
        public static PrestigeIconsData CreatePrestigeIconsData(Texture2D icon32x, Texture2D icon132x, Texture2D icon512x)
        {
            return new PrestigeIconsData
            {
                BigIcon = SimpleCreateSprite(icon132x),
                SmallIcon = SimpleCreateSprite(icon32x),
                ConformationIcon = SimpleCreateSprite(icon512x),
            };
        }
        public static PrestigeIconsData CreatePrestigeIconsData2(Texture2D icon32x, Texture2D icon132x, Texture2D icon512x, PrestigeIconsData icondata)
        {

            return new PrestigeIconsData
            {
                BigIcon = Sprite.Create(
                    icon512x,
                    icondata.BigIcon.rect,           // 保持原矩形
                    icondata.BigIcon.pivot,          // 保持原 pivot
                    icondata.BigIcon.pixelsPerUnit,  // 保持原单位
                    0,
                    SpriteMeshType.FullRect,
                    icondata.BigIcon.border          // 保持原边框
                ),
                SmallIcon = Sprite.Create(
                    icon132x,
                    icondata.SmallIcon.rect,           // 保持原矩形
                    icondata.SmallIcon.pivot,          // 保持原 pivot
                    icondata.SmallIcon.pixelsPerUnit,  // 保持原单位
                    0,
                    SpriteMeshType.FullRect,
                    icondata.SmallIcon.border          // 保持原边框
                ),
                ConformationIcon = Sprite.Create(
                    icon32x,
                    icondata.ConformationIcon.rect,           // 保持原矩形
                    icondata.ConformationIcon.pivot,          // 保持原 pivot
                    icondata.ConformationIcon.pixelsPerUnit,  // 保持原单位
                    0,
                    SpriteMeshType.FullRect,
                    icondata.ConformationIcon.border          // 保持原边框
                ),
            };
        }
        public static Sprite SimpleCreateSprite(Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
        }
        public static class PrestigeSprite
        {
            // --- 等级 1 ---
            public static Texture2D Level1_32x = LoadFromFile("prestigeicon/prestige_level_1_32x.png", 32, 32);
            public static Texture2D Level1_132x = LoadFromFile("prestigeicon/prestige_level_1_132x.png", 132, 132);
            public static Texture2D Level1_512x = LoadFromFile("prestigeicon/prestige_level_1_512x.png", 512, 512);
            // --- 等级 2 ---
            public static Texture2D Level2_32x = LoadFromFile("prestigeicon/prestige_level_2_32x.png", 32, 32);
            public static Texture2D Level2_132x = LoadFromFile("prestigeicon/prestige_level_2_132x.png", 132, 132);
            public static Texture2D Level2_512x = LoadFromFile("prestigeicon/prestige_level_2_512x.png", 512, 512);
            // --- 等级 3 ---
            public static Texture2D Level3_32x = LoadFromFile("prestigeicon/prestige_level_3_32x.png", 32, 32);
            public static Texture2D Level3_132x = LoadFromFile("prestigeicon/prestige_level_3_132x.png", 132, 132);
            public static Texture2D Level3_512x = LoadFromFile("prestigeicon/prestige_level_3_512x.png", 512, 512);
            // --- 等级 4 ---
            public static Texture2D Level4_32x = LoadFromFile("prestigeicon/prestige_level_4_32x.png", 32, 32);
            public static Texture2D Level4_132x = LoadFromFile("prestigeicon/prestige_level_4_132x.png", 132, 132);
            public static Texture2D Level4_512x = LoadFromFile("prestigeicon/prestige_level_4_512x.png", 512, 512);
        }
        internal static ConfigEntry<bool> UsePVPLogo { get; set; }
        internal static ConfigEntry<bool> ChangeLogo { get; set; }
        internal static ConfigEntry<bool> DisableTopGlow { get; set; }

    }
}