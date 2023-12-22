using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Com.MyCompany.JAHAS
{
    public class Ready : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        public bool isReady;

        public void Start()
        {
            isReady = false;
        }

        public void OnClickReadyButton()
        {
            Debug.Log("photonView is: " + photonView);
            Debug.Log("photonView.IsMine value: " + photonView.IsMine);

            if (photonView.IsMine)
            {
                Debug.Log("플레이어가 Ready 버튼을 누름");
                if (!isReady)
                {
                    // 클라이언트의 isReady 변수를 true로 변경
                    isReady = true;
                    photonView.RPC("UpdateNickNameColor", RpcTarget.All, true);
                }
                else
                {
                    // 클라이언트의 isReady 변수를 false로 변경
                    isReady = false;
                    photonView.RPC("UpdateNickNameColor", RpcTarget.All, false);
                }
            }
        }

        // 모든 클라이언트들이 이 메서드를 실행하도록 설정
        [PunRPC]
        public void UpdateNickNameColor(bool state)
        {
            // 내것만 Ready 상태 반영
            if (photonView.IsMine)
            {
                Debug.Log("눌렸어요웩");
                Debug.Log("isReady state is now: " + state);

                // SeekerNickName 클래스 인스턴스를 가져옴
                var seekerNickName = GetComponent<SeekerNickName>();

                // 색상 업데이트 RPC 호출
                seekerNickName.photonView.RPC("NickUpdate", RpcTarget.AllBuffered, photonView.Owner.NickName, state);

                // 각 플레이어들의 ready 상태를 확인하기 위한 Hashtable 추가
                Hashtable props = new Hashtable { { "isReady", state } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }
        }
    }
}
