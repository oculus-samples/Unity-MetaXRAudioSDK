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

using UnityEngine;

public class LookAtHeadSet : MonoBehaviour
{
    [SerializeField] bool _active = true;
    Camera _camera;
    [SerializeField] bool _lerp = true;
    [SerializeField] float _damping = 2f;
    private Quaternion _initialLocalRotation;
    void Start()
    {
        _initialLocalRotation = this.transform.localRotation;
        _camera = Camera.main;
        if (_camera)
        {
            _camera = FindFirstObjectByType<Camera>();
            if(_camera && _active){
                this.transform.LookAt(_camera.transform.position,Vector3.up);
            }
        }
    }
    void Update()
    {
        if (_camera && _active)
        {
            if(_lerp){
                Quaternion lookAtQ = Quaternion.LookRotation(_camera.transform.position - this.transform.position);
                this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lookAtQ, _damping * Time.deltaTime);
            }else{
                this.transform.LookAt(_camera.transform.position,Vector3.up);
            }
        }else{
            this.transform.localRotation = _initialLocalRotation;
        }
    }
}
