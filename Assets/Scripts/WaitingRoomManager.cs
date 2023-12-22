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
        // 잠깐 임시로 최대 플레이어 수 고정
        private const int MAX_PLAYERS = 2; //수정하기


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
                Debug.LogFormat("로컬플레이어 생성 from {0}", SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
            }
        }
        #region Photon Callbacks

        // 코드 작성한 부분
        // 플레이어의 Properties가 업데이트 되었을 때
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            Debug.Log("OnPlayerPropertiesUpdate() 호출");
            CheckPlayersAndLoadGameRoom();
        }

        // 플레이어 수 체크, 조건 충족 시 GameRoom 씬으로 이동
        private void CheckPlayersAndLoadGameRoom()
        {
            // 플레이어 수가 짝수이고, 모든 플레이어가 레디 상태일 때 다음 씬으로 넘어감
            if (PhotonNetwork.CurrentRoom.PlayerCount % 2 == 0 && AllPlayersReady())
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    // 룸 옵션 - 더이상 새로운 플레이어가 룸에 들어오지 못하게 방 설정 변경
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    // 모든 플레이어가 ready 상태라면, MasterClient가 GameRoom Scene 호출
                    PhotonNetwork.LoadLevel("GameRoom");
                }
            }
        }

        // 모든 플레이어가 ready 상태인지 확인하는 함수
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

            // 플레이어가 룸을 떠났을 때 CheckPlayersAndLoadGameRoom() 실행
            CheckPlayersAndLoadGameRoom();
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            }
        }
        // 현재 클라이언트가 룸을 떠났을 때
        public override void OnLeftRoom()
        {
            Debug.LogFormat("OnLeftRoom() 호출");
            SceneManager.LoadScene(0);
        }

        #endregion
        #region Private Methods



        #endregion
        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
            Debug.LogFormat("LeaveRoom() 호출");
        }

        #endregion
    }
}