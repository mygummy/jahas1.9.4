using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Com.MyCompany.JAHAS
{
    public class GameRoomManager : MonoBehaviourPunCallbacks
    {
        [Tooltip("The prefab to use for representing the player")]
        public GameObject seekerPrefab;
        public GameObject ghostPrefab; //귀신 테스트 용 추후 제거 
        public GameObject seekerDataObject; //이거 안되면 저 아래에 선언으로 바꾸기

        public int numOfGhosts; // 유령 수

        // 플레이어의 isAlive 상태 관리
        private Dictionary<int, bool> playerAliveStatus = new Dictionary<int, bool>();
        // 플레이어의 팀 상태 관리
        private Dictionary<int, int> playerTeamStatus = new Dictionary<int, int>();
        // 플레이어의 미션 상태 관리
        private Dictionary<int, int> playerMissionStatus = new Dictionary<int, int>();
        // 미션 임시 저장 장소
        private int[] missionArray;
        // 미션 아이템 프리팹
        public GameObject missionItemPrefab;
        public Transform contentTransform;

        // 상자 프리팹, 스폰 위치
        public Vector2[] boxSpawnPoints = { new Vector2(-10, -10), new Vector2(0, -10), new Vector2(-10, 0) };
        public GameObject boxPrefab;
        public GameObject boxDataObject; // 생성된 box 데이터 저장
        public Vector3[] ghostSpawnPoints = { new Vector3(-28, 10, 0), new Vector3(-28, 5, 0), new Vector3(-40, 6, 0), new Vector3(-42, 12, 0), new Vector3(-30, 12, 0), new Vector3(-22, 7, 0), new Vector3(-15, 16, 0), new Vector3(-12, 24, 0), new Vector3(-9, 13, 0), new Vector3(-9, 18, 0), new Vector3(-7, 19, 0), new Vector3(-16, 26, 0), new Vector3(-7, 26, 0), new Vector3(-8, 32, 0), new Vector3(-4, 5, 0), new Vector3(0, 5, 0), new Vector3(2, 5, 0), new Vector3(9, 7, 0), new Vector3(3, -2, 0), new Vector3(0, -5, 0), new Vector3(-28, 10, 0), new Vector3(8, -6, 0), new Vector3(0, -9, 0), new Vector3(-35, -7, 0), new Vector3(-31, -5, 0), new Vector3(-30, -20, 0), new Vector3(-21, -20, 0), new Vector3(-26, -25, 0), new Vector3(-26, -19, 0), new Vector3(-20, -20, 0), new Vector3(-60, -11, 0), new Vector3(-55, 13, 0), new Vector3(-59, -3, 0), new Vector3(-54, -3, 0) };
        public Vector3[] seekerSpawnPoints = {new Vector3(-28,15,0), new Vector3(-58, -10, 0),new Vector3(-25, -16, 0),new Vector3(0, -5, 0),new Vector3(0, 6, 0),new Vector3(-13, 16, 0),new Vector3(-11, 28, 0),new Vector3(-32,6,0) };


        // 총 미션 달성 플레이어 수
        private int numOfmissionClearPlayer = 0;

        // 팀 배정, 게임 시작 직전 플레이어 수 체크
        private int teamCounter = 0;
        private int numOfReadyPlayers = 0;// teamCounter로 해도 되는데, 일단 구분 위해 변수 만듬

        public GameObject screenBlock; // 화면 가리게
        public GameObject gameStartPanel;
        public GameObject gameOverPanel;


        private void Start()
        {
            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene("Launcher");
                return;
            }

            // 마스터 클라이언트가 대표로 플레이어에게 미션 배정, 랜덤 상자 위치 배정
            if (PhotonNetwork.IsMasterClient)
            {
                List<int> missionList = new List<int>();
                for (int i = 1; i <= PhotonNetwork.CurrentRoom.PlayerCount; i++)
                {
                    missionList.Add(i);
                }
                missionList = missionList.OrderBy(x => UnityEngine.Random.Range(0, PhotonNetwork.CurrentRoom.PlayerCount)).ToList();
                int[] missionArray = missionList.ToArray();

                // 룸의 커스텀 프로퍼티에 저장
                ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
                roomProps.Add("mission", missionArray);
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

                // 상자
                int randomIndex = Random.Range(0, boxSpawnPoints.Length);
                Vector2 boxSpawnPosition = boxSpawnPoints[randomIndex];
                PhotonNetwork.InstantiateRoomObject(boxPrefab.name, boxSpawnPosition, Quaternion.identity, 0, null);
            }

            // 마스터 클라이언트가 유령을 생성
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < numOfGhosts; i++)
                {
                    // 랜덤 위치 생성
                    //Vector3 randomPosition = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), 0f);
                    // InstantiateRoomObject - 마스터 클라이언트의 소속이 아닌, Scene Object의 소속이 됨. 마스터 클라이언트가 변경되더라도 파괴되지 않음!
                    PhotonNetwork.InstantiateRoomObject(ghostPrefab.name, ghostSpawnPoints[i], Quaternion.identity, 0, null);
                }
            }

            CreatePlayer();
        }
        private void CreatePlayer()
        {
            Debug.Log("캐릭터 생성 및 해쉬 테이블 추가 이건 클라이언트 별로 한번 호출됨");

            // 랜덤 위치를 생성합니다.
            Vector3 randomPosition = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f),0f );

            /*GameObject*/
            seekerDataObject = PhotonNetwork.Instantiate(seekerPrefab.name, randomPosition, Quaternion.identity, 0); //다른 스크립트에서 사용을 위한 초기화
            int viewID = seekerDataObject.GetComponent<PhotonView>().ViewID;
            
            seekerDataObject.GetComponent<Animator>().SetBool("isMeOrTeam", true);
            
            // Custom Properties에 자신의 ViewID를 저장합니다.
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "viewID", viewID } });
        }

        //이 방식은 모든 플레이어가 룸에 접속해 있는 상태로 다음 씬으로 넘어가기 때문에 가능하다. 
        //딕셔너리 추가 시 딕셔너리가 다 찼는지 확인하고, 다 찼으면 게임 시작시 실행할 여러 작동을 넣으면 된다. 
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (changedProps.ContainsKey("viewID"))
            {
                int viewID = (int)targetPlayer.CustomProperties["viewID"];

                //Alive 딕셔너리 추가
                playerAliveStatus[viewID] = true;

                //Team 딕셔너리 추가
                playerTeamStatus[viewID] = teamCounter / 2;
                teamCounter++;
                numOfReadyPlayers++;

                if(numOfReadyPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    GameStart();
                }
            }
        }

        // 마스터 클라이언트에서 설정한 랜덤 미션 배열을 각 클라이언트에서도 똑같이 적용
        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            if (propertiesThatChanged.ContainsKey("mission"))
            {
                missionArray = (int[])propertiesThatChanged["mission"];
                for(int i = 0; i < missionArray.Length; i++)
                {
                    Debug.Log("missionArray: " + missionArray[i]);
                }
            }
        }

        // 모든 플레이어가 게임 시작 상태일 때 실행(= OnPlayerPropertiesUpdate가 플레이어 수만큼 호출되었을 때)
        private void GameStart()
        {
            // 자신과 같은 팀은 Seeker로 보이게 설정
            int myViewID = seekerDataObject.GetComponent<PhotonView>().ViewID;
            int myTeamNumber = playerTeamStatus[myViewID];

            int i = 0;
            foreach (var pair in playerTeamStatus)
            {
                //Mission 딕셔너리 추가
                playerMissionStatus[pair.Key] = missionArray[i];
                i++;
                Debug.Log("ViewID: " + pair.Key + " missionNum: " + playerMissionStatus[pair.Key]);

                // 팀 코드가 일치하는지 확인
                if (pair.Value == myTeamNumber)
                {
                    // 일치하는 플레이어의 ViewID를 가져옵니다.
                    int matchingViewID = pair.Key;

                    // 이 ViewID를 이용하여 PhotonNetwork을 통해 GameObject에 접근
                    PhotonView pv = PhotonView.Find(matchingViewID);

                    GameObject go = pv.gameObject;
                    Animator anim = go.GetComponent<Animator>();

                    anim.SetBool("isMeOrTeam", true);
                }
            }

            // 자신의 미션 text를 UI에 출력, scroll view에 미션 목록을 처음 넣을때 실행
            UpdateMissionText(playerMissionStatus[myViewID]);

            //플레이어 스폰장소로 이동
            seekerDataObject.transform.position = seekerSpawnPoints[playerMissionStatus[myViewID]-1];

            //화면 암전 제거 추후 애니메이션 적용
            screenBlock.SetActive(false);
            //겜시작 반작반짝
            StartCoroutine(ToggleGameStart());



        }
        IEnumerator ToggleGameStart()
        {
            gameStartPanel.SetActive(true); // 오브젝트 활성화

            yield return new WaitForSeconds(3f);  // 3초 대기

            gameStartPanel.SetActive(false); // 오브젝트 비활성화
        }

        public void UpdateMissionText(int missionId)
        {
            if (missionDict.TryGetValue(missionId, out Mission mission))
            {
                // 미션 아이템을 Content에 추가
                GameObject newMissionItem = Instantiate(missionItemPrefab, contentTransform);

                // TextMeshPro 컴포넌트를 찾아 문구 설정
                TMP_Text missionText = newMissionItem.GetComponentInChildren<TMP_Text>();
                missionText.text = mission.Description;

                // MissionObject 컴포넌트의 MissionNumber 변수 값을 설정
                MissionObject missionObject = newMissionItem.GetComponent<MissionObject>();
                if (missionObject != null)
                {
                    missionObject.missionNumber = mission.Id;
                }
            }
            else
            {
                // Id에 해당하는 미션이 없을 경우의 처리
                Debug.LogError("UpdateMissionText error.");
            }
        }

        private void Update()
        {
            foreach (var pair in playerAliveStatus)
            {
                Debug.Log($"ViewID: {pair.Key}, IsAlive: {pair.Value}");
            }
            foreach (var pair in playerTeamStatus)
            {
                Debug.Log($"ViewID: {pair.Key}, TeamCode: {pair.Value}");
            }
        }

        // 미션 데이터 출력용
        Dictionary<int, Mission> missionDict = new Dictionary<int, Mission>
        {
            { 1, new Mission { Id = 1, Description = "Object1" } },
            { 2, new Mission { Id = 2, Description = "Object2" } },
            { 3, new Mission { Id = 3, Description = "Object3" } },
            { 4, new Mission { Id = 4, Description = "Object4" } },
            { 5, new Mission { Id = 5, Description = "Object5" } },
            { 6, new Mission { Id = 6, Description = "Object6" } }
        };

        // 미션 딕셔너리 가져오기
        public Dictionary<int, int> GetMissionDictionary()
        {
            return playerMissionStatus;
        }

        public void SetPlayerAliveStatus(int viewID, bool isAlive)
        {
            playerAliveStatus[viewID] = isAlive;
            
            //내 캐릭터의 죽음이라면 캐릭터 조작 불가 및 자신의 캐릭터 사망 부과 효과 다 여기 넣기 
            // 승리조건 달성 여부도 여기서 체크
            if (viewID == seekerDataObject.GetComponent<PhotonView>().ViewID) {
                seekerDataObject.GetComponent<Animator>().SetBool("Revealed",true);
            }

            // 게임 끝 세팅하는 코드 작성
            int winTeamNumber = CheckGameEnd();
            if (winTeamNumber >= 0)
            {
                Debug.Log("게임 끝!" + winTeamNumber);
                gameOverPanel.SetActive(true);
            }
        }

        // 미션 클리어시 발생, 미션 클리어시 numOfmissionClearPlayer 증가시킴
        public void SetPlayerMissionStatus(int viewID, int missionNumber)
        {
            playerMissionStatus[viewID] = missionNumber;
            numOfmissionClearPlayer++;
            if(numOfmissionClearPlayer >= numOfReadyPlayers / 2)
            {
                // 상자와 관련된 코드는 여기 작성, 조건 만족시 마스터 클라이언트가 RPC로 상자가 활성화되게 설정
                GameObject box = GameObject.FindWithTag("Box");
                PhotonView boxPhotonView = box.GetComponent<PhotonView>();
                boxPhotonView.RPC("SetBoxState", RpcTarget.All, true);   
            }
        }

        public bool IsPlayerAlive(int viewID)
        {
            return playerAliveStatus.TryGetValue(viewID, out bool isAlive) ? isAlive : false;
        }

        public void SetGameOver(int viewID)
        {
            // 팀 찾고, 팀원 제외 나머지 사람들 playerAliveStatus 상태 바꾸기
            int teamNumber = playerTeamStatus[viewID];

            foreach (var pair in playerTeamStatus)
            {
                // 팀원 제외 나머지 사람들 playerAliveStatus 상태 바꾸기
                if (pair.Value != teamNumber)
                {
                    int matchingViewID = pair.Key;
                    playerAliveStatus[matchingViewID] = false;

                    // 죽는 사람이 나라면
                    if (matchingViewID == seekerDataObject.GetComponent<PhotonView>().ViewID)
                    {
                        seekerDataObject.GetComponent<Animator>().SetBool("Revealed", true);
                    }
                }
            }
            // 게임 끝 세팅하는 코드 작성
            int winTeamNumber = CheckGameEnd();
            if (winTeamNumber >= 0)
            {
                Debug.Log("게임 끝!" + winTeamNumber);

                gameOverPanel.SetActive(true);
            }
        }

        // 게임이 끝나지 않았을 경우 -1 반환, 게임이 끝났을 경우 우승팀 반환
        public int CheckGameEnd()
        {
            int winTeamNumber = -1;
            foreach (var pair in playerAliveStatus)
            {
                if(pair.Value == true)
                {
                    if(winTeamNumber < 0)
                    {
                        winTeamNumber = playerTeamStatus[pair.Key];
                    }
                    // 다른 생존자가 같은 팀이 아닐 경우
                    else if(winTeamNumber != playerTeamStatus[pair.Key])
                    {

                        Debug.Log("아직 같은팀 아닌 놈이 살아있다");
                        return -1;
                    }
                }
            }
            return winTeamNumber;
        }



        // 기타 코드...
        // Photon Callbacks 및 Private Methods, Public Methods 등은 이전 코드와 동일하게 유지
        //사실 콜백 메서드는 디버그 or 커스텀 할거 아니면 굳이 건들 필요 없음
    }
}


public class Mission
{
    public int Id { get; set; }
    public string Description { get; set; }
}