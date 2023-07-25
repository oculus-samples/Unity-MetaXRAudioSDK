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

namespace MetaXRAudioSDK
{
    public class HideDialog : MonoBehaviour
    {
        private ControllerRef _rightController = null;
        private ControllerRef _leftController = null;

        private void Update()
        {
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            if (_rightController == null || _leftController == null)
            {
                LocateControllers();
            }

            if (_rightController != null && _rightController.IsButtonUsageAnyActive(ControllerButtonUsage.PrimaryButton))
            {
                Debug.Log("A button pressed, hiding feature description dialog.");
                this.gameObject.SetActive(false);
            }

            if (_leftController != null && _leftController.IsButtonUsageAllActive(ControllerButtonUsage.PrimaryButton))
            {
                Debug.Log("X button pressed, hiding feature description dialog.");
                this.gameObject.SetActive(false);
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
    }
}
