﻿using System.Diagnostics;
using SharpDX.Direct3D;
using VRageMath;

namespace VRageRender
{
    class MyBloom : MyImmediateRC
    {
        static ComputeShaderId m_bloomShader;
        static ComputeShaderId m_downscaleShader;
        static ComputeShaderId m_blurH;
        static ComputeShaderId m_blurV;

        const int m_numthreads = 8;

        internal static void Init()
        {
            var threadMacro = new[] { new ShaderMacro("NUMTHREADS", 8) };
            //MyRender11.RegisterSettingsChangedListener(new OnSettingsChangedDelegate(RecreateShadersForSettings));
            m_bloomShader = MyShaders.CreateCs("bloom_init.hlsl",threadMacro);
            m_downscaleShader = MyShaders.CreateCs("bloom_downscale.hlsl", threadMacro);
            m_blurH = MyShaders.CreateCs("bloom_blur_h.hlsl", threadMacro);
            m_blurV = MyShaders.CreateCs("bloom_blur_v.hlsl", threadMacro);
        }

        internal static ConstantsBufferId GetCB_blurH(MyStereoRegion region, Vector3I uavSize)
        {
            int offX = 0;
            int maxX = uavSize.X - 1;
            if (region == MyStereoRegion.LEFT)
                maxX = uavSize.X / 2 - 1;
            else if (region == MyStereoRegion.RIGHT)
            {
                offX = uavSize.X / 2;
                maxX = uavSize.X / 2 - 1;
            }

            var buffer = MyCommon.GetObjectCB(8);
            var mapping = MyMapping.MapDiscard(buffer);
            mapping.WriteAndPosition(ref offX);
            mapping.WriteAndPosition(ref maxX);
            mapping.Unmap();
            return buffer;
        }

        internal static MyBindableResource Run(MyBindableResource src, MyBindableResource avgLum)
        {
            RC.CSSetCB(MyCommon.FRAME_SLOT, MyCommon.FrameConstants);

            RC.BindUAV(0, MyRender11.HalfScreenUavHDR);
            RC.BindSRVs(0, src, avgLum);

            RC.SetCS(m_bloomShader);

            var size = MyRender11.HalfScreenUavHDR.GetSize();
            VRageMath.Vector2I threadGroups = new VRageMath.Vector2I((size.X + m_numthreads - 1) / m_numthreads, (size.Y + m_numthreads - 1) / m_numthreads);
            RC.DeviceContext.Dispatch(threadGroups.X, threadGroups.Y, 1);

            RC.SetCS(m_downscaleShader);

            size = MyRender11.QuarterScreenUavHDR.GetSize();
            threadGroups = new VRageMath.Vector2I((size.X + m_numthreads - 1) / m_numthreads, (size.Y + m_numthreads - 1) / m_numthreads);
            RC.BindUAV(0, MyRender11.QuarterScreenUavHDR);
            RC.BindSRV(0, MyRender11.HalfScreenUavHDR);
            RC.DeviceContext.Dispatch(threadGroups.X, threadGroups.Y, 1);

            size = MyRender11.EighthScreenUavHDR.GetSize();
            threadGroups = new VRageMath.Vector2I((size.X + m_numthreads - 1) / m_numthreads, (size.Y + m_numthreads - 1) / m_numthreads);
            RC.BindUAV(0, MyRender11.EighthScreenUavHDR);
            RC.BindSRV(0, MyRender11.QuarterScreenUavHDR);
            RC.DeviceContext.Dispatch(threadGroups.X, threadGroups.Y, 1);

            RC.SetCS(m_blurV);
            RC.BindUAV(0, MyRender11.EighthScreenUavHDRHelper);
            RC.BindSRV(0, MyRender11.EighthScreenUavHDR); 
            RC.DeviceContext.Dispatch(threadGroups.X, threadGroups.Y, 1);

            RC.SetCS(m_blurH);
            RC.BindUAV(0, MyRender11.EighthScreenUavHDR);
            RC.BindSRV(0, MyRender11.EighthScreenUavHDRHelper);

            int nPasses = 1;
            if (MyStereoRender.Enable)
            {
                threadGroups.X /= 2;
                nPasses = 2;
            }
            for (int nPass = 0; nPass < nPasses; nPass++)
            {
                MyStereoRegion region = MyStereoRegion.FULLSCREEN;
                if (MyStereoRender.Enable)
                    region = nPass == 0 ? MyStereoRegion.LEFT : MyStereoRegion.RIGHT;
                RC.CSSetCB(1, GetCB_blurH(region, size));
                RC.DeviceContext.Dispatch(threadGroups.X, threadGroups.Y, 1);
            }

            return MyRender11.EighthScreenUavHDR;
        }

    }
}
