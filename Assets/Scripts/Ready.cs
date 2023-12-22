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
                Debug.Log("�÷��̾ Ready ��ư�� ����");
                if (!isReady)
                {
                    // Ŭ���̾�Ʈ�� isReady ������ true�� ����
                    isReady = true;
                    photonView.RPC("UpdateNickNameColor", RpcTarget.All, true);
                }
                else
                {
                    // Ŭ���̾�Ʈ�� isReady ������ false�� ����
                    isReady = false;
                    photonView.RPC("UpdateNickNameColor", RpcTarget.All, false);
                }
            }
        }

        // ��� Ŭ���̾�Ʈ���� �� �޼��带 �����ϵ��� ����
        [PunRPC]
        public void UpdateNickNameColor(bool state)
        {
            // ���͸� Ready ���� �ݿ�
            if (photonView.IsMine)
            {
                Debug.Log("���Ⱦ����");
                Debug.Log("isReady state is now: " + state);

                // SeekerNickName Ŭ���� �ν��Ͻ��� ������
                var seekerNickName = GetComponent<SeekerNickName>();

                // ���� ������Ʈ RPC ȣ��
                seekerNickName.photonView.RPC("NickUpdate", RpcTarget.AllBuffered, photonView.Owner.NickName, state);

                // �� �÷��̾���� ready ���¸� Ȯ���ϱ� ���� Hashtable �߰�
                Hashtable props = new Hashtable { { "isReady", state } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }
        }
    }
}
