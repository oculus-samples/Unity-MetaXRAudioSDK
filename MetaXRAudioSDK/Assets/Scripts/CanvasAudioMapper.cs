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
using UnityEngine;

namespace MetaXRAudioSDK
{
    [RequireComponent(typeof(PointableCanvas))]
    public class CanvasAudioMapper : MonoBehaviour
    {
        private UIAudio _uiAudio;

        [SerializeField, Interface(typeof(IPointableCanvas))]
        private UnityEngine.Object _pointableCanvas;
        private IPointableCanvas PointableCanvas;

        [SerializeField, Tooltip("Selection and hover events will not be fired while dragging.")]
        private bool _suppressWhileDragging = true;

        protected bool _started = false;

        private bool ShouldFireEvent(PointableCanvasEventArgs args)
        {
            if (args.Canvas != PointableCanvas.Canvas)
            {
                return false;
            }
            if (_suppressWhileDragging && args.Dragging)
            {
                return false;
            }
            return true;
        }

        private void PointableCanvasModule_WhenSelectableHoverEnter(PointableCanvasEventArgs args)
        {
            if (ShouldFireEvent(args))
            {
                _uiAudio.PlayHoverSound(args.Canvas.transform.position);
            }
        }

        private void PointableCanvasModule_WhenSelectableSelected(PointableCanvasEventArgs args)
        {
            if (ShouldFireEvent(args))
            {
                if (args.Hovered == null)
                    _uiAudio.PlayPressSound(args.Canvas.transform.position);
                else
                    _uiAudio.PlayPressSound(args.Canvas.transform.position);
            }
        }

        private void PointableCanvasModule_WhenSelectableUnselected(PointableCanvasEventArgs args)
        {
            if (ShouldFireEvent(args))
            {
                if (args.Hovered != null)
                    _uiAudio.PlayReleaseSound(args.Canvas.transform.position);
            }
        }

        protected virtual void Awake()
        {
            PointableCanvas = _pointableCanvas as IPointableCanvas;
        }

        protected virtual void Start()
        {
            _uiAudio = FindObjectOfType<UIAudio>();

            this.BeginStart(ref _started);
            this.AssertField(PointableCanvas, nameof(PointableCanvas));
            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                PointableCanvasModule.WhenSelectableHovered += PointableCanvasModule_WhenSelectableHoverEnter;
                PointableCanvasModule.WhenSelected += PointableCanvasModule_WhenSelectableSelected;
                PointableCanvasModule.WhenUnselected += PointableCanvasModule_WhenSelectableUnselected;
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                PointableCanvasModule.WhenSelectableHovered -= PointableCanvasModule_WhenSelectableHoverEnter;
                PointableCanvasModule.WhenSelected -= PointableCanvasModule_WhenSelectableSelected;
                PointableCanvasModule.WhenUnselected -= PointableCanvasModule_WhenSelectableUnselected;
            }
        }
    }
}
