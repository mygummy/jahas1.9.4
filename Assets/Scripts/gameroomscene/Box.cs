using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.JAHAS
{
    public class Box : MonoBehaviourPun
    {
        // Box(RoomObject로 생성) 수정에 대한 RPC는 Box 컴포넌트에 있어야 한다네...
        [PunRPC]
        private void SetBoxState(bool isConditionClear)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            MissionObject boxMissionObject = GetComponent<MissionObject>();
            if (!isConditionClear)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                boxMissionObject.missionNumber = 50;
            }
            else
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                boxMissionObject.missionNumber = 100;
            }
        }
    }
}