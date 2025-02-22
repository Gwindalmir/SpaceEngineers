﻿#region Using

using System;
using System.Text;
using Havok;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Gui;
using Sandbox.Game.Localization;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI;
using VRage;
using VRage.Game;
using VRage.Utils;
using VRageMath;

#endregion

namespace SpaceEngineers.Game.Entities.Blocks
{
    [MyCubeBlockType(typeof(MyObjectBuilder_GravityGenerator))]
    public class MyGravityGenerator : MyGravityGeneratorBase, IMyGravityGenerator
    {
        private const int NUM_DECIMALS = 0;
        private new MyGravityGeneratorDefinition BlockDefinition
        {
            get { return (MyGravityGeneratorDefinition)base.BlockDefinition; }
        }

        private BoundingBox m_gizmoBoundingBox = new BoundingBox();

        private readonly Sync<Vector3> m_fieldSize;
        public Vector3 FieldSize
        {
            get { return m_fieldSize; }
            set
            {

                m_fieldSize.Value = value;           
            }
        }
        
        public override BoundingBox? GetBoundingBox()  
        {
            m_gizmoBoundingBox.Min = PositionComp.LocalVolume.Center - FieldSize / 2.0f;
            m_gizmoBoundingBox.Max = PositionComp.LocalVolume.Center + FieldSize / 2.0f;
            return m_gizmoBoundingBox;
        }

        public MyGravityGenerator()
        {
#if XB1 // XB1_SYNC_NOREFLECTION
            m_fieldSize = SyncType.CreateAndAddProp<Vector3>();
#endif // XB1
            CreateTerminalControls();

            m_fieldSize.ValueChanged += (x) => UpdateFieldShape();
        }

        static void CreateTerminalControls()
        {
            if (MyTerminalControlFactory.AreControlsCreated<MyGravityGenerator>())
                return;

            var fieldWidth = new MyTerminalControlSlider<MyGravityGenerator>("Width", MySpaceTexts.BlockPropertyTitle_GravityFieldWidth, MySpaceTexts.BlockPropertyDescription_GravityFieldWidth);
            fieldWidth.SetLimits(1, 150);
            fieldWidth.DefaultValue = 150;
            fieldWidth.Getter = (x) => x.m_fieldSize.Value.X;
            fieldWidth.Setter = (x, v) =>
            {
                Vector3 fieldSize = x.m_fieldSize;
                fieldSize.X = v;
                x.m_fieldSize.Value = fieldSize;
            };
            fieldWidth.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.m_fieldSize.Value.X, NUM_DECIMALS)).Append(" m");
            fieldWidth.EnableActions();
            MyTerminalControlFactory.AddControl(fieldWidth);

            var fieldHeight = new MyTerminalControlSlider<MyGravityGenerator>("Height", MySpaceTexts.BlockPropertyTitle_GravityFieldHeight, MySpaceTexts.BlockPropertyDescription_GravityFieldHeight);
            fieldHeight.SetLimits(1, 150);
            fieldHeight.DefaultValue = 150;
            fieldHeight.Getter = (x) => x.m_fieldSize.Value.Y;
            fieldHeight.Setter = (x, v) =>
            {
                Vector3 fieldSize = x.m_fieldSize;
                fieldSize.Y = v;
                x.m_fieldSize.Value = fieldSize;
            };
            fieldHeight.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.m_fieldSize.Value.Y, NUM_DECIMALS)).Append(" m");

            fieldHeight.EnableActions();
            MyTerminalControlFactory.AddControl(fieldHeight);

            var fieldDepth = new MyTerminalControlSlider<MyGravityGenerator>("Depth", MySpaceTexts.BlockPropertyTitle_GravityFieldDepth, MySpaceTexts.BlockPropertyDescription_GravityFieldDepth);
            fieldDepth.SetLimits(1, 150);
            fieldDepth.DefaultValue = 150;
            fieldDepth.Getter = (x) => x.m_fieldSize.Value.Z;
            fieldDepth.Setter = (x, v) =>
            {
                Vector3 fieldSize = x.m_fieldSize;
                fieldSize.Z = v;
                x.m_fieldSize.Value = fieldSize;
            };
            fieldDepth.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.m_fieldSize.Value.Z, NUM_DECIMALS)).Append(" m");
            fieldDepth.EnableActions();
            MyTerminalControlFactory.AddControl(fieldDepth);

            var gravityAcceleration = new MyTerminalControlSlider<MyGravityGenerator>("Gravity", MySpaceTexts.BlockPropertyTitle_GravityAcceleration, MySpaceTexts.BlockPropertyDescription_GravityAcceleration);
            gravityAcceleration.SetLimits(-MyGravityProviderSystem.G, MyGravityProviderSystem.G);
			gravityAcceleration.DefaultValue = MyGravityProviderSystem.G;
            gravityAcceleration.Getter = (x) => x.GravityAcceleration;
            gravityAcceleration.Setter = (x, v) => x.GravityAcceleration = v;
            gravityAcceleration.Writer = (x, result) => result.AppendDecimal(x.m_gravityAcceleration / MyGravityProviderSystem.G, 2).Append(" G");
            gravityAcceleration.EnableActions();
            MyTerminalControlFactory.AddControl(gravityAcceleration);
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);

            var builder = (MyObjectBuilder_GravityGenerator)objectBuilder;
            m_fieldSize.Value = builder.FieldSize;
            m_gravityAcceleration.Value = builder.GravityAcceleration;
        }

	    protected override void InitializeSinkComponent()
	    {
			var sinkComp = new MyResourceSinkComponent();
			sinkComp.Init(
				BlockDefinition.ResourceSinkGroup,
				BlockDefinition.RequiredPowerInput,
				CalculateRequiredPowerInput);
			ResourceSink = sinkComp;

			if (CubeGrid.CreatePhysics)
			{
				ResourceSink.IsPoweredChanged += Receiver_IsPoweredChanged;
				ResourceSink.RequiredInputChanged += Receiver_RequiredInputChanged;
				AddDebugRenderComponent(new MyDebugRenderComponentDrawPowerReciever(ResourceSink, this));
			}

	    }

	    public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            var builder = (MyObjectBuilder_GravityGenerator)base.GetObjectBuilderCubeBlock(copy);

            builder.FieldSize = m_fieldSize.Value;
            builder.GravityAcceleration = m_gravityAcceleration;

            return builder;
        }

      
        protected override float CalculateRequiredPowerInput()
        {
            if (Enabled && IsFunctional)
                return 0.0003f * Math.Abs(m_gravityAcceleration) * (float)Math.Pow(m_fieldSize.Value.Volume, 0.35);
            else
                return 0.0f;
        }

        protected override void UpdateText()
        {
            DetailedInfo.Clear();
            DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            DetailedInfo.Append(BlockDefinition.DisplayNameText);
            DetailedInfo.Append("\n");
            DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(ResourceSink.MaxRequiredInput, DetailedInfo);
            DetailedInfo.Append("\n");
            DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentInput));
			MyValueFormatter.AppendWorkInBestUnit(ResourceSink.IsPowered ? ResourceSink.RequiredInput : 0, DetailedInfo);
            RaisePropertiesChanged();
        }

        public override bool IsPositionInRange(Vector3D worldPoint)
        {
            Vector3 halfExtents = m_fieldSize.Value * 0.5f;
            MyOrientedBoundingBox obb = new MyOrientedBoundingBox((Vector3)WorldMatrix.Translation, halfExtents, Quaternion.CreateFromRotationMatrix(WorldMatrix));
            Vector3 conv = (Vector3)worldPoint;
            return obb.Contains(ref conv);
        }

        public override Vector3 GetWorldGravity(Vector3D worldPoint)
        {
            return Vector3.TransformNormal(Vector3.Down * GravityAcceleration, WorldMatrix);
        }

        protected override HkShape GetHkShape()
        {
            return new HkBoxShape(m_fieldSize.Value * 0.5f);
        }

        float ModAPI.Ingame.IMyGravityGenerator.FieldWidth { get { return m_fieldSize.Value.X; } }
        float ModAPI.Ingame.IMyGravityGenerator.FieldHeight { get { return m_fieldSize.Value.Y; } }
        float ModAPI.Ingame.IMyGravityGenerator.FieldDepth { get { return m_fieldSize.Value.Z; } }
        float ModAPI.Ingame.IMyGravityGenerator.Gravity { get { return GravityAcceleration / MyGravityProviderSystem.G; } }
    }
}

