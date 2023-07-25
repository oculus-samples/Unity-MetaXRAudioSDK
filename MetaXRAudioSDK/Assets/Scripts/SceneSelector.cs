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

using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MetaXRAudioSDK
{
    public class SceneSelector : MonoBehaviour
    {
        public string[] SceneNames;
        public string[] SceneDescriptions;

        public ToggleDeselect[] SceneToggles;

        public ToggleDeselect LaunchButton;

        public TextMeshProUGUI SceneDescriptionText;

        private UnityAction<bool>[] sceneToggleListeners;
        private int _currentSceneIndex = 0;

        private void Start()
        {
            sceneToggleListeners = new UnityAction<bool>[SceneToggles.Length];
            for (int n = 0; n < SceneToggles.Length; n++)
            {
                int index = n;
                sceneToggleListeners[n] = (bool on) =>
                {
                    if (on)
                    {
                        SceneSelected(index);
                    }
                };

                SceneToggles[n].onValueChanged.AddListener(sceneToggleListeners[n]);

                if (n == _currentSceneIndex)
                {
                    SceneToggles[n].isOn = true;
                }
            }

            LaunchButton.onValueChanged.AddListener(OnLaunchButtonValueChange);

            UpdateSceneDescription();
        }

        private void OnDestroy()
        {
            for (int n = 0; n < SceneToggles.Length; n++)
            {
                SceneToggles[n].onValueChanged.RemoveListener(sceneToggleListeners[n]);
            }
            sceneToggleListeners = null;

            LaunchButton.onValueChanged.RemoveListener(OnLaunchButtonValueChange);
        }

        private void SceneSelected(int index)
        {
            _currentSceneIndex = index;

            UpdateSceneDescription();
        }

        private void UpdateSceneDescription()
        {
            SceneDescriptionText.text = SceneDescriptions[_currentSceneIndex];
        }

        private void OnLaunchButtonValueChange(bool on)
        {
            if (on)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames[_currentSceneIndex]);
            }
        }
    }
}
