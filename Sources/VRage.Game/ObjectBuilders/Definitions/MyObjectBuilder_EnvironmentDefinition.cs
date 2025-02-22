﻿using System.Collections.Generic;
using ProtoBuf;
using VRage.Data;
using VRage.ObjectBuilders;
using VRageMath;
using System.Xml.Serialization;

namespace VRage.Game
{
    [ProtoContract]
    [MyObjectBuilderDefinition]
    public class MyObjectBuilder_EnvironmentDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember]
        public SerializableVector3 SunDirection;

        [ProtoMember, ModdableContentFile("dds")]
        public string EnvironmentTexture;

        [ProtoMember, ModdableContentFile("dds")]
        public string EnvironmentTextureNight = null;

        [ProtoMember, ModdableContentFile("dds")]
        public string EnvironmentTextureNightPrefiltered = null;

        [ProtoMember]
        public MyOrientation EnvironmentOrientation;

        [ProtoMember]
        public bool EnableFog;

        [ProtoMember]
        public float FogNear;

        [ProtoMember]
        public float FogFar;

        [ProtoMember]
        public float FogMultiplier;

        [ProtoMember]
        public float FogBacklightMultiplier;

        [ProtoMember]
        public float FogDensity;

        [ProtoMember]
        public SerializableVector3 FogColor;

        [ProtoMember]
        public SerializableVector3 SunDiffuse = new SerializableVector3(200 / 255.0f, 200 / 255.0f, 200 / 255.0f);

        [ProtoMember]
        public float SunIntensity = 1.456f;

        [ProtoMember]
        public SerializableVector3 SunSpecular = new SerializableVector3(200 / 255.0f, 200 / 255.0f, 200 / 255.0f);

        [ProtoMember]
        public SerializableVector3 BackLightDiffuse = new SerializableVector3(200 / 255.0f, 200 / 255.0f, 200 / 255.0f);

        [ProtoMember]
        public float BackLightIntensity = 0f;

        [ProtoMember, XmlArrayItem("LightDirection")]
        public SerializableVector2[] AdditionalSunDirection = new SerializableVector2[] { new Vector2(0, 0) };

        [ProtoMember]
        public SerializableVector3 AmbientColor = new SerializableVector3(36 / 255.0f, 36 / 255.0f, 36 / 255.0f);

        [ProtoMember]
        public float AmbientMultiplier = 0.969f;

        [ProtoMember]
        public float EnvironmentAmbientIntensity = 0.500f;

        [ProtoMember]
        public SerializableVector3 BackgroundColor = new SerializableVector3(0, 0, 0);

        [ProtoMember]
        public string SunMaterial = "SunDisk";

        [ProtoMember]
        public float SunSizeMultiplier = 200;

        [ProtoMember]
        public float SmallShipMaxSpeed = 100;

        [ProtoMember]
        public float LargeShipMaxSpeed = 100;

        [ProtoMember]
        public float SmallShipMaxAngularSpeed = 36000;

        [ProtoMember]
        public float LargeShipMaxAngularSpeed = 18000;

		[ProtoContract]
		public struct EnvironmentalParticleSettings
		{
			[ProtoMember]
			public SerializableDefinitionId Id;

			[ProtoMember]
			public string Material;

			[ProtoMember]
			public Vector4 Color;

			[ProtoMember]
			public float MaxSpawnDistance;

			[ProtoMember]
			public float DespawnDistance;

			[ProtoMember]
			public float Density;

			[ProtoMember]
			public int MaxLifeTime;

			[ProtoMember]
			public int MaxParticles;
		}

		[ProtoMember, XmlArrayItem("ParticleType")]
		public List<EnvironmentalParticleSettings> EnvironmentalParticles = new List<EnvironmentalParticleSettings>();

        [ProtoMember]
        public Vector4 ContourHighlightColor = new Vector4(1.0f, 1.0f, 0.0f, 0.05f);

        [ProtoMember]
        public float ContourHighlightThickness = 1;

        [ProtoMember]
        public float HighlightPulseInSeconds = 0;
    }
}
