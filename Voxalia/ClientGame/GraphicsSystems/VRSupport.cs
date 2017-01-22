//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Valve.VR;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK;

namespace Voxalia.ClientGame.GraphicsSystems
{
    public class VRSupport
    {
        public CVRSystem VR = null;

        public Client TheClient;

        public CVRCompositor Compositor;

        public static bool Available()
        {
            return OpenVR.IsHmdPresent();
        }

        public static VRSupport TryInit(Client tclient)
        {
            if (!Available())
            {
                return null;
            }
            EVRInitError err = EVRInitError.None;
            VRSupport vrs = new VRSupport();
            vrs.TheClient = tclient;
            vrs.VR = OpenVR.Init(ref err);
            if (err != EVRInitError.None)
            {
                SysConsole.Output(OutputType.INFO, "VR error: " + err + ": " + OpenVR.GetStringForHmdError(err));
                return null;
            }
            vrs.Start();
            return vrs;
        }

        public void Start()
        {
            uint w = 0;
            uint h = 0;
            VR.GetRecommendedRenderTargetSize(ref w, ref h);
            if (w <= 0 || h <= 0)
            {
                throw new Exception("Failed to start VR: Invalid render target size!");
            }
            TheClient.MainWorldView.Generate(TheClient, (int)w, (int)h);
            TheClient.MainWorldView.GenerateFBO();
            SysConsole.Output(OutputType.INFO, "Switching to VR mode!");
            Compositor = OpenVR.Compositor;
            Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseStanding);
            Compositor.CompositorBringToFront();
        }

        public Matrix4 Eye(bool lefteye)
        {
            HmdMatrix34_t temp = VR.GetEyeToHeadTransform(lefteye ? EVREye.Eye_Left : EVREye.Eye_Right);
            Matrix4 eye = new Matrix4(temp.m0, temp.m1, temp.m2, temp.m3, temp.m4, temp.m5, temp.m6, temp.m7, temp.m8, temp.m9, temp.m10, temp.m11, 0, 0, 0, 1);
            eye.Transpose();
            eye = eye.ClearTranslation() * Matrix4.CreateTranslation(eye.ExtractTranslation() * (1.5f * TheClient.CVars.r_vrscale.ValueF));
            return headMat * eye;
        }

        public Matrix4 GetProjection(bool lefteye, float znear, float zfar)
        {
            HmdMatrix44_t temp = VR.GetProjectionMatrix(!lefteye ? EVREye.Eye_Left : EVREye.Eye_Right, znear, zfar, EGraphicsAPIConvention.API_OpenGL);
            Matrix4 proj = new Matrix4(temp.m0, temp.m1, temp.m2, temp.m3, temp.m4, temp.m5, temp.m6, temp.m7, temp.m8, temp.m9, temp.m10, temp.m11, temp.m12, temp.m13, temp.m14, temp.m15);
            proj.Transpose();
            return proj;
        }

        public void Stop()
        {
            OpenVR.Shutdown();
        }

        public Matrix4 HeadMatRot = Matrix4.Identity;
        
        public Matrix4 headMat = Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ);

        public VRController GetController(bool left)
        {
            VRControllerState_t vrcont = new VRControllerState_t();
            TrackedDevicePose_t vrpose = new TrackedDevicePose_t();
            
            bool valid = VR.GetControllerStateWithPose(ETrackingUniverseOrigin.TrackingUniverseStanding, VR.GetTrackedDeviceIndexForControllerRole(left ? ETrackedControllerRole.LeftHand : ETrackedControllerRole.RightHand), ref vrcont, ref vrpose);
            if (!valid || !vrpose.bPoseIsValid)
            {
                return null;
            }
            HmdMatrix34_t tmat = vrpose.mDeviceToAbsoluteTracking;
            Matrix4 resp = new Matrix4(tmat.m0, tmat.m1, tmat.m2, tmat.m3, tmat.m4, tmat.m5, tmat.m6, tmat.m7, tmat.m8, tmat.m9, tmat.m10, tmat.m11, 0, 0, 0, 1);
            resp.Transpose();
            resp = resp.ClearTranslation() * Matrix4.CreateTranslation(resp.ExtractTranslation() * (1.5f * TheClient.CVars.r_vrscale.ValueF));
            resp = resp * Matrix4.CreateRotationX((float)(Math.PI * 0.5));
            VRController res = new VRController();
            res.Position = resp;
            res.Axes[0] = new Vector2(vrcont.rAxis0.x, vrcont.rAxis0.y);
            res.Axes[1] = new Vector2(vrcont.rAxis1.x, vrcont.rAxis1.y);
            res.Axes[2] = new Vector2(vrcont.rAxis2.x, vrcont.rAxis2.y);
            res.Axes[3] = new Vector2(vrcont.rAxis3.x, vrcont.rAxis3.y);
            res.Axes[4] = new Vector2(vrcont.rAxis4.x, vrcont.rAxis4.y);
            res.Touched = (VRButtons)vrcont.ulButtonTouched;
            res.Pressed = (VRButtons)vrcont.ulButtonPressed;
            return res;
        }

        public VRController Left;

        public VRController Right;

        public void Submit()
        {
            VREvent_t evt = new VREvent_t();
            while (VR.PollNextEvent(ref evt, (uint)Marshal.SizeOf(typeof(VREvent_t))))
            {
                // No need to do anything here!
            }
            TrackedDevicePose_t[] rposes = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            TrackedDevicePose_t[] gposes = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            EVRCompositorError merr = Compositor.WaitGetPoses(rposes, gposes);
            if (rposes[OpenVR.k_unTrackedDeviceIndex_Hmd].bPoseIsValid)
            {
                HmdMatrix34_t tmat = rposes[OpenVR.k_unTrackedDeviceIndex_Hmd].mDeviceToAbsoluteTracking;
                headMat = new Matrix4(tmat.m0, tmat.m1, tmat.m2, tmat.m3, tmat.m4, tmat.m5, tmat.m6, tmat.m7, tmat.m8, tmat.m9, tmat.m10, tmat.m11, 0, 0, 0, 1);
                headMat.Transpose();
                HeadMatRot = headMat * Matrix4.CreateRotationX((float)(Math.PI * 0.5));
                headMat = headMat * Matrix4.CreateRotationX((float)(Math.PI * 0.5));
                headMat = headMat.ClearTranslation() * Matrix4.CreateTranslation(headMat.ExtractTranslation() * (1.5f * TheClient.CVars.r_vrscale.ValueF));
                headMat.Invert();
            }
            if (merr != EVRCompositorError.None)
            {
                SysConsole.Output(OutputType.WARNING, "Posing error: " + merr);
            }
            Left = GetController(true);
            Right = GetController(false);
            if (!Compositor.CanRenderScene())
            {
                SysConsole.Output(OutputType.WARNING, "Can't render VR scene!");
            }
            Texture_t left = new Texture_t();
            left.eColorSpace = EColorSpace.Auto;
            left.eType = EGraphicsAPIConvention.API_OpenGL;
            left.handle = new IntPtr(TheClient.MainWorldView.CurrentFBOTexture);
            VRTextureBounds_t bounds = new VRTextureBounds_t();
            bounds.uMin = 0f;
            bounds.uMax = 0.5f;
            bounds.vMin = 0f;
            bounds.vMax = 1f;
            EVRCompositorError lerr = Compositor.Submit(EVREye.Eye_Left, ref left, ref bounds, EVRSubmitFlags.Submit_Default);
            if (lerr != EVRCompositorError.None)
            {
                SysConsole.Output(OutputType.WARNING, "Left eye error: " + lerr);
            }
            Texture_t right = new Texture_t();
            right.eColorSpace = EColorSpace.Auto;
            right.eType = EGraphicsAPIConvention.API_OpenGL;
            right.handle = new IntPtr(TheClient.MainWorldView.CurrentFBOTexture);
            VRTextureBounds_t rbounds = new VRTextureBounds_t();
            rbounds.uMin = 0.5f;
            rbounds.uMax = 1f;
            rbounds.vMin = 0f;
            rbounds.vMax = 1f;
            EVRCompositorError rerr = Compositor.Submit(EVREye.Eye_Right, ref right, ref rbounds, EVRSubmitFlags.Submit_Default);
            if (rerr != EVRCompositorError.None)
            {
                SysConsole.Output(OutputType.WARNING, "Right eye error: " + rerr);
            }
        }
    }

    public class VRController
    {
        public const int AXIS_TRACKPAD = 0;

        public const int AXIS_TRIGGER = 1;

        public Vector2 TrackPad
        {
            get
            {
                return Axes[AXIS_TRACKPAD];
            }
        }

        public float Trigger
        {
            get
            {
                return Axes[AXIS_TRIGGER].X;
            }
        }

        public Matrix4 Position;

        public Vector2[] Axes = new Vector2[5];

        public VRButtons Touched;

        public VRButtons Pressed;

        public override string ToString()
        {
            return Axes[0] + ", " + Axes[1] + ", " + Axes[2] + ", " + Axes[3] + ", " + Axes[4] + ", " + Touched + ",,, " + Pressed;
        }
    }

    [Flags]
    public enum VRButtons : ulong
    {
        /// <summary>
        /// No buttons down.
        /// </summary>
        NONE = 0,
        __A__ONE = 1,
        /// <summary>
        /// The menu button.
        /// </summary>
        MENU_BUTTON = 2,
        /// <summary>
        /// The side grip.
        /// </summary>
        SIDE_GRIP = 4,
        __A__EIGHT = 8,
        __A__SIXTEEN = 16,
        __A__THIRTY_TWO = 32,
        __A__SIXTY_FOUR = 64,
        __A__ONE_TWENTY_EIGHT = 128,
        __A__TWO_FIFTY_SIX = 256,
        __A__FIVE_TWELVE = 512,
        __A__TEN_TWENTY_FOUR = 1024,
        __A__TWENTY_FOURTY_EIGHTY = 2048,
        __A__FOURTY_NINETY_SIX = 4096,
        __A__EIGHTY_ONE_NINETY_TWO = 8192,
        __N__ONE = 1 * 16384,
        __N__TWO = 2 * 16384,
        __N__FOUR = 4 * 16384,
        __N__EIGHT = 8 * 16384,
        __N__SIXTEEN = 16 * 16384,
        __N__THIRTY_TWO = 32 * 16384,
        __N__SIXTY_FOUR = 64 * 16384,
        __N__ONE_TWENTY_EIGHT = 128 * 16384,
        __N__TWO_FIFTY_SIX = 256 * 16384,
        __N__FIVE_TWELVE = 512 * 16384,
        __N__TEN_TWENTY_FOUR = 1024 * 16384,
        __N__TWENTY_FOURTY_EIGHTY = 2048 * 16384,
        __N__FOURTY_NINETY_SIX = 4096 * 16384,
        __N__EIGHTY_ONE_NINETY_TWO = 8192 * 16384,
        __U__ONE = 1 * 16384 * 16384ul,
        __U__TWO = 2 * 16384 * 16384ul,
        __U__FOUR = 4 * 16384 * 16384ul,
        __U__EIGHT = 8 * 16384 * 16384ul,
        /// <summary>
        /// The track pad.
        /// </summary>
        TRACKPAD = 16 * 16384 * 16384ul,
        /// <summary>
        /// The trigger.
        /// </summary>
        TRIGGER = 32 * 16384 * 16384ul,
        __U__SIXTY_FOUR = 64 * 16384 * 16384ul,
        __U__ONE_TWENTY_EIGHT = 128 * 16384 * 16384ul,
        __U__TWO_FIFTY_SIX = 256 * 16384 * 16384ul,
        __U__FIVE_TWELVE = 512 * 16384 * 16384ul,
        __U__TEN_TWENTY_FOUR = 1024 * 16384 * 16384ul,
        __U__TWENTY_FOURTY_EIGHTY = 2048 * 16384 * 16384ul,
        __U__FOURTY_NINETY_SIX = 4096 * 16384 * 16384ul,
        __U__EIGHTY_ONE_NINETY_TWO = 8192 * 16384 * 16384ul,
        __V__ONE = 1 * 16384 * 16384ul * 16384ul,
        __V__TWO = 2 * 16384 * 16384ul * 16384ul,
        __V__FOUR = 4 * 16384 * 16384ul * 16384ul,
        __V__EIGHT = 8 * 16384 * 16384ul * 16384ul,
        __V__SIXTEEN = 16 * 16384 * 16384ul * 16384ul,
        __V__THIRTY_TWO = 32 * 16384 * 16384ul * 16384ul,
        __V__SIXTY_FOUR = 64 * 16384 * 16384ul * 16384ul,
        __V__ONE_TWENTY_EIGHT = 128 * 16384 * 16384ul * 16384ul,
        __V__TWO_FIFTY_SIX = 256 * 16384 * 16384ul * 16384ul,
        __V__FIVE_TWELVE = 512 * 16384 * 16384ul * 16384ul,
        __V__TEN_TWENTY_FOUR = 1024 * 16384 * 16384ul * 16384ul,
        __V__TWENTY_FOURTY_EIGHTY = 2048 * 16384 * 16384ul * 16384ul,
        __V__FOURTY_NINETY_SIX = 4096 * 16384 * 16384ul * 16384ul,
        __V__EIGHTY_ONE_NINETY_TWO = 8192 * 16384 * 16384ul * 16384ul,
        __Z__ONE = 1 * 16384 * 16384ul * 16384ul * 16384ul,
        __Z__TWO = 2 * 16384 * 16384ul * 16384ul * 16384ul,
        __Z__FOUR = 4 * 16384 * 16384ul * 16384ul * 16384ul,
        __Z__EIGHT = 8 * 16384 * 16384ul * 16384ul * 16384ul,
        __Z__SIXTEEN = 16 * 16384 * 16384ul * 16384ul * 16384ul,
        __Z__THIRTY_TWO = 32 * 16384 * 16384ul * 16384ul * 16384ul,
        __Z__SIXTY_FOUR = 64 * 16384 * 16384ul * 16384ul * 16384ul,
        __Z__ONE_TWENTY_EIGHT = 128 * 16384 * 16384ul * 16384ul * 16384ul
    }
}
