using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DQR.Models
{
	[System.Serializable]
	public struct MeshSheetFrame
	{
		public Mesh m_Mesh;
		public float m_Duration;
	}
	
	public class MeshSheet : ScriptableObject
	{
		public enum PlaybackMode
		{
			PlayOnce,
			Loop,
			PingPong
		}

		[SerializeField]
		private MeshSheetFrame[] m_AnimationFrames = null;

		[SerializeField]
		private float m_GlobalDurationScale = 1.0f;

		[SerializeField]
		private PlaybackMode m_PlaybackMode = PlaybackMode.PlayOnce;

		[SerializeField, HideInInspector]
		private float m_TotalUnscaledDuration = 0.0f;

		public static MeshSheet CreateNew(IEnumerable<MeshSheetFrame> frames)
		{
			MeshSheet sheet = ScriptableObject.CreateInstance<MeshSheet>();
			sheet.m_AnimationFrames = frames.ToArray();

			sheet.m_TotalUnscaledDuration = 0.0f;
			foreach (var frame in frames)
				sheet.m_TotalUnscaledDuration += frame.m_Duration;

			return sheet;
		}

		public int FrameCount
		{
			get => m_AnimationFrames.Length;
		}
		
		public float TotalUnscaledDuration
		{
			get => m_TotalUnscaledDuration;
		}

		public float TotalDuration
		{
			get => m_TotalUnscaledDuration * m_GlobalDurationScale;
		}

		public float GlobalDurationScale
		{
			get => m_GlobalDurationScale;
			set => m_GlobalDurationScale = value;
		}

		public PlaybackMode PlayMode
		{
			get => m_PlaybackMode;
			set => m_PlaybackMode = value;
		}

		public Mesh GetMeshAtFrame(int frameIndex)
		{
			Assert.Condition(frameIndex >= 0 && frameIndex < m_AnimationFrames.Length);
			return m_AnimationFrames[frameIndex].m_Mesh;
		}

		public Mesh GetMeshAtTime(float timeStamp)
		{
			// Invalid/Single frame animation, so early out
			int count = m_AnimationFrames.Length;

			if (count == 1 || m_TotalUnscaledDuration <= 0.0f)
				return m_AnimationFrames[0].m_Mesh;

			// Perform animation
			float t = GetAnimationTime(timeStamp);

			foreach (var frame in m_AnimationFrames)
			{
				if (t < frame.m_Duration)
					return frame.m_Mesh;
				else
					t -= frame.m_Duration;
			}

			return m_AnimationFrames[m_AnimationFrames.Length - 1].m_Mesh;
		}

		public float GetAnimationTime(float timeStamp)
		{
			float t = 0.0f;

			switch (m_PlaybackMode)
			{
				case PlaybackMode.PlayOnce:
					t = Mathf.Clamp(timeStamp, -m_TotalUnscaledDuration, m_TotalUnscaledDuration);
					break;

				case PlaybackMode.Loop:
					t = timeStamp % m_TotalUnscaledDuration;
					break;

				case PlaybackMode.PingPong:
					int steps = Mathf.FloorToInt(t / m_TotalUnscaledDuration);
					break;

				default:
					Assert.FailFormat("Unsupported PlaybackMode '{0}'", m_PlaybackMode);
					return 0.0f;
			}
			
			return t * m_GlobalDurationScale;
		}
	}
}
