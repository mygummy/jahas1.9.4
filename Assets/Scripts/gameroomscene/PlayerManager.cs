using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using TMPro;

namespace Com.MyCompany.JAHAS
{
    public class PlayerManager : MonoBehaviourPun
    {
        [SerializeField]
        private float interactionRange;
        [SerializeField]
        private float missionInteractionRange;

        private GameRoomManager gameRoomManager;
        private HashSet<Collider2D> touchedObjects = new HashSet<Collider2D>(); // 닿은 오브젝트를 저장할 HashSet


        private void Start()
        {
            gameRoomManager = FindObjectOfType<GameRoomManager>();

            // Seeker 오브젝트 참조를 찾습니다. Tag나 다른 방법을 사용할 수도 있습니다.
            //GameObject seekerObject = GameObject.Find("Seeker"); // Seeker의 이름이나 태그 등을 사용
            
        }

        //private void Update()
        //{
        //    // 플레이어 위치 얻기
        //    Transform seekerTransform = gameRoomManager.seekerDataObject.GetComponent<Transform>();

        //    // 원 안에 있는 모든 콜라이더 얻기
        //    Collider2D[] colliders = Physics2D.OverlapCircleAll(seekerTransform.position, missionInteractionRange);
        //    HashSet<Collider2D> currentlyTouchedObjects = new HashSet<Collider2D>(); // 현재 닿은 오브젝트를 저장할 임시 HashSet

        //    foreach (Collider2D col in colliders)
        //    {
        //        // MissionObject 컴포넌트가 있는지 확인
        //        MissionObject missionObject = col.GetComponent<MissionObject>();

        //        if (missionObject != null)
        //        {
        //            Dictionary<int, int> localMissionDict = gameRoomManager.GetMissionDictionary();
        //            int myViewID = gameRoomManager.seekerDataObject.GetComponent<PhotonView>().ViewID;

        //            // ID가 같은 물체인지 확인
        //            if (missionObject.missionNumber == localMissionDict[myViewID])
        //            {
        //                Debug.Log("닿였다." + col.gameObject.name);
        //                // 테두리를 두껍게 만드는 로직
        //                // 예: SpriteRenderer를 가져와 테두리 설정 변경
        //                SpriteRenderer spriteRenderer = col.GetComponent<SpriteRenderer>();
        //                spriteRenderer.material.SetFloat("_OutlineWidth", 0.1f);
        //                spriteRenderer.material.SetColor("_OutlineColor", Color.green);

        //                currentlyTouchedObjects.Add(col); // 현재 닿은 오브젝트 저장
        //            }
        //        }
        //    }
        //    // 범위를 벗어난 오브젝트 처리
        //    touchedObjects.ExceptWith(currentlyTouchedObjects);
        //    foreach (Collider2D col in touchedObjects)
        //    {
        //        SpriteRenderer spriteRenderer = col.GetComponent<SpriteRenderer>();
        //        if (spriteRenderer != null)
        //        {
        //            spriteRenderer.material.SetFloat("_OutlineWidth", 0f);
        //            spriteRenderer.material.SetColor("_OutlineColor", Color.green);
        //        }
        //    }

        //    touchedObjects = currentlyTouchedObjects; // 현재 닿은 오브젝트를 저장
        //}

        // 디버그 목적으로 원 그리기
        //private void OnDrawGizmos()
        //{
        //    Transform seekerTransform = gameRoomManager.seekerDataObject.GetComponent<Transform>();
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawWireSphere(seekerTransform.position, missionInteractionRange);
        //}


        //PC버전용 미션, 상호작용 버튼
        private void Update()
        {
            // 'a' 키가 눌렸을 때
            if (Input.GetKeyDown(KeyCode.A))
            {
                Interact();
            }

            // 's' 키가 눌렸을 때
            if (Input.GetKeyDown(KeyCode.S))
            {
                MissionInteract();
            }
        }

        public void Interact()
        {
            Debug.Log("저격시작0");

            /*if (!photonView.IsMine )
                return;*/
            Debug.Log("저격시작1");

            //생성된 시커 데이터 가져오기
            Transform seekerTransform = gameRoomManager.seekerDataObject.GetComponent<Transform>();
            SpriteRenderer seekerRenderer = gameRoomManager.seekerDataObject.GetComponent<SpriteRenderer>();
            Collider2D myCollider = seekerTransform.GetComponent<Collider2D>();
            Animator seekerAnimator = gameRoomManager.seekerDataObject.GetComponent<Animator>();

            seekerAnimator.SetTrigger("Attack");
            
            RaycastHit2D[] hits;

            if (seekerRenderer.flipX == true) //오른쪽으로 발사
            {
                hits = Physics2D.RaycastAll(seekerTransform.position, seekerTransform.right, interactionRange);
                Debug.DrawRay(seekerTransform.position, seekerTransform.right * interactionRange, Color.red);

            }
            else //왼쪽으로 발사
            {
                hits = Physics2D.RaycastAll(seekerTransform.position, -seekerTransform.right, interactionRange);
                Debug.DrawRay(seekerTransform.position, -seekerTransform.right * interactionRange, Color.red);

            }


            //첨맞은애가 무조건 내캐릭이라 배열 씀, 대신 두번째로 걸린놈을 해야하는데 일단 브레잌을 써보자
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != myCollider && hit.collider.CompareTag("Player"))
                {
                    // 충돌 처리
                    Debug.Log("플레이어태그이상무");
                    int viewID = hit.collider.GetComponent<PhotonView>().ViewID;
                    photonView.RPC("KillPlayer", RpcTarget.All, viewID);
                    break;
                }

                 else if (hit.collider != myCollider && hit.collider.CompareTag("Ghost"))
                {
                    // 자기 자신이 죽는 경우
                    Debug.Log("고스트태그이상무");

                    Die();
                    break;
                 }
            }


        }
        
        // 해당하는 오브젝트 모두 발동. 물건 둘 때 멀리 둬야할듯. 미션 1개 = 물건 1개 적용
        public void MissionInteract()
        {
            // 플레이어 위치 얻기
            Transform seekerTransform = gameRoomManager.seekerDataObject.GetComponent<Transform>();

            // 원 안에 있는 모든 콜라이더 얻기
            Collider2D[] colliders = Physics2D.OverlapCircleAll(seekerTransform.position, missionInteractionRange);

            foreach (Collider2D col in colliders)
            {
                // MissionObject 컴포넌트가 있는지 확인
                MissionObject missionObject = col.GetComponent<MissionObject>();

                if (missionObject != null)
                {
                    // 승리조건 - 활성화 상태가 된 상자를 열면 자신의 팀을 제외한 다른 모든 플레이어를 죽임
                    if(missionObject.missionNumber == 100)
                    {
                        int myViewID = gameRoomManager.seekerDataObject.GetComponent<PhotonView>().ViewID;
                        photonView.RPC("OpenChest", RpcTarget.All, myViewID);
                    }

                    // scroll view에 있는 미션 목록들을 가져옴
                    foreach (Transform child in gameRoomManager.contentTransform)
                    {
                        // 미션 목록 데이터를 가져옴
                        MissionObject childMissionObject = child.GetComponent<MissionObject>();

                        // 내가 미션을 깼을 경우
                        // missionNumber가 같은지 확인
                        if (missionObject.missionNumber == childMissionObject.missionNumber)
                        {
                            Dictionary<int, int> localMissionDict = gameRoomManager.GetMissionDictionary();

                            // 딕셔너리에서 미션 넘버와 같은 ID 값을 찾음
                            // 누군가가 미션을 클리어 하면, 딕셔너리에 해당하는 value 값이 무조건 존재함
                            int targetID = -1;
                            foreach (var pair in localMissionDict)
                            {
                                if (pair.Value == missionObject.missionNumber)
                                {
                                    targetID = pair.Key;
                                    break;
                                }
                            }

                            // 내가 내 미션을 깼을 경우
                            int myViewID = gameRoomManager.seekerDataObject.GetComponent<PhotonView>().ViewID;
                            if (targetID == myViewID)
                            {
                                Debug.Log("내가 내 미션을 깼다!");
                                photonView.RPC("MissionClearPlayer", RpcTarget.All, targetID, 20);
                                // 내 미션을 깼으니, 다른 깨지지 않은 미션들 scroll view content에 추가
                                foreach (var pair in localMissionDict)
                                {
                                    if (pair.Value < 10 && pair.Value != missionObject.missionNumber)
                                    {
                                        gameRoomManager.UpdateMissionText(pair.Value);
                                    }
                                }
                            }
                            // 내가 남 미션을 깼을 경우
                            else
                            {
                                Debug.Log("내가 남 미션을 깼다!");
                                photonView.RPC("MissionClearPlayer", RpcTarget.All, targetID, 21);
                            }

                            // 해당 child mission number를 가진 사람 모두 21로 수정, text를 빨간색으로 설정, RPC
                            photonView.RPC("SetMissionTextFailed", RpcTarget.All, missionObject.missionNumber);

                            // 해당 scroll view의 미션을 mission number를 20, text를 초록색으로 설정
                            childMissionObject.missionNumber = 20;
                            TMP_Text missionText = child.GetComponentInChildren<TMP_Text>();
                            missionText.color = Color.green;
                        }
                    }
                }
            }
        }

        [PunRPC]
        private void KillPlayer(int viewID)
        {
            gameRoomManager.SetPlayerAliveStatus(viewID, false);
        }

        [PunRPC]
        private void MissionClearPlayer(int viewID, int value)
        {
            gameRoomManager.SetPlayerMissionStatus(viewID, value);

            // 여기다 미션 클리어 횟수 담당??

        }

        [PunRPC]
        private void SetMissionTextFailed(int missionNumber)
        {
            foreach (Transform child in gameRoomManager.contentTransform)
            {
                MissionObject childMissionObject = child.GetComponent<MissionObject>();
                if(childMissionObject.missionNumber == missionNumber)
                {
                    childMissionObject.missionNumber = 21;
                    TMP_Text missionText = child.GetComponentInChildren<TMP_Text>();
                    missionText.color = Color.red;
                }
            }
        }

        [PunRPC]
        private void OpenChest(int viewID)
        {
            gameRoomManager.SetGameOver(viewID);
        }

        private void Die()
        {
            // 자신의 죽음 처리 로직
            int myViewID = gameRoomManager.seekerDataObject.GetComponent<PhotonView>().ViewID;
            photonView.RPC("KillPlayer", RpcTarget.All, myViewID);
        }
    }
}
