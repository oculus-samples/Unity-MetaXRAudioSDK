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

using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetaXRAudioSDK
{
    public class ReturnToMain : MonoBehaviour
    {
        private const string MAIN_SCENE_NAME = "Main";
        private ControllerRef _rightController = null;
        private ControllerRef _leftController = null;
        private UIAudio _uiAudio = null;

        private void Start()
        {
            var uiAudios = FindObjectsOfType<UIAudio>();
            UnityEngine.Assertions.Assert.AreEqual(uiAudios.Length, 1);
            _uiAudio = uiAudios[0];
        }

        private void Update()
        {
            if (_rightController == null || _leftController == null)
            {
                LocateControllers();
            }

            if (_rightController != null && _rightController.IsButtonUsageAnyActive(ControllerButtonUsage.SecondaryButton))
            {
                Debug.Log("B button pressed, returning to Main scene.");
                _uiAudio.PlayReleaseSound(_rightController.transform.position);
                ChangeScene();
            }

            if (_leftController != null && _leftController.IsButtonUsageAnyActive(ControllerButtonUsage.SecondaryButton))
            {
                Debug.Log("Y button pressed, returning to Main scene.");
                _uiAudio.PlayReleaseSound(_rightController.transform.position);
                ChangeScene();
            }
        }

        private void LocateControllers()
        {
            var controllers = FindObjectsOfType<ControllerRef>();
            foreach (var controller in controllers)
            {
                if (controller.Handedness == Handedness.Right)
                {
                    _rightController = controller;
                }
                else if (controller.Handedness == Handedness.Left)
                {
                    _leftController = controller;
                }
            }

            if (_rightController == null)
            {
                Debug.LogError("Failed to locate right controller in scene");
            }

            if (_leftController == null)
            {
                Debug.LogError("Failed to locate left controller in scene");
            }
        }

        private void ChangeScene()
        {
            SceneManager.LoadScene(MAIN_SCENE_NAME);
        }
    }
}
