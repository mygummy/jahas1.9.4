using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using EasyTransition;

namespace Com.MyCompany.JAHAS
{
    public class WaitingRoomManager : MonoBehaviourPunCallbacks
    {
        #region Transition Fields
        public TransitionSettings transition;
        public float startDelay;
        #endregion
        
        #region Public Variables 
        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;
        // ��� �ӽ÷� �ִ� �÷��̾� �� ����
        private const int MAX_PLAYERS = 2; //�����ϱ�


        #endregion

        void Awake()
        {
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
            // TransitionManager.Instance().Transition(transition, startDelay);

        }

        public void Start()
        {
            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene("Launcher");
                return;
            }

            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                Debug.LogFormat("�����÷��̾� ���� from {0}", SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
            }
        }
        #region Photon Callbacks

        // �ڵ� �ۼ��� �κ�
        // �÷��̾��� Properties�� ������Ʈ �Ǿ��� ��
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            Debug.Log("OnPlayerPropertiesUpdate() ȣ��");
            CheckPlayersAndLoadGameRoom();
        }

        // �÷��̾� �� üũ, ���� ���� �� GameRoom ������ �̵�
        private void CheckPlayersAndLoadGameRoom()
        {
            // �÷��̾� ���� ¦���̰�, ��� �÷��̾ ���� ������ �� ���� ������ �Ѿ
            if (PhotonNetwork.CurrentRoom.PlayerCount % 2 == 0 && AllPlayersReady())
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    // �� �ɼ� - ���̻� ���ο� �÷��̾ �뿡 ������ ���ϰ� �� ���� ����
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    // ��� �÷��̾ ready ���¶��, MasterClient�� GameRoom Scene ȣ��
                    PhotonNetwork.LoadLevel("GameRoom");
                }
            }
        }

        // ��� �÷��̾ ready �������� Ȯ���ϴ� �Լ�
        private bool AllPlayersReady()
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                object isReady = null;
                if (player.CustomProperties.TryGetValue("isReady", out isReady))
                {
                    Debug.Log($"Player {player.NickName} ready status: {(bool)isReady}");
                    if (!(bool)isReady)
                    {
                        return false;
                    }
                }
                else
                {
                    Debug.Log($"Player {player.NickName} has no ready status");
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        /// 
        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            }
        }

        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

            // �÷��̾ ���� ������ �� CheckPlayersAndLoadGameRoom() ����
            CheckPlayersAndLoadGameRoom();
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            }
        }
        // ���� Ŭ���̾�Ʈ�� ���� ������ ��
        public override void OnLeftRoom()
        {
            Debug.LogFormat("OnLeftRoom() ȣ��");
            SceneManager.LoadScene(0);
        }

        #endregion
        #region Private Methods



        #endregion
        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
            Debug.LogFormat("LeaveRoom() ȣ��");
        }

        #endregion
    }
}