using System;
using UnityEngine;

namespace UnityEssentials
{
    [Serializable]
    public class AutoExposureSettings
    {
        public bool _autoExposure = true;

        [Space]
        [Min(0.1f)] public float MaxAdjustmentSpeed = 2f;
        [Range(0.1f, 15f)] public float AdjustmentSpeedLightToDark = 0.25f;
        [Range(0.1f, 15f)] public float AdjustmentSpeedDarkToLight = 0.25f;

        [Space]
        [Range(0.1f, 2f)] public float ShutterBias = 2;
        [Range(0.1f, 2f)] public float IsoBias = 1;

        [Space]
        [Range(0.1f, 15f)] public float MinTargetLuminance = 1;
        [Range(0.1f, 15f)] public float MaxTargetLuminance = 7;
        [Range(0.01f, 1f)] public float LuminanceTolerance = 0.3f;
    }

    [RequireComponent(typeof(CameraPhysicalPropertiesController), typeof(CameraLuminanceCalculator))]
    public class CameraAutoExposureController : MonoBehaviour
    {
        public AutoExposureSettings Settings;

        private CameraPhysicalPropertiesController _controller;
        private CameraLuminanceCalculator _calculator;

        public void Awake()
        {
            _controller = GetComponent<CameraPhysicalPropertiesController>();
            _calculator = GetComponent<CameraLuminanceCalculator>();
        }

        public void Update() =>
            AdjustExposure();

        private void AdjustExposure()
        {
            if (!Settings._autoExposure || _calculator == null || _controller == null)
                return;

            float currentLuminance = _calculator.Luminance * 15;

            float targetLuminance = GetTargetLuminance(currentLuminance);
            float luminanceDifference = Mathf.Abs(currentLuminance - targetLuminance);

            // Calculate speed scale factor (1x at tolerance, max at full difference)
            float scaleFactor = Mathf.Lerp(1f, Settings.MaxAdjustmentSpeed,
                luminanceDifference / (1f - Settings.LuminanceTolerance));

            if (luminanceDifference > Settings.LuminanceTolerance)
            {
                if (currentLuminance < targetLuminance)
                {
                    // Too dark - increase exposure
                    float adjustment = Settings.AdjustmentSpeedLightToDark * scaleFactor * Time.deltaTime;

                    _controller.ShutterSpeedMultiplier = Mathf.Clamp01(_controller.ShutterSpeedMultiplier + adjustment * Settings.ShutterBias);
                    _controller.IsoMultiplier = Mathf.Clamp01(_controller.IsoMultiplier + adjustment * Settings.IsoBias);
                }
                else
                {
                    // Too bright - decrease exposure
                    float adjustment = Settings.AdjustmentSpeedDarkToLight * scaleFactor * Time.deltaTime;

                    _controller.ShutterSpeedMultiplier = Mathf.Clamp01(_controller.ShutterSpeedMultiplier - adjustment);
                    _controller.IsoMultiplier = Mathf.Clamp01(_controller.IsoMultiplier - Settings.AdjustmentSpeedDarkToLight * 2 * Time.deltaTime);
                }
            }

            // Too noisy - decrease iso
            if (IsLuminanceInTargetRange(currentLuminance, 0.06f))
                _controller.IsoMultiplier = Mathf.Clamp01(_controller.IsoMultiplier - Settings.AdjustmentSpeedDarkToLight * 0.5f * Time.deltaTime);
        }

        private float GetTargetLuminance(float currentLuminance)
        {
            if (currentLuminance < Settings.MinTargetLuminance) return Settings.MinTargetLuminance;
            if (currentLuminance > Settings.MaxTargetLuminance) return Settings.MaxTargetLuminance;

            return currentLuminance; // Maintain current if within range
        }

        private bool IsLuminanceInTargetRange(float currentLuminance, float tolerance)
        {
            bool aboveMinLuminance = (currentLuminance - tolerance) > Settings.MinTargetLuminance;
            bool belowMaxLuminance = (currentLuminance + tolerance) < Settings.MaxTargetLuminance;

            return aboveMinLuminance && belowMaxLuminance; // is current within range with tolerance
        }
    }
}