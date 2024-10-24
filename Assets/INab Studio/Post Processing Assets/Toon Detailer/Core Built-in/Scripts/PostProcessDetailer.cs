using System;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace INab.Detailer.BIRP
{
    [Serializable]
    public class DetailerSettings
    {
        public enum DetailerType { Both = 0, Contours = 1, Cavity = 2 }
        public enum StencilUse { None = 0, NotEqual = 1, Equal = 2 }

        // General

        [SerializeField] public DetailerType _DetailerType = DetailerType.Both;
        [SerializeField] public StencilUse _StencilUse = StencilUse.None;
        [SerializeField] public bool _FastMode = false;

        // Adjustments

        [SerializeField] public Color _ColorHue = Color.black;
        [SerializeField] public bool _UseFade = false;
        [SerializeField] public bool _FadeAffectsOnlyContours = false;
        [SerializeField] public float _FadeStart = 40;
        [SerializeField] public float _FadeEnd = 60;
        [SerializeField, Range(0, 1)] public float _BlackOffset = .2f;

        // Contours

        [SerializeField, Range(0, 1)] public float _ContoursIntensity = 1.0f;
        [SerializeField, Range(0, 3)] public float _ContoursThickness = 0.5f;
        [SerializeField, Range(0, 3)] public float _ContoursElevationStrength = 1;
        [SerializeField, Range(0, 0.9f)] public float _ContoursElevationSmoothness = 0;
        [SerializeField, Range(0, 3)] public float _ContoursDepressionStrength = 2;
        [SerializeField, Range(0, 0.9f)] public float _ContoursDepressionSmoothness = 0;

        // Cavity

        [SerializeField, Range(0, 1)] public float _CavityIntensity = 1.0f;
        [SerializeField, Range(0, 1)] public float _CavityRadius = 0.5f;
        [SerializeField, Range(0, 5)] public float _CavityStrength = 1.25f;
        [SerializeField, Range(1, 16)] public int _CavitySamples = 12;

    }

    //  ImageEffectAllowedInSceneView, 
    [ExecuteInEditMode,RequireComponent(typeof(Camera))]
    public class PostProcessDetailer : MonoBehaviour
    {
        [SerializeField] private DetailerSettings m_Settings = new DetailerSettings();

        public LayerMask _StencilMaskLayer;
        private List<Renderer> stencilRenderers = new List<Renderer>();

        private Material m_DetailerMaterial;
        private Material m_NormalsMaterial;
        private Material m_StencilMaterial;

        private const string k_UseContours = "_USE_CONTOURS";
        private const string k_UseCavity = "_USE_CAVITY";

        private const string k_Orthographic = "_ORTHOGRAPHIC";

        private const string k_FastMode = "_FAST_MODE";
        private const string k_FadeContoursOnly = "_FADE_COUNTOURS_ONLY";
        private const string k_FadeOn = "_FADE_ON";

        private Camera cam;
        private Camera sceneCam;
        private CommandBuffer cmd;

        private void FindStencilRenderers()
        {
            stencilRenderers.Clear();

            var renderersArray = FindObjectsOfType<Renderer>(false);

            foreach (var renderer in renderersArray)
            {
                if (_StencilMaskLayer == (_StencilMaskLayer | (1 << renderer.gameObject.layer)))
                {
                    stencilRenderers.Add(renderer);
                }
            }
        }

        private void OnEnable()
        {
            GetMaterials();

            cam = GetComponent<Camera>();
            cam.depthTextureMode = DepthTextureMode.Depth;

            cmd = new CommandBuffer();
            cmd.name = "Post Process Detailer";

            // Reconstructed normals
            int normalsRT = Shader.PropertyToID("_ReconstructedNormals");
            cmd.GetTemporaryRT(normalsRT, -1, -1, 0, FilterMode.Point);

            cmd.SetGlobalTexture("_ReconstructedNormals", normalsRT);

            cmd.SetRenderTarget(normalsRT);
            cmd.Blit(BuiltinRenderTextureType.CameraTarget, normalsRT, m_NormalsMaterial, 0);
            // Reconstructed normals

            // Temporarty RT
            int tempRT = Shader.PropertyToID("_MainTex");
            cmd.GetTemporaryRT(tempRT, -1, -1, 0, FilterMode.Point);

            cmd.SetGlobalTexture("_MainTex", tempRT);

            cmd.SetRenderTarget(tempRT);
            cmd.Blit(BuiltinRenderTextureType.CameraTarget, tempRT);
            // Temporarty RT

            // Stencil
            if (m_Settings._StencilUse != DetailerSettings.StencilUse.None)
            {
                cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

                FindStencilRenderers();
                foreach (var renderer in stencilRenderers)
                {
                    for (int submeshIndex = 0; submeshIndex < renderer.sharedMaterials.Length; submeshIndex++)
                    {
                        cmd.DrawRenderer(renderer, m_StencilMaterial, submeshIndex, 0);
                    }
                }
            }
            // Stencil


            // Main Blit
            cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cmd.Blit(tempRT, BuiltinRenderTextureType.CameraTarget, m_DetailerMaterial, ((int)m_Settings._StencilUse));

            cmd.ReleaseTemporaryRT(tempRT);
            cmd.ReleaseTemporaryRT(normalsRT);

            cam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, cmd);
#if UNITY_EDITOR
            if(SceneView.GetAllSceneCameras().Length > 0)
            {
                sceneCam = SceneView.GetAllSceneCameras()[0];
                sceneCam.depthTextureMode = DepthTextureMode.Depth;
                sceneCam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, cmd);
            }
#endif
        }

        private void OnDisable()
        {
            if (m_DetailerMaterial)
            {
                DestroyImmediate(m_DetailerMaterial);
            }

            if (m_NormalsMaterial)
            {
                DestroyImmediate(m_NormalsMaterial);
            }

            if (m_StencilMaterial)
            {
                DestroyImmediate(m_StencilMaterial);
            }

            cam.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, cmd);

#if UNITY_EDITOR
            if(sceneCam) sceneCam.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, cmd);
#endif
        }

        private void Update()
        {
            SetMaterialProperties();
        }

        private void SetMaterialProperties()
        {
            // Normals Material

            if (m_Settings._FastMode)
            {
                m_NormalsMaterial.EnableKeyword(k_FastMode);
            }
            else
            {
                m_NormalsMaterial.DisableKeyword(k_FastMode);
            }

            // General

            var camera = GetComponent<Camera>();

            if (camera.orthographic)
            {
                m_DetailerMaterial.EnableKeyword(k_Orthographic);
            }
            else
            {
                m_DetailerMaterial.DisableKeyword(k_Orthographic);
            }

            switch (m_Settings._DetailerType)
            {
                case DetailerSettings.DetailerType.Both:
                    m_DetailerMaterial.EnableKeyword(k_UseContours);
                    m_DetailerMaterial.EnableKeyword(k_UseCavity);
                    break;
                case DetailerSettings.DetailerType.Contours:
                    m_DetailerMaterial.EnableKeyword(k_UseContours);
                    m_DetailerMaterial.DisableKeyword(k_UseCavity);
                    break;
                case DetailerSettings.DetailerType.Cavity:
                    m_DetailerMaterial.DisableKeyword(k_UseContours);
                    m_DetailerMaterial.EnableKeyword(k_UseCavity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Adjustments

            m_DetailerMaterial.SetColor("_ColorHue", m_Settings._ColorHue);
            m_DetailerMaterial.SetFloat("_FadeStart", m_Settings._FadeStart);
            m_DetailerMaterial.SetFloat("_FadeEnd", m_Settings._FadeEnd);
            m_DetailerMaterial.SetFloat("_BlackOffset", m_Settings._BlackOffset);

            if (m_Settings._UseFade)
            {
                m_DetailerMaterial.EnableKeyword(k_FadeOn);
            }
            else
            {
                m_DetailerMaterial.DisableKeyword(k_FadeOn);
                m_DetailerMaterial.DisableKeyword(k_FadeContoursOnly);
            }


            if (m_Settings._FadeAffectsOnlyContours && m_Settings._UseFade)
            {
                m_DetailerMaterial.EnableKeyword(k_FadeContoursOnly);
                m_DetailerMaterial.DisableKeyword(k_FadeOn);
            }
            else
            {
                m_DetailerMaterial.DisableKeyword(k_FadeContoursOnly);
            }

            // Countour

            m_DetailerMaterial.SetFloat("_ContoursIntensity", m_Settings._ContoursIntensity);
            m_DetailerMaterial.SetFloat("_ContoursThickness", m_Settings._ContoursThickness);
            m_DetailerMaterial.SetFloat("_ContoursElevationStrength", 3.0f*(m_Settings._ContoursElevationStrength * (0.7f / (1.0f - m_Settings._ContoursElevationSmoothness))));
            m_DetailerMaterial.SetFloat("_ContoursElevationSmoothness", 1 - m_Settings._ContoursElevationSmoothness);
            m_DetailerMaterial.SetFloat("_ContoursDepressionStrength", 2.0f * (m_Settings._ContoursDepressionStrength * (0.7f / (1.0f - m_Settings._ContoursDepressionSmoothness))));
            m_DetailerMaterial.SetFloat("_ContoursDepressionSmoothness", 1 - m_Settings._ContoursDepressionSmoothness);

            // Cavity 
            m_DetailerMaterial.SetFloat("_CavityIntensity", m_Settings._CavityIntensity);
            m_DetailerMaterial.SetFloat("_CavityRadius", m_Settings._CavityRadius);
            m_DetailerMaterial.SetFloat("_CavityStrength", m_Settings._CavityStrength);
            m_DetailerMaterial.SetInt("_CavitySamples", m_Settings._CavitySamples);

        }

        private void GetMaterials()
        {
            m_DetailerMaterial = new Material(Shader.Find("Hidden/INab/DetailerBIRP"));
            m_NormalsMaterial = new Material(Shader.Find("Hidden/INab/DetailerBIRP/NormalsReconstruct"));
            m_StencilMaterial = new Material(Shader.Find("Hidden/INab/DetailerBIRP/Stencil"));
        }
    }
}