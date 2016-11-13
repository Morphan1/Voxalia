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
            return headMat * eye;
        }

        public Matrix4 GetProjection(bool lefteye, float znear, float zfar)
        {
            HmdMatrix44_t temp = VR.GetProjectionMatrix(lefteye ? EVREye.Eye_Left : EVREye.Eye_Right, znear, zfar, EGraphicsAPIConvention.API_DirectX);
            return new Matrix4(temp.m0, temp.m1, temp.m2, temp.m3, temp.m4, temp.m5, temp.m6, temp.m7, temp.m8, temp.m9, temp.m10, temp.m11, temp.m12, temp.m13, temp.m14, temp.m15);
        }

        public void Stop()
        {
            OpenVR.Shutdown();
        }

        Matrix4 headMat = Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ);

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
            resp = resp * Matrix4.CreateScale(1.5f);
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
                headMat.Invert();
                headMat = Matrix4.CreateRotationX((float)(Math.PI * -0.5)) * headMat * Matrix4.CreateScale(0.75f); // TODO: 1.5 -> Cvar?
            }
            if (merr != EVRCompositorError.None)
            {
                SysConsole.Output(OutputType.WARNING, "Posing error: " + merr);
            }
            Left = GetController(true);
            Right = GetController(false);
            SysConsole.Output(OutputType.DEBUG, "Left: " + Left);
            SysConsole.Output(OutputType.DEBUG, "Right: " + Right);
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
        public Matrix4 Position;

        public Vector2[] Axes = new Vector2[5];

        public VRButtons Touched;

        public VRButtons Pressed;

        public override string ToString()
        {
            return Position + ",,, " + Axes[0] + ", " + Axes[1] + ", " + Axes[2] + ", " + Axes[3] + ", " + Axes[4] + ", " + Touched + ",,, " + Pressed;
        }
    }

    [Flags]
    public enum VRButtons : ulong
    {
        NONE = 0,
        ONE = 1,
        TWO = 2,
        FOUR = 4,
        EIGHT = 8,
        SIXTEEN = 16,
        THIRTY_TWO = 32,
        SIXTY_FOUR = 64,
        ONE_TWENTY_EIGHT = 128,
        TWO_FIFTY_SIX = 256,
        FIVE_TWELVE = 512,
        TEN_TWENTY_FOUR = 1024,
        TWENTY_FOURTY_EIGHTY = 2048,
        FOURTY_NINETY_SIX = 4096,
        EIGHTY_ONE_NINETY_TWO = 8192
        // maybe that's enough?
    }
}
