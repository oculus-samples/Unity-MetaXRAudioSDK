/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SignalDotsController : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] SkinnedMeshRenderer[] _skinnedMeshRenderers;
    [SerializeField] float _yRMSScaler = 100;
    [SerializeField] float _minYScalerPercentage = 0.1f;
    [SerializeField] float _disappearThresholdPercentage = 0.1f;
    [SerializeField] float _timeBeforeDisappearInSeconds = 1.0f;
    [SerializeField] float _appearTimeInSeconds = 0.1f;
    [SerializeField] float _disappearTimeInSeconds = 0.1f;

    private DelaySwitch delaySwitch = new DelaySwitch();
    private Vector3 _initialScale;
    private float[] _initialDotYScales;

    const int RMS_SAMPLE_SIZE = 1000;
    private float[] _samples;
    private Queue<float> _rmsValues;

    void Start()
    {
        if (_audioSource == null)
        {
            _audioSource = gameObject.GetComponentInParent<AudioSource>();
        }

        if (_audioSource == null)
        {
            this.gameObject.SetActive(false);
            return;
        }

        _samples = new float[RMS_SAMPLE_SIZE];

        _initialScale = this.gameObject.transform.localScale;

        _initialDotYScales = new float[_skinnedMeshRenderers.Length];
        for (int n = 0; n < _skinnedMeshRenderers.Length; n++)
        {
            _initialDotYScales[n] = _skinnedMeshRenderers[n].transform.localScale.y;
        }

        _rmsValues = new Queue<float>();
    }

    void Update()
    {
        if (_audioSource != null)
        {
            while (_rmsValues.Count < _skinnedMeshRenderers.Length)
            {
                _rmsValues.Enqueue(0);
            }

            System.Array.Clear(_samples, 0, _samples.Length);
            _audioSource.clip.GetData(_samples, _audioSource.timeSamples);
            _rmsValues.Enqueue(CalcRMS(_samples));

            while (_rmsValues.Count > _skinnedMeshRenderers.Length)
            {
                _rmsValues.Dequeue();
            }

            bool showGraphic = false;
            for (int n = 0; n < _skinnedMeshRenderers.Length; n++)
            {
                float yScale = _yRMSScaler * _initialDotYScales[n] * _rmsValues.ElementAt(_skinnedMeshRenderers.Length - n - 1);
                yScale = yScale > _initialDotYScales[n] ? _initialDotYScales[n] : yScale;
                yScale = yScale < _minYScalerPercentage * _initialDotYScales[n] ? _minYScalerPercentage * _initialDotYScales[n] : yScale;
                _skinnedMeshRenderers[n].SetBlendShapeWeight(0, yScale * 100);
                if (yScale > (1 + _disappearThresholdPercentage) * _minYScalerPercentage * _initialDotYScales[n])
                {
                    showGraphic = true;
                }
            }

            delaySwitch.timeBeforeDisappear = _timeBeforeDisappearInSeconds;
            delaySwitch.appearTime = _appearTimeInSeconds;
            delaySwitch.disappearTime = _disappearTimeInSeconds;
            float value = delaySwitch.Update(showGraphic);
            transform.localScale = new Vector3(
                _initialScale.x,
                value * _initialScale.y,
                _initialScale.z);
        }
    }

    static float CalcRMS(float[] samples)
    {
        float sum = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }

        return Mathf.Sqrt(sum / samples.Length);
    }

    private class DelaySwitch
    {
        public float timeBeforeDisappear = 1;
        public float appearTime = 1;
        public float disappearTime = 1;
        private float value = 0;
        private float dt = 0;

        public DelaySwitch() { }

        public float Update(bool on)
        {
            if (on)
            {
                dt = 0;
                value += Time.deltaTime / appearTime;
                value = value > 1 ? 1 : value;
            }
            else
            {
                dt += Time.deltaTime;
                if (dt > timeBeforeDisappear)
                {
                    value -= Time.deltaTime / disappearTime;
                    value = value < 0 ? 0 : value;
                }
            }

            return value;
        }
    }
}
