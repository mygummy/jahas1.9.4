using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

namespace Com.MyCompany.JAHAS
{
    public class SeekerNickName : MonoBehaviourPunCallbacks
    {
        private TextMeshPro textMesh;
        private Ready readyScript;

        // Start is called before the first frame update
        void Start()
        {
            textMesh = GetComponentInChildren<TextMeshPro>();
            readyScript = GetComponent<Ready>();

            textMesh.text = photonView.Owner.NickName;

            // 플레이어의 준비 상태를 읽어옴
            object isReady = null;
            if (photonView.Owner.CustomProperties.TryGetValue("isReady", out isReady))
            {
                readyScript.isReady = (bool)isReady;
            }

            if (readyScript.isReady)
            {
                textMesh.color = Color.green;
            }
            else
            {
                textMesh.color = Color.black;
            }
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (photonView.IsMine && newPlayer == PhotonNetwork.LocalPlayer)
            {
                photonView.RPC("NickUpdate", RpcTarget.AllBuffered, photonView.Owner.NickName);
            }
        }

        [PunRPC]
        public void NickUpdate(string nick, bool isReady)
        {
            if (textMesh == null || readyScript == null)
            {
                Debug.LogError("textMesh or readyScript is null");
                return;
            }

            textMesh.text = nick;
            readyScript.isReady = isReady;

            if (readyScript.isReady)
            {
                textMesh.color = Color.green;
            }
            else
            {
                textMesh.color = Color.black;
            }
        }

    }
}
