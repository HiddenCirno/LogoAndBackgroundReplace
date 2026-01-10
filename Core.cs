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
        // ==================== 插件配置常量 ====================
        private const string ASSEMBLY_GUID = "com.eft.logoreplace";

        // 资源文件名称常量
        private const string LOGO_FILE_NAME = "logo_pvp.png";
        private const string BG1_FILE_NAME = "Ending_part4_Merge.png";
        private const string BG2_FILE_NAME = "Ending_part1_Merge.png";
        private const string BG3_FILE_NAME = "Ending_part2_Merge.png";
        private const string BG4_FILE_NAME = "Ending_part3_Merge_New.png";

        // 游戏对象路径常量
        public const string LOGO_DECAL_PVP_PATH = "EnvironmentUISceneWood/WoodsLayout/logo_decal/decal_plane";
        public const string LOGO_DECAL_PVE_PATH = "EnvironmentUISceneWood/WoodsLayout/logo_decal/decal_plane_pve";
        public const string PANORAMA_PATH = "EnvironmentUISceneWood/WoodsLayout/panorama";
        public const string TOP_GLOW_PVE_PATH = "Common/Glow Canvas/TopGlowPve";
        public const string TOP_GLOW_PVP_PATH = "Common/Glow Canvas/TopGlowRegular";
        public const string CAMERA_PATH = "EnvironmentUISceneWood/WoodsCameraContainer/MainMenuCamera";
        public const string BRANCH_PATH = "EnvironmentUISceneWood/WoodsLayout/BranchContainer/Pine_branch_animated/Pine_branch";
        public const string BRANCH_SUB_PATH = "EnvironmentUISceneWood/WoodsLayout/BranchContainer/Pine_branch_animated/PineHub/PineBone/PineHub004Bone001Bone001";
        public const string BRANCH_CONTAINER_PATH = "EnvironmentUISceneWood/WoodsLayout/BranchContainer";

        // 后处理组件名称常量
        public const string BLOOM_COMPONENT_NAME = "UnityStandardAssets.ImageEffects.Bloom";
        public const string BLOOM_FLARES_COMPONENT_NAME = "UnityStandardAssets.ImageEffects.BloomAndFlares";
        public const string COLOR_CORRECTION_COMPONENT_NAME = "UnityStandardAssets.ImageEffects.ColorCorrectionCurves";

        // 背景调整参数常量
        public static readonly Vector3 BACKGROUND_ROTATION = new Vector3(270f, 225f, 0f);
        public static readonly Vector3 BACKGROUND_POSITION = new Vector3(0f, -4.85f, 0f);
        public static readonly Vector3 BACKGROUND_SCALE = new Vector3(5.9f, 5.4f, 18.3f);
        public static readonly Vector3 POINT_LIGHT_POSITION = new Vector3(-0.1953f, -998.6731f, 0.8506f);

        // 光源参数常量
        public const float LIGHT_INTENSITY = 3f;
        public const float LIGHT_RANGE = 9f;

        // 默认纹理尺寸
        public const int DEFAULT_TEXTURE_WIDTH = 2;
        public const int DEFAULT_TEXTURE_HEIGHT = 2;

        // ==================== 静态资源字段 ====================
        public static string PluginAssemblyPath => Assembly.GetExecutingAssembly().Location;
        public static string PluginDirectory => Path.GetDirectoryName(PluginAssemblyPath);

        public static Texture2D PvpLogoTexture;
        public static Texture2D BackgroundTexture1;
        public static Texture2D BackgroundTexture2;
        public static Texture2D BackgroundTexture3;
        public static Texture2D BackgroundTexture4;

        // ==================== 插件生命周期方法 ====================
        private void Awake()
        {
            try
            {
                Debug.Log("[LogoReplace] 插件初始化开始");

                // 加载所有纹理资源
                LoadTextures();

                // 应用Harmony补丁
                ApplyHarmonyPatches();

                Debug.Log("[LogoReplace] 插件初始化完成");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[LogoReplace] 插件初始化失败: {exception.Message}");
                Debug.LogError(exception.StackTrace);
            }
        }

        // ==================== 资源管理方法 ====================
        /// <summary>
        /// 加载所有需要的纹理资源
        /// </summary>
        private void LoadTextures()
        {
            Debug.Log("[LogoReplace] 开始加载纹理资源");

            PvpLogoTexture = LoadTextureFromFile(LOGO_FILE_NAME);
            BackgroundTexture1 = LoadTextureFromFile(BG1_FILE_NAME);
            BackgroundTexture2 = LoadTextureFromFile(BG2_FILE_NAME);
            BackgroundTexture3 = LoadTextureFromFile(BG3_FILE_NAME);
            BackgroundTexture4 = LoadTextureFromFile(BG4_FILE_NAME);

            Debug.Log("[LogoReplace] 纹理资源加载完成");
        }

        /// <summary>
        /// 从文件加载纹理
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="width">纹理宽度（默认值）</param>
        /// <param name="height">纹理高度（默认值）</param>
        /// <returns>加载的纹理，如果失败则返回null</returns>
        public static Texture2D LoadTextureFromFile(string fileName, int width = DEFAULT_TEXTURE_WIDTH, int height = DEFAULT_TEXTURE_HEIGHT)
        {
            string filePath = Path.Combine(PluginDirectory, fileName);

            if (!File.Exists(filePath))
            {
                Debug.LogError($"[LogoReplace] 文件未找到: {filePath}");
                return null;
            }

            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

                texture.LoadImage(fileBytes);

                Debug.Log($"[LogoReplace] 纹理加载成功: {fileName}");
                return texture;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[LogoReplace] 加载纹理时出错 {fileName}: {exception.Message}");
                return null;
            }
        }

        // ==================== Harmony补丁管理 ====================
        /// <summary>
        /// 应用所有Harmony补丁
        /// </summary>
        private void ApplyHarmonyPatches()
        {
            Harmony harmonyInstance = new Harmony(ASSEMBLY_GUID);
            harmonyInstance.PatchAll();

            Debug.Log($"[LogoReplace] Harmony补丁已应用: {ASSEMBLY_GUID}");
        }
    }

    // ==================== UI组件替换管理器 ====================
    /// <summary>
    /// UI组件替换管理器，负责处理所有UI元素的替换逻辑
    /// </summary>
    internal static class UIReplacementManager
    {
        // ==================== Logo替换模块 ====================
        /// <summary>
        /// 替换游戏Logo
        /// </summary>
        /// <param name="environmentUIInstance">环境UI实例</param>
        public static void ReplaceLogo(EnvironmentUI environmentUIInstance)
        {
            try
            {
                Debug.Log("[LogoReplace] 开始替换Logo");

                // 查找Logo游戏对象
                Transform pvpLogoTransform = environmentUIInstance.transform.Find(LogoReplacePlugin.LOGO_DECAL_PVP_PATH);
                Transform pveLogoTransform = environmentUIInstance.transform.Find(LogoReplacePlugin.LOGO_DECAL_PVE_PATH);

                if (pvpLogoTransform == null)
                {
                    Debug.LogWarning("[LogoReplace] PVP Logo游戏对象未找到");
                    return;
                }

                // 替换PVP Logo纹理
                MeshRenderer pvpLogoRenderer = pvpLogoTransform.GetComponent<MeshRenderer>();
                if (pvpLogoRenderer != null && pvpLogoRenderer.sharedMaterial != null)
                {
                    pvpLogoRenderer.sharedMaterial.mainTexture = LogoReplacePlugin.PvpLogoTexture;
                    Debug.Log("[LogoReplace] PVP Logo纹理已替换");
                }
                else
                {
                    Debug.LogWarning("[LogoReplace] PVP Logo的MeshRenderer或Material未找到");
                }

                // 控制Logo显示状态
                if (pveLogoTransform != null)
                {
                    pveLogoTransform.gameObject.SetActive(false);
                    Debug.Log("[LogoReplace] PVE Logo已隐藏");
                }

                pvpLogoTransform.gameObject.SetActive(true);
                Debug.Log("[LogoReplace] PVP Logo已显示");

                // 控制Logo光效显示状态
                ControlLogoGlowEffects(environmentUIInstance);

                Debug.Log("[LogoReplace] Logo替换完成");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[LogoReplace] 替换Logo时出错: {exception.Message}");
            }
        }

        /// <summary>
        /// 控制Logo光效的显示状态
        /// </summary>
        /// <param name="environmentUIInstance">环境UI实例</param>
        private static void ControlLogoGlowEffects(EnvironmentUI environmentUIInstance)
        {
            Transform pveGlowTransform = environmentUIInstance.transform.Find(LogoReplacePlugin.TOP_GLOW_PVE_PATH);
            Transform pvpGlowTransform = environmentUIInstance.transform.Find(LogoReplacePlugin.TOP_GLOW_PVP_PATH);

            if (pveGlowTransform != null)
            {
                pveGlowTransform.gameObject.SetActive(false);
                Debug.Log("[LogoReplace] PVE光效已隐藏");
            }

            if (pvpGlowTransform != null)
            {
                pvpGlowTransform.gameObject.SetActive(true);
                Debug.Log("[LogoReplace] PVP光效已显示");
            }
        }

        // ==================== 背景替换模块 ====================
        /// <summary>
        /// 替换游戏背景
        /// </summary>
        /// <param name="environmentUIInstance">环境UI实例</param>
        public static void ReplaceBackground(EnvironmentUI environmentUIInstance)
        {
            try
            {
                Debug.Log("[LogoReplace] 开始替换背景");

                // 查找背景游戏对象
                Transform panoramaTransform = environmentUIInstance.transform.Find(LogoReplacePlugin.PANORAMA_PATH);

                if (panoramaTransform == null)
                {
                    Debug.LogWarning("[LogoReplace] 背景游戏对象未找到");
                    return;
                }

                // 替换背景纹理
                ReplaceBackgroundTextures(panoramaTransform);

                // 调整背景参数
                AdjustBackgroundParameters(panoramaTransform);

                // 禁用后处理效果
                DisablePostProcessingEffects(environmentUIInstance);

                // 调整光源设置
                AdjustLightingSettings(environmentUIInstance);

                Debug.Log("[LogoReplace] 背景替换完成");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[LogoReplace] 替换背景时出错: {exception.Message}");
            }
        }

        /// <summary>
        /// 替换背景纹理
        /// </summary>
        /// <param name="panoramaTransform">全景背景变换组件</param>
        private static void ReplaceBackgroundTextures(Transform panoramaTransform)
        {
            MeshRenderer backgroundRenderer = panoramaTransform.GetComponent<MeshRenderer>();

            if (backgroundRenderer == null || backgroundRenderer.sharedMaterials == null)
            {
                Debug.LogWarning("[LogoReplace] 背景MeshRenderer或Materials未找到");
                return;
            }

            Material[] backgroundMaterials = backgroundRenderer.sharedMaterials;

            // 确保有足够的材质槽
            if (backgroundMaterials.Length >= 4)
            {
                // 替换发射贴图
                backgroundMaterials[0].SetTexture("_EmissionMap", LogoReplacePlugin.BackgroundTexture1);
                backgroundMaterials[1].SetTexture("_EmissionMap", LogoReplacePlugin.BackgroundTexture4);
                backgroundMaterials[2].SetTexture("_EmissionMap", LogoReplacePlugin.BackgroundTexture3);
                backgroundMaterials[3].SetTexture("_EmissionMap", LogoReplacePlugin.BackgroundTexture2);

                // 替换主贴图
                backgroundMaterials[0].mainTexture = LogoReplacePlugin.BackgroundTexture1;
                backgroundMaterials[1].mainTexture = LogoReplacePlugin.BackgroundTexture4;
                backgroundMaterials[2].mainTexture = LogoReplacePlugin.BackgroundTexture3;
                backgroundMaterials[3].mainTexture = LogoReplacePlugin.BackgroundTexture2;

                Debug.Log("[LogoReplace] 背景纹理已替换");
            }
            else
            {
                Debug.LogWarning($"[LogoReplace] 背景材质数量不足: {backgroundMaterials.Length}/4");
            }
        }

        /// <summary>
        /// 调整背景参数
        /// </summary>
        /// <param name="panoramaTransform">全景背景变换组件</param>
        private static void AdjustBackgroundParameters(Transform panoramaTransform)
        {
            // 应用材质调整
            MeshRenderer backgroundRenderer = panoramaTransform.GetComponent<MeshRenderer>();
            foreach (Material material in backgroundRenderer.sharedMaterials)
            {
                material.shader = Shader.Find("Standard");
                material.color = Color.white;
            }

            // 调整变换参数
            panoramaTransform.localEulerAngles = LogoReplacePlugin.BACKGROUND_ROTATION;
            panoramaTransform.localPosition = LogoReplacePlugin.BACKGROUND_POSITION;
            panoramaTransform.localScale = LogoReplacePlugin.BACKGROUND_SCALE;

            Debug.Log("[LogoReplace] 背景参数已调整");
        }

        /// <summary>
        /// 禁用后处理效果
        /// </summary>
        /// <param name="environmentUIInstance">环境UI实例</param>
        private static void DisablePostProcessingEffects(EnvironmentUI environmentUIInstance)
        {
            Transform cameraTransform = environmentUIInstance.transform.Find(LogoReplacePlugin.CAMERA_PATH);

            if (cameraTransform == null)
            {
                Debug.LogWarning("[LogoReplace] 相机游戏对象未找到");
                return;
            }

            try
            {
                // 禁用Bloom效果
                MonoBehaviour bloomComponent = cameraTransform.GetComponent(LogoReplacePlugin.BLOOM_COMPONENT_NAME) as MonoBehaviour;
                if (bloomComponent != null) bloomComponent.enabled = false;

                // 禁用Bloom和光晕效果
                MonoBehaviour bloomFlaresComponent = cameraTransform.GetComponent(LogoReplacePlugin.BLOOM_FLARES_COMPONENT_NAME) as MonoBehaviour;
                if (bloomFlaresComponent != null) bloomFlaresComponent.enabled = false;

                // 禁用颜色校正
                MonoBehaviour colorCorrectionComponent = cameraTransform.GetComponent(LogoReplacePlugin.COLOR_CORRECTION_COMPONENT_NAME) as MonoBehaviour;
                if (colorCorrectionComponent != null) colorCorrectionComponent.enabled = false;

                // 禁用棱镜效果
                PrismEffects prismEffects = cameraTransform.GetComponent<PrismEffects>();
                if (prismEffects != null) prismEffects.enabled = false;

                Debug.Log("[LogoReplace] 后处理效果已禁用");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[LogoReplace] 禁用后处理效果时出错: {exception.Message}");
            }
        }

        /// <summary>
        /// 调整光源设置
        /// </summary>
        /// <param name="environmentUIInstance">环境UI实例</param>
        private static void AdjustLightingSettings(EnvironmentUI environmentUIInstance)
        {
            try
            {
                // 隐藏树枝元素
                Transform branchTransform = environmentUIInstance.transform.Find(LogoReplacePlugin.BRANCH_PATH);
                Transform branchSubTransform = environmentUIInstance.transform.Find(LogoReplacePlugin.BRANCH_SUB_PATH);

                if (branchTransform != null) branchTransform.gameObject.SetActive(false);
                if (branchSubTransform != null) branchSubTransform.gameObject.SetActive(false);

                Debug.Log("[LogoReplace] 树枝元素已隐藏");

                // 调整点光源
                Transform branchContainer = environmentUIInstance.transform.Find(LogoReplacePlugin.BRANCH_CONTAINER_PATH);
                if (branchContainer != null)
                {
                    Transform[] pointLights = branchContainer.GetComponentsInChildren<Transform>(true)
                        .Where(transform => transform.name == "Point light")
                        .ToArray();

                    if (pointLights.Length > 2)
                    {
                        Transform targetPointLight = pointLights[2];
                        targetPointLight.position = LogoReplacePlugin.POINT_LIGHT_POSITION;

                        Light pointLightComponent = targetPointLight.GetComponent<Light>();
                        if (pointLightComponent != null)
                        {
                            pointLightComponent.intensity = LogoReplacePlugin.LIGHT_INTENSITY;
                            pointLightComponent.range = LogoReplacePlugin.LIGHT_RANGE;
                        }

                        Debug.Log("[LogoReplace] 点光源已调整");
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[LogoReplace] 调整光源设置时出错: {exception.Message}");
            }
        }
    }

    // ==================== Harmony补丁类 ====================
    /// <summary>
    /// EnvironmentUI.ShowEnvironment方法的补丁
    /// </summary>
    [HarmonyPatch(typeof(EnvironmentUI), nameof(EnvironmentUI.ShowEnvironment))]
    internal static class EnvironmentUIPatch
    {
        /// <summary>
        /// ShowEnvironment方法的后置补丁
        /// </summary>
        /// <param name="__instance">EnvironmentUI实例</param>
        /// <param name="value">是否显示环境</param>
        [HarmonyPostfix]
        internal static void Postfix(EnvironmentUI __instance, bool value)
        {
            try
            {
                Debug.Log("[LogoReplace] 开始执行UI替换");

                // 执行Logo替换
                UIReplacementManager.ReplaceLogo(__instance);

                // 执行背景替换
                UIReplacementManager.ReplaceBackground(__instance);

                Debug.Log("[LogoReplace] UI替换执行完成");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[LogoReplace] 执行UI替换时发生未捕获的异常: {exception.Message}");
                Debug.LogError(exception.StackTrace);
            }
        }
    }
}