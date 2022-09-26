using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Main_FieldPortal : Main_FieldObjectBase
	{
		public HashSet<Main_FieldPortal> hsContactPortal = new HashSet<Main_FieldPortal>();
		public const float c_fTotalFadeTime = 1f;
		public static bool isProcessPortal = false;

		[Header("[Main_FieldPortal]")]
		public Main_FieldGround fgFieldDest;

		public GameObject goConnectObj;
		public Vector2 vec2OffsetDest;

		public bool isFade;

		private void OnTriggerExit2D(Collider2D collision)
		{
			if (GlobalUtility.Digit.Declude(collision.gameObject.layer, GlobalDefine.CollideLayer.PlayerPoint))
				return;

			hsContactPortal.Remove(this);
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (GlobalUtility.Digit.Declude(collision.gameObject.layer, GlobalDefine.CollideLayer.PlayerPoint))
				return;

			hsContactPortal.Add(this);

			if (isProcessPortal)
			{
				return;
			}

			Main_FieldObjectManager.Single.fgFieldBuffer = fgFieldDest;

			// Buffer Field 활성화
			Main_FieldObjectManager.Single.fgFieldBuffer.OnEnableField();

			// 화면 전환
			Main_FieldPortal.isProcessPortal = true;

			if (isFade)
			{
				Main_PageManager.Single.FadeOut(
					Main_FieldPortal.c_fTotalFadeTime / 2f,
					() =>
					{
						Main_PageManager.Single.FadeIn(Main_FieldPortal.c_fTotalFadeTime / 2f,
							() => Main_FieldPortal.isProcessPortal = false);

						TeleportPlayer();
					});
			}
			else
			{
				TeleportPlayer();
			}

			Main_FieldObjectManager.Single.fgFieldCurrent = Main_FieldObjectManager.Single.fgFieldBuffer;
			Main_FieldObjectManager.Single.fgFieldBuffer = null;
		}

		private void TeleportPlayer()
		{
			// Current Field 비활성화
			Main_FieldObjectManager.Single.fgFieldCurrent.OnDisableField();

			if (isFade)
			{
				Vector3 vec3CamDest = fgFieldDest.transform.position;
				vec3CamDest.z = SceneMain_Main.Single.camMain.transform.position.z;

				SceneMain_Main.Single.fcPlayer.transform.position = goConnectObj.transform.position + vec2OffsetDest.Vec3();
				SceneMain_Main.Single.camMain.transform.position = vec3CamDest;
			}
			else
			{
				Vector3 vec3CamSour = SceneMain_Main.Single.camMain.transform.position;
				Vector3 vec3CamDest = fgFieldDest.transform.position;
				vec3CamDest.z = vec3CamSour.z;

				Vector3 vec3CharSour = SceneMain_Main.Single.fcPlayer.transform.position;
				Vector3 vec3CharDest = goConnectObj.transform.position + vec2OffsetDest.Vec3();

				CustomRoutine.CallInTime(Main_FieldPortal.c_fTotalFadeTime,
					(fInTime) =>
					{
						SceneMain_Main.Single.fcPlayer.transform.position = Vector3.Lerp(vec3CharSour, vec3CharDest, Easing.EaseOutExpo(0, 1, fInTime));
						SceneMain_Main.Single.camMain.transform.position = Vector3.Lerp(vec3CamSour, vec3CamDest, Easing.EaseOutExpo(0, 1, fInTime));
					},
					() => Main_FieldPortal.isProcessPortal = false);
			}
		}
	}
}