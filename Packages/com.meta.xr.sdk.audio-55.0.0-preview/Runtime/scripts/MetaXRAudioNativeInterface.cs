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

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Meta.XR.Audio;

namespace Meta
{
    namespace XR
    {
        namespace Audio
        {
            /***********************************************************************************/
            // ENUMS and STRUCTS
            /***********************************************************************************/
            [Flags]
            public enum EnableFlag : uint
            {
                NONE = 0,
                SIMPLE_ROOM_MODELING = 2,
                LATE_REVERBERATION = 3,
                RANDOMIZE_REVERB = 4,
                PERFORMANCE_COUNTERS = 5,
            }
        }
    }
}

public class MetaXRAudioNativeInterface
{
    static NativeInterface CachedInterface;
    public static NativeInterface Interface { get { if (CachedInterface == null) CachedInterface = FindInterface(); return CachedInterface; } }

    static NativeInterface FindInterface()
    {
        IntPtr temp;
        try
        {
            temp = WwisePluginInterface.getOrCreateGlobalOvrAudioContext();
            Debug.Log("Meta XR Audio Native Interface initialized with Wwise plugin");
            return new WwisePluginInterface();
        }
        catch(System.DllNotFoundException)
        {
            // this is fine
        }
        try
        {
            FMODPluginInterface.ovrAudio_GetPluginContext(out temp, ClientType.OVRA_CLIENT_TYPE_FMOD);
            Debug.Log("Meta XR Audio Native Interface initialized with FMOD plugin");
            return new FMODPluginInterface();
        }
        catch (System.DllNotFoundException)
        {
            // this is fine
        }

        Debug.Log("Meta XR Audio Native Interface initialized with Unity plugin");
        return new UnityNativeInterface();
    }

    public enum ovrAudioScalarType : uint
    {
        Int8,
        UInt8,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Float16,
        Float32,
        Float64
    }

    public class ClientType
    {
        // Copied from AudioSDK\OVRAudio\OVR_Audio_Internal.h
        public const uint OVRA_CLIENT_TYPE_NATIVE = 0;
        public const uint OVRA_CLIENT_TYPE_WWISE_2016 = 1;
        public const uint OVRA_CLIENT_TYPE_WWISE_2017_1 = 2;
        public const uint OVRA_CLIENT_TYPE_WWISE_2017_2 = 3;
        public const uint OVRA_CLIENT_TYPE_WWISE_2018_1 = 4;
        public const uint OVRA_CLIENT_TYPE_FMOD = 5;
        public const uint OVRA_CLIENT_TYPE_UNITY = 6;
        public const uint OVRA_CLIENT_TYPE_UE4 = 7;
        public const uint OVRA_CLIENT_TYPE_VST = 8;
        public const uint OVRA_CLIENT_TYPE_AAX = 9;
        public const uint OVRA_CLIENT_TYPE_TEST = 10;
        public const uint OVRA_CLIENT_TYPE_OTHER = 11;
        public const uint OVRA_CLIENT_TYPE_WWISE_UNKNOWN = 12;
    }

    public interface NativeInterface
    {

        /***********************************************************************************/
        // Shoebox Reflections API
        int SetAdvancedBoxRoomParameters(float width, float height, float depth,
            bool lockToListenerPosition, Vector3 position, float[] wallMaterials);
        int SetRoomClutterFactor(float[] clutterFactor);
        int SetReflectionModel(int reflectionModel);
        int SetDynamicRoomRaysPerSecond(int RaysPerSecond);
        int SetDynamicRoomInterpSpeed(float InterpSpeed);
        int SetDynamicRoomMaxWallDistance(float MaxWallDistance);
        int SetDynamicRoomRaysRayCacheSize(int RayCacheSize);
        int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);
        int GetRaycastHits(Vector3[] points, Vector3[] normals, int length);
    }

    /***********************************************************************************/
    // UNITY NATIVE
    /***********************************************************************************/
    public class UnityNativeInterface : NativeInterface
    {
        // The name used for the plugin DLL.
        public const string binaryName = "MetaXRAudioUnity";

        /***********************************************************************************/
        // Context API: Required to create internal context if it does not exist yet
        IntPtr context_ = IntPtr.Zero;
        IntPtr context { get { if (context_ == IntPtr.Zero) { ovrAudio_GetPluginContext(out context_, ClientType.OVRA_CLIENT_TYPE_UNITY); } return context_; } }

        [DllImport(binaryName)]
        public static extern int ovrAudio_GetPluginContext(out IntPtr context, uint clientType);

    /***********************************************************************************/
        // Shoebox Reflections API
        [DllImport(binaryName)]
        private static extern int ovrAudio_SetAdvancedBoxRoomParametersUnity(IntPtr context, float width, float height, float depth,
            bool lockToListenerPosition, float positionX, float positionY, float positionZ,
            float[] wallMaterials);
        public int SetAdvancedBoxRoomParameters(float width, float height, float depth,
            bool lockToListenerPosition, Vector3 position, float[] wallMaterials)
        {
            return ovrAudio_SetAdvancedBoxRoomParametersUnity(context, width, height, depth,
                lockToListenerPosition, position.x, position.y, -position.z, wallMaterials);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetRoomClutterFactor(IntPtr context, float[] clutterFactor);
        public int SetRoomClutterFactor(float[] clutterFactor)
        {
            return ovrAudio_SetRoomClutterFactor(context, clutterFactor);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetReflectionModel(IntPtr context, int model);
        public int SetReflectionModel(int model)
        {
            return ovrAudio_SetReflectionModel(context, model);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);
        public int SetEnabled(int feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, EnableFlag what, int enable);
        public int SetEnabled(EnableFlag feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysPerSecond(IntPtr context, int RaysPerSecond);
        public int SetDynamicRoomRaysPerSecond(int RaysPerSecond)
        {
            return ovrAudio_SetDynamicRoomRaysPerSecond(context, RaysPerSecond);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomInterpSpeed(IntPtr context, float InterpSpeed);
        public int SetDynamicRoomInterpSpeed(float InterpSpeed)
        {
            return ovrAudio_SetDynamicRoomInterpSpeed(context, InterpSpeed);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomMaxWallDistance(IntPtr context, float MaxWallDistance);
        public int SetDynamicRoomMaxWallDistance(float MaxWallDistance)
        {
            return ovrAudio_SetDynamicRoomMaxWallDistance(context, MaxWallDistance);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysRayCacheSize(IntPtr context, int RayCacheSize);
        public int SetDynamicRoomRaysRayCacheSize(int RayCacheSize)
        {
            return ovrAudio_SetDynamicRoomRaysRayCacheSize(context, RayCacheSize);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRoomDimensions(IntPtr context, float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);
        public int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position)
        {
            return ovrAudio_GetRoomDimensions(context, roomDimensions, reflectionsCoefs, out position);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRaycastHits(IntPtr context, Vector3[] points, Vector3[] normals, int length);
        public int GetRaycastHits(Vector3[] points, Vector3[] normals, int length)
        {
            return ovrAudio_GetRaycastHits(context, points, normals, length);
        }
    }

    /***********************************************************************************/
    // WWISE
    /***********************************************************************************/
    public class WwisePluginInterface : NativeInterface
    {
#if true // Wwise Object Endpoint Sink Plugin
        // The name used for the plugin DLL.
        public const string binaryName = "MetaXRAudioWwise";
        /***********************************************************************************/
        // Context API: Required to create internal context if it does not exist yet
        IntPtr context_ = IntPtr.Zero;

        IntPtr context
        {
            get
            {
                if (context_ == IntPtr.Zero)
                {
                    context_ = getOrCreateGlobalOvrAudioContext();
                }
                return context_;
            }
        }

        [DllImport(binaryName)]
        public static extern IntPtr getOrCreateGlobalOvrAudioContext();

#else // Oculus Spatializer Wwise Mixer Plugin
        public const string binaryName = "OculusEndpoint";
        /***********************************************************************************/
        // Context API: Required to create internal context if it does not exist yet
        IntPtr context_ = IntPtr.Zero;
        IntPtr context { get { if (context_ == IntPtr.Zero) { ovrAudio_GetPluginContext(out context_, ClientType.OVRA_CLIENT_TYPE_WWISE_UNKNOWN); } return context_; } }

        [DllImport(binaryName)]
        public static extern int ovrAudio_GetPluginContext(out IntPtr context, uint clientType);
#endif
        /***********************************************************************************/
        // Shoebox Reflections API
        [DllImport(binaryName)]
        private static extern int ovrAudio_SetAdvancedBoxRoomParametersUnity(IntPtr context, float width, float height, float depth,
            bool lockToListenerPosition, float positionX, float positionY, float positionZ,
            float[] wallMaterials);
        public int SetAdvancedBoxRoomParameters(float width, float height, float depth,
            bool lockToListenerPosition, Vector3 position, float[] wallMaterials)
        {
            return ovrAudio_SetAdvancedBoxRoomParametersUnity(context, width, height, depth,
                lockToListenerPosition, position.x, position.y, -position.z, wallMaterials);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetRoomClutterFactor(IntPtr context, float[] clutterFactor);
        public int SetRoomClutterFactor(float[] clutterFactor)
        {
            return ovrAudio_SetRoomClutterFactor(context, clutterFactor);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetReflectionModel(IntPtr context, int model);

        public int SetReflectionModel(int model)
        {
            return ovrAudio_SetReflectionModel(context, model);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);
        public int SetEnabled(int feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, EnableFlag what, int enable);
        public int SetEnabled(EnableFlag feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysPerSecond(IntPtr context, int RaysPerSecond);
        public int SetDynamicRoomRaysPerSecond(int RaysPerSecond)
        {
            return ovrAudio_SetDynamicRoomRaysPerSecond(context, RaysPerSecond);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomInterpSpeed(IntPtr context, float InterpSpeed);
        public int SetDynamicRoomInterpSpeed(float InterpSpeed)
        {
            return ovrAudio_SetDynamicRoomInterpSpeed(context, InterpSpeed);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomMaxWallDistance(IntPtr context, float MaxWallDistance);
        public int SetDynamicRoomMaxWallDistance(float MaxWallDistance)
        {
            return ovrAudio_SetDynamicRoomMaxWallDistance(context, MaxWallDistance);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysRayCacheSize(IntPtr context, int RayCacheSize);
        public int SetDynamicRoomRaysRayCacheSize(int RayCacheSize)
        {
            return ovrAudio_SetDynamicRoomRaysRayCacheSize(context, RayCacheSize);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRoomDimensions(IntPtr context, float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);
        public int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position)
        {
            return ovrAudio_GetRoomDimensions(context, roomDimensions, reflectionsCoefs, out position);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRaycastHits(IntPtr context, Vector3[] points, Vector3[] normals, int length);
        public int GetRaycastHits(Vector3[] points, Vector3[] normals, int length)
        {
            return ovrAudio_GetRaycastHits(context, points, normals, length);
        }
    }

    /***********************************************************************************/
    // FMOD
    /***********************************************************************************/
    public class FMODPluginInterface : NativeInterface
    {
        // The name used for the plugin DLL.
        public const string binaryName = "MetaXRAudioFMOD";

        /***********************************************************************************/
        // Context API: Required to create internal context if it does not exist yet
        IntPtr context_ = IntPtr.Zero;
        IntPtr context { get { if (context_ == IntPtr.Zero) { ovrAudio_GetPluginContext(out context_, ClientType.OVRA_CLIENT_TYPE_FMOD); } return context_; } }

        [DllImport(binaryName)]
        public static extern int ovrAudio_GetPluginContext(out IntPtr context, uint clientType);

        /***********************************************************************************/
        // Shoebox Reflections API
        [DllImport(binaryName)]
        private static extern int ovrAudio_SetAdvancedBoxRoomParametersUnity(IntPtr context, float width, float height, float depth,
            bool lockToListenerPosition, float positionX, float positionY, float positionZ,
            float[] wallMaterials);
        public int SetAdvancedBoxRoomParameters(float width, float height, float depth,
            bool lockToListenerPosition, Vector3 position, float[] wallMaterials)
        {
            return ovrAudio_SetAdvancedBoxRoomParametersUnity(context, width, height, depth,
                lockToListenerPosition, position.x, position.y, -position.z, wallMaterials);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetRoomClutterFactor(IntPtr context, float[] clutterFactor);
        public int SetRoomClutterFactor(float[] clutterFactor)
        {
            return ovrAudio_SetRoomClutterFactor(context, clutterFactor);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetReflectionModel(IntPtr context, int model);

        public int SetReflectionModel(int model)
        {
            return ovrAudio_SetReflectionModel(context, model);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);
        public int SetEnabled(int feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, EnableFlag what, int enable);
        public int SetEnabled(EnableFlag feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysPerSecond(IntPtr context, int RaysPerSecond);
        public int SetDynamicRoomRaysPerSecond(int RaysPerSecond)
        {
            return ovrAudio_SetDynamicRoomRaysPerSecond(context, RaysPerSecond);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomInterpSpeed(IntPtr context, float InterpSpeed);
        public int SetDynamicRoomInterpSpeed(float InterpSpeed)
        {
            return ovrAudio_SetDynamicRoomInterpSpeed(context, InterpSpeed);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomMaxWallDistance(IntPtr context, float MaxWallDistance);
        public int SetDynamicRoomMaxWallDistance(float MaxWallDistance)
        {
            return ovrAudio_SetDynamicRoomMaxWallDistance(context, MaxWallDistance);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysRayCacheSize(IntPtr context, int RayCacheSize);
        public int SetDynamicRoomRaysRayCacheSize(int RayCacheSize)
        {
            return ovrAudio_SetDynamicRoomRaysRayCacheSize(context, RayCacheSize);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRoomDimensions(IntPtr context, float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);
        public int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position)
        {
            return ovrAudio_GetRoomDimensions(context, roomDimensions, reflectionsCoefs, out position);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRaycastHits(IntPtr context, Vector3[] points, Vector3[] normals, int length);
        public int GetRaycastHits(Vector3[] points, Vector3[] normals, int length)
        {
            return ovrAudio_GetRaycastHits(context, points, normals, length);
        }
    }
}
