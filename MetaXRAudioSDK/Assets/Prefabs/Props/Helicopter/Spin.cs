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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] bool _x = false;
    [SerializeField] bool _y = false;
    [SerializeField] bool _z = false;
    [SerializeField] float _speed = 10f;
    private float x, y, z;
    void Start()
    {
        if (_x)
        {
            x = 1;
        }
        else
        {
            x = 0;
        }
        if (_y)
        {
            y = 1;
        }
        else
        {
            y = 0;
        }
        if (_z)
        {
            z = 1;
        }
        else
        {
            z = 0;
        }
    }
    void Update()
    {
        this.transform.Rotate(x * Time.deltaTime * _speed, y * Time.deltaTime * _speed, z * Time.deltaTime * _speed);
    }
}
