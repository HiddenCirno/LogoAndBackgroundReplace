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
        public static Texture2D PVPLogoTexture = LoadFromFile("logo_pvp.png");
        public static Texture2D BG1 = LoadFromFile("Ending_part4_Merge.png");
        public static Texture2D BG2 = LoadFromFile("Ending_part1_Merge.png");
        public static Texture2D BG3 = LoadFromFile("Ending_part2_Merge.png");
        public static Texture2D BG4 = LoadFromFile("Ending_part3_Merge_New.png");
        private void Awake()
        {
            var harmony = new Harmony("com.eft.logoreplace");
            harmony.PatchAll();
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
                        mr_pvp.sharedMaterial.mainTexture = PVPLogoTexture;
                        Debug.Log("MeshRenderer logo Texture 已替换");
                    }
                    else
                    {
                        Debug.LogWarning("MeshRenderer 未找到或 Material 为空");
                    }
                        decal_pve.gameObject.SetActive(false);
                        decal_pvp.gameObject.SetActive(true);
                        var topglowpve = __instance.transform.Find("Common/Glow Canvas/TopGlowPve");
                        var topglowpvp = __instance.transform.Find("Common/Glow Canvas/TopGlowRegular");
                        if (topglowpve != null)
                        {
                            topglowpve.gameObject.SetActive(false);
                        }
                        if (topglowpvp != null)
                        {
                            topglowpvp.gameObject.SetActive(true);
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
                            mat.shader = Shader.Find("Standard");
                            mat.color = Color.white;
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
                }
                catch (Exception err)
                {
                    Debug.LogError(err);
                }
            }
        }

    }
}