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
        public GameObject ghostPrefab; //�ͽ� �׽�Ʈ �� ���� ���� 
        public GameObject seekerDataObject; //�̰� �ȵǸ� �� �Ʒ��� �������� �ٲٱ�

        public int numOfGhosts; // ���� ��

        // �÷��̾��� isAlive ���� ����
        private Dictionary<int, bool> playerAliveStatus = new Dictionary<int, bool>();
        // �÷��̾��� �� ���� ����
        private Dictionary<int, int> playerTeamStatus = new Dictionary<int, int>();
        // �÷��̾��� �̼� ���� ����
        private Dictionary<int, int> playerMissionStatus = new Dictionary<int, int>();
        // �̼� �ӽ� ���� ���
        private int[] missionArray;
        // �̼� ������ ������
        public GameObject missionItemPrefab;
        public Transform contentTransform;

        // ���� ������, ���� ��ġ
        public Vector2[] boxSpawnPoints = { new Vector2(-10, -10), new Vector2(0, -10), new Vector2(-10, 0) };
        public GameObject boxPrefab;
        public GameObject boxDataObject; // ������ box ������ ����
        public Vector3[] ghostSpawnPoints = { new Vector3(-28, 10, 0), new Vector3(-28, 5, 0), new Vector3(-40, 6, 0), new Vector3(-42, 12, 0), new Vector3(-30, 12, 0), new Vector3(-22, 7, 0), new Vector3(-15, 16, 0), new Vector3(-12, 24, 0), new Vector3(-9, 13, 0), new Vector3(-9, 18, 0), new Vector3(-7, 19, 0), new Vector3(-16, 26, 0), new Vector3(-7, 26, 0), new Vector3(-8, 32, 0), new Vector3(-4, 5, 0), new Vector3(0, 5, 0), new Vector3(2, 5, 0), new Vector3(9, 7, 0), new Vector3(3, -2, 0), new Vector3(0, -5, 0), new Vector3(-28, 10, 0), new Vector3(8, -6, 0), new Vector3(0, -9, 0), new Vector3(-35, -7, 0), new Vector3(-31, -5, 0), new Vector3(-30, -20, 0), new Vector3(-21, -20, 0), new Vector3(-26, -25, 0), new Vector3(-26, -19, 0), new Vector3(-20, -20, 0), new Vector3(-60, -11, 0), new Vector3(-55, 13, 0), new Vector3(-59, -3, 0), new Vector3(-54, -3, 0) };
        public Vector3[] seekerSpawnPoints = {new Vector3(-28,15,0), new Vector3(-58, -10, 0),new Vector3(-25, -16, 0),new Vector3(0, -5, 0),new Vector3(0, 6, 0),new Vector3(-13, 16, 0),new Vector3(-11, 28, 0),new Vector3(-32,6,0) };


        // �� �̼� �޼� �÷��̾� ��
        private int numOfmissionClearPlayer = 0;

        // �� ����, ���� ���� ���� �÷��̾� �� üũ
        private int teamCounter = 0;
        private int numOfReadyPlayers = 0;// teamCounter�� �ص� �Ǵµ�, �ϴ� ���� ���� ���� ����

        public GameObject screenBlock; // ȭ�� ������
        public GameObject gameStartPanel;
        public GameObject gameOverPanel;


        private void Start()
        {
            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene("Launcher");
                return;
            }

            // ������ Ŭ���̾�Ʈ�� ��ǥ�� �÷��̾�� �̼� ����, ���� ���� ��ġ ����
            if (PhotonNetwork.IsMasterClient)
            {
                List<int> missionList = new List<int>();
                for (int i = 1; i <= PhotonNetwork.CurrentRoom.PlayerCount; i++)
                {
                    missionList.Add(i);
                }
                missionList = missionList.OrderBy(x => UnityEngine.Random.Range(0, PhotonNetwork.CurrentRoom.PlayerCount)).ToList();
                int[] missionArray = missionList.ToArray();

                // ���� Ŀ���� ������Ƽ�� ����
                ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
                roomProps.Add("mission", missionArray);
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

                // ����
                int randomIndex = Random.Range(0, boxSpawnPoints.Length);
                Vector2 boxSpawnPosition = boxSpawnPoints[randomIndex];
                PhotonNetwork.InstantiateRoomObject(boxPrefab.name, boxSpawnPosition, Quaternion.identity, 0, null);
            }

            // ������ Ŭ���̾�Ʈ�� ������ ����
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < numOfGhosts; i++)
                {
                    // ���� ��ġ ����
                    //Vector3 randomPosition = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), 0f);
                    // InstantiateRoomObject - ������ Ŭ���̾�Ʈ�� �Ҽ��� �ƴ�, Scene Object�� �Ҽ��� ��. ������ Ŭ���̾�Ʈ�� ����Ǵ��� �ı����� ����!
                    PhotonNetwork.InstantiateRoomObject(ghostPrefab.name, ghostSpawnPoints[i], Quaternion.identity, 0, null);
                }
            }

            CreatePlayer();
        }
        private void CreatePlayer()
        {
            Debug.Log("ĳ���� ���� �� �ؽ� ���̺� �߰� �̰� Ŭ���̾�Ʈ ���� �ѹ� ȣ���");

            // ���� ��ġ�� �����մϴ�.
            Vector3 randomPosition = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f),0f );

            /*GameObject*/
            seekerDataObject = PhotonNetwork.Instantiate(seekerPrefab.name, randomPosition, Quaternion.identity, 0); //�ٸ� ��ũ��Ʈ���� ����� ���� �ʱ�ȭ
            int viewID = seekerDataObject.GetComponent<PhotonView>().ViewID;
            
            seekerDataObject.GetComponent<Animator>().SetBool("isMeOrTeam", true);
            
            // Custom Properties�� �ڽ��� ViewID�� �����մϴ�.
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "viewID", viewID } });
        }

        //�� ����� ��� �÷��̾ �뿡 ������ �ִ� ���·� ���� ������ �Ѿ�� ������ �����ϴ�. 
        //��ųʸ� �߰� �� ��ųʸ��� �� á���� Ȯ���ϰ�, �� á���� ���� ���۽� ������ ���� �۵��� ������ �ȴ�. 
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (changedProps.ContainsKey("viewID"))
            {
                int viewID = (int)targetPlayer.CustomProperties["viewID"];

                //Alive ��ųʸ� �߰�
                playerAliveStatus[viewID] = true;

                //Team ��ųʸ� �߰�
                playerTeamStatus[viewID] = teamCounter / 2;
                teamCounter++;
                numOfReadyPlayers++;

                if(numOfReadyPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    GameStart();
                }
            }
        }

        // ������ Ŭ���̾�Ʈ���� ������ ���� �̼� �迭�� �� Ŭ���̾�Ʈ������ �Ȱ��� ����
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

        // ��� �÷��̾ ���� ���� ������ �� ����(= OnPlayerPropertiesUpdate�� �÷��̾� ����ŭ ȣ��Ǿ��� ��)
        private void GameStart()
        {
            // �ڽŰ� ���� ���� Seeker�� ���̰� ����
            int myViewID = seekerDataObject.GetComponent<PhotonView>().ViewID;
            int myTeamNumber = playerTeamStatus[myViewID];

            int i = 0;
            foreach (var pair in playerTeamStatus)
            {
                //Mission ��ųʸ� �߰�
                playerMissionStatus[pair.Key] = missionArray[i];
                i++;
                Debug.Log("ViewID: " + pair.Key + " missionNum: " + playerMissionStatus[pair.Key]);

                // �� �ڵ尡 ��ġ�ϴ��� Ȯ��
                if (pair.Value == myTeamNumber)
                {
                    // ��ġ�ϴ� �÷��̾��� ViewID�� �����ɴϴ�.
                    int matchingViewID = pair.Key;

                    // �� ViewID�� �̿��Ͽ� PhotonNetwork�� ���� GameObject�� ����
                    PhotonView pv = PhotonView.Find(matchingViewID);

                    GameObject go = pv.gameObject;
                    Animator anim = go.GetComponent<Animator>();

                    anim.SetBool("isMeOrTeam", true);
                }
            }

            // �ڽ��� �̼� text�� UI�� ���, scroll view�� �̼� ����� ó�� ������ ����
            UpdateMissionText(playerMissionStatus[myViewID]);

            //�÷��̾� ������ҷ� �̵�
            seekerDataObject.transform.position = seekerSpawnPoints[playerMissionStatus[myViewID]-1];

            //ȭ�� ���� ���� ���� �ִϸ��̼� ����
            screenBlock.SetActive(false);
            //�׽��� ���۹�¦
            StartCoroutine(ToggleGameStart());



        }
        IEnumerator ToggleGameStart()
        {
            gameStartPanel.SetActive(true); // ������Ʈ Ȱ��ȭ

            yield return new WaitForSeconds(3f);  // 3�� ���

            gameStartPanel.SetActive(false); // ������Ʈ ��Ȱ��ȭ
        }

        public void UpdateMissionText(int missionId)
        {
            if (missionDict.TryGetValue(missionId, out Mission mission))
            {
                // �̼� �������� Content�� �߰�
                GameObject newMissionItem = Instantiate(missionItemPrefab, contentTransform);

                // TextMeshPro ������Ʈ�� ã�� ���� ����
                TMP_Text missionText = newMissionItem.GetComponentInChildren<TMP_Text>();
                missionText.text = mission.Description;

                // MissionObject ������Ʈ�� MissionNumber ���� ���� ����
                MissionObject missionObject = newMissionItem.GetComponent<MissionObject>();
                if (missionObject != null)
                {
                    missionObject.missionNumber = mission.Id;
                }
            }
            else
            {
                // Id�� �ش��ϴ� �̼��� ���� ����� ó��
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

        // �̼� ������ ��¿�
        Dictionary<int, Mission> missionDict = new Dictionary<int, Mission>
        {
            { 1, new Mission { Id = 1, Description = "Object1" } },
            { 2, new Mission { Id = 2, Description = "Object2" } },
            { 3, new Mission { Id = 3, Description = "Object3" } },
            { 4, new Mission { Id = 4, Description = "Object4" } },
            { 5, new Mission { Id = 5, Description = "Object5" } },
            { 6, new Mission { Id = 6, Description = "Object6" } }
        };

        // �̼� ��ųʸ� ��������
        public Dictionary<int, int> GetMissionDictionary()
        {
            return playerMissionStatus;
        }

        public void SetPlayerAliveStatus(int viewID, bool isAlive)
        {
            playerAliveStatus[viewID] = isAlive;
            
            //�� ĳ������ �����̶�� ĳ���� ���� �Ұ� �� �ڽ��� ĳ���� ��� �ΰ� ȿ�� �� ���� �ֱ� 
            // �¸����� �޼� ���ε� ���⼭ üũ
            if (viewID == seekerDataObject.GetComponent<PhotonView>().ViewID) {
                seekerDataObject.GetComponent<Animator>().SetBool("Revealed",true);
            }

            // ���� �� �����ϴ� �ڵ� �ۼ�
            int winTeamNumber = CheckGameEnd();
            if (winTeamNumber >= 0)
            {
                Debug.Log("���� ��!" + winTeamNumber);
                gameOverPanel.SetActive(true);
            }
        }

        // �̼� Ŭ����� �߻�, �̼� Ŭ����� numOfmissionClearPlayer ������Ŵ
        public void SetPlayerMissionStatus(int viewID, int missionNumber)
        {
            playerMissionStatus[viewID] = missionNumber;
            numOfmissionClearPlayer++;
            if(numOfmissionClearPlayer >= numOfReadyPlayers / 2)
            {
                // ���ڿ� ���õ� �ڵ�� ���� �ۼ�, ���� ������ ������ Ŭ���̾�Ʈ�� RPC�� ���ڰ� Ȱ��ȭ�ǰ� ����
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
            // �� ã��, ���� ���� ������ ����� playerAliveStatus ���� �ٲٱ�
            int teamNumber = playerTeamStatus[viewID];

            foreach (var pair in playerTeamStatus)
            {
                // ���� ���� ������ ����� playerAliveStatus ���� �ٲٱ�
                if (pair.Value != teamNumber)
                {
                    int matchingViewID = pair.Key;
                    playerAliveStatus[matchingViewID] = false;

                    // �״� ����� �����
                    if (matchingViewID == seekerDataObject.GetComponent<PhotonView>().ViewID)
                    {
                        seekerDataObject.GetComponent<Animator>().SetBool("Revealed", true);
                    }
                }
            }
            // ���� �� �����ϴ� �ڵ� �ۼ�
            int winTeamNumber = CheckGameEnd();
            if (winTeamNumber >= 0)
            {
                Debug.Log("���� ��!" + winTeamNumber);

                gameOverPanel.SetActive(true);
            }
        }

        // ������ ������ �ʾ��� ��� -1 ��ȯ, ������ ������ ��� ����� ��ȯ
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
                    // �ٸ� �����ڰ� ���� ���� �ƴ� ���
                    else if(winTeamNumber != playerTeamStatus[pair.Key])
                    {

                        Debug.Log("���� ������ �ƴ� ���� ����ִ�");
                        return -1;
                    }
                }
            }
            return winTeamNumber;
        }



        // ��Ÿ �ڵ�...
        // Photon Callbacks �� Private Methods, Public Methods ���� ���� �ڵ�� �����ϰ� ����
        //��� �ݹ� �޼���� ����� or Ŀ���� �Ұ� �ƴϸ� ���� �ǵ� �ʿ� ����
    }
}


public class Mission
{
    public int Id { get; set; }
    public string Description { get; set; }
}