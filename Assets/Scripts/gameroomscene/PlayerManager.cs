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
        private HashSet<Collider2D> touchedObjects = new HashSet<Collider2D>(); // ���� ������Ʈ�� ������ HashSet


        private void Start()
        {
            gameRoomManager = FindObjectOfType<GameRoomManager>();

            // Seeker ������Ʈ ������ ã���ϴ�. Tag�� �ٸ� ����� ����� ���� �ֽ��ϴ�.
            //GameObject seekerObject = GameObject.Find("Seeker"); // Seeker�� �̸��̳� �±� ���� ���
            
        }

        //private void Update()
        //{
        //    // �÷��̾� ��ġ ���
        //    Transform seekerTransform = gameRoomManager.seekerDataObject.GetComponent<Transform>();

        //    // �� �ȿ� �ִ� ��� �ݶ��̴� ���
        //    Collider2D[] colliders = Physics2D.OverlapCircleAll(seekerTransform.position, missionInteractionRange);
        //    HashSet<Collider2D> currentlyTouchedObjects = new HashSet<Collider2D>(); // ���� ���� ������Ʈ�� ������ �ӽ� HashSet

        //    foreach (Collider2D col in colliders)
        //    {
        //        // MissionObject ������Ʈ�� �ִ��� Ȯ��
        //        MissionObject missionObject = col.GetComponent<MissionObject>();

        //        if (missionObject != null)
        //        {
        //            Dictionary<int, int> localMissionDict = gameRoomManager.GetMissionDictionary();
        //            int myViewID = gameRoomManager.seekerDataObject.GetComponent<PhotonView>().ViewID;

        //            // ID�� ���� ��ü���� Ȯ��
        //            if (missionObject.missionNumber == localMissionDict[myViewID])
        //            {
        //                Debug.Log("�꿴��." + col.gameObject.name);
        //                // �׵θ��� �β��� ����� ����
        //                // ��: SpriteRenderer�� ������ �׵θ� ���� ����
        //                SpriteRenderer spriteRenderer = col.GetComponent<SpriteRenderer>();
        //                spriteRenderer.material.SetFloat("_OutlineWidth", 0.1f);
        //                spriteRenderer.material.SetColor("_OutlineColor", Color.green);

        //                currentlyTouchedObjects.Add(col); // ���� ���� ������Ʈ ����
        //            }
        //        }
        //    }
        //    // ������ ��� ������Ʈ ó��
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

        //    touchedObjects = currentlyTouchedObjects; // ���� ���� ������Ʈ�� ����
        //}

        // ����� �������� �� �׸���
        //private void OnDrawGizmos()
        //{
        //    Transform seekerTransform = gameRoomManager.seekerDataObject.GetComponent<Transform>();
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawWireSphere(seekerTransform.position, missionInteractionRange);
        //}


        //PC������ �̼�, ��ȣ�ۿ� ��ư
        private void Update()
        {
            // 'a' Ű�� ������ ��
            if (Input.GetKeyDown(KeyCode.A))
            {
                Interact();
            }

            // 's' Ű�� ������ ��
            if (Input.GetKeyDown(KeyCode.S))
            {
                MissionInteract();
            }
        }

        public void Interact()
        {
            Debug.Log("���ݽ���0");

            /*if (!photonView.IsMine )
                return;*/
            Debug.Log("���ݽ���1");

            //������ ��Ŀ ������ ��������
            Transform seekerTransform = gameRoomManager.seekerDataObject.GetComponent<Transform>();
            SpriteRenderer seekerRenderer = gameRoomManager.seekerDataObject.GetComponent<SpriteRenderer>();
            Collider2D myCollider = seekerTransform.GetComponent<Collider2D>();
            Animator seekerAnimator = gameRoomManager.seekerDataObject.GetComponent<Animator>();

            seekerAnimator.SetTrigger("Attack");
            
            RaycastHit2D[] hits;

            if (seekerRenderer.flipX == true) //���������� �߻�
            {
                hits = Physics2D.RaycastAll(seekerTransform.position, seekerTransform.right, interactionRange);
                Debug.DrawRay(seekerTransform.position, seekerTransform.right * interactionRange, Color.red);

            }
            else //�������� �߻�
            {
                hits = Physics2D.RaycastAll(seekerTransform.position, -seekerTransform.right, interactionRange);
                Debug.DrawRay(seekerTransform.position, -seekerTransform.right * interactionRange, Color.red);

            }


            //÷�����ְ� ������ ��ĳ���̶� �迭 ��, ��� �ι�°�� �ɸ����� �ؾ��ϴµ� �ϴ� �극���� �Ẹ��
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != myCollider && hit.collider.CompareTag("Player"))
                {
                    // �浹 ó��
                    Debug.Log("�÷��̾��±��̻�");
                    int viewID = hit.collider.GetComponent<PhotonView>().ViewID;
                    photonView.RPC("KillPlayer", RpcTarget.All, viewID);
                    break;
                }

                 else if (hit.collider != myCollider && hit.collider.CompareTag("Ghost"))
                {
                    // �ڱ� �ڽ��� �״� ���
                    Debug.Log("��Ʈ�±��̻�");

                    Die();
                    break;
                 }
            }


        }
        
        // �ش��ϴ� ������Ʈ ��� �ߵ�. ���� �� �� �ָ� �־��ҵ�. �̼� 1�� = ���� 1�� ����
        public void MissionInteract()
        {
            // �÷��̾� ��ġ ���
            Transform seekerTransform = gameRoomManager.seekerDataObject.GetComponent<Transform>();

            // �� �ȿ� �ִ� ��� �ݶ��̴� ���
            Collider2D[] colliders = Physics2D.OverlapCircleAll(seekerTransform.position, missionInteractionRange);

            foreach (Collider2D col in colliders)
            {
                // MissionObject ������Ʈ�� �ִ��� Ȯ��
                MissionObject missionObject = col.GetComponent<MissionObject>();

                if (missionObject != null)
                {
                    // �¸����� - Ȱ��ȭ ���°� �� ���ڸ� ���� �ڽ��� ���� ������ �ٸ� ��� �÷��̾ ����
                    if(missionObject.missionNumber == 100)
                    {
                        int myViewID = gameRoomManager.seekerDataObject.GetComponent<PhotonView>().ViewID;
                        photonView.RPC("OpenChest", RpcTarget.All, myViewID);
                    }

                    // scroll view�� �ִ� �̼� ��ϵ��� ������
                    foreach (Transform child in gameRoomManager.contentTransform)
                    {
                        // �̼� ��� �����͸� ������
                        MissionObject childMissionObject = child.GetComponent<MissionObject>();

                        // ���� �̼��� ���� ���
                        // missionNumber�� ������ Ȯ��
                        if (missionObject.missionNumber == childMissionObject.missionNumber)
                        {
                            Dictionary<int, int> localMissionDict = gameRoomManager.GetMissionDictionary();

                            // ��ųʸ����� �̼� �ѹ��� ���� ID ���� ã��
                            // �������� �̼��� Ŭ���� �ϸ�, ��ųʸ��� �ش��ϴ� value ���� ������ ������
                            int targetID = -1;
                            foreach (var pair in localMissionDict)
                            {
                                if (pair.Value == missionObject.missionNumber)
                                {
                                    targetID = pair.Key;
                                    break;
                                }
                            }

                            // ���� �� �̼��� ���� ���
                            int myViewID = gameRoomManager.seekerDataObject.GetComponent<PhotonView>().ViewID;
                            if (targetID == myViewID)
                            {
                                Debug.Log("���� �� �̼��� ����!");
                                photonView.RPC("MissionClearPlayer", RpcTarget.All, targetID, 20);
                                // �� �̼��� ������, �ٸ� ������ ���� �̼ǵ� scroll view content�� �߰�
                                foreach (var pair in localMissionDict)
                                {
                                    if (pair.Value < 10 && pair.Value != missionObject.missionNumber)
                                    {
                                        gameRoomManager.UpdateMissionText(pair.Value);
                                    }
                                }
                            }
                            // ���� �� �̼��� ���� ���
                            else
                            {
                                Debug.Log("���� �� �̼��� ����!");
                                photonView.RPC("MissionClearPlayer", RpcTarget.All, targetID, 21);
                            }

                            // �ش� child mission number�� ���� ��� ��� 21�� ����, text�� ���������� ����, RPC
                            photonView.RPC("SetMissionTextFailed", RpcTarget.All, missionObject.missionNumber);

                            // �ش� scroll view�� �̼��� mission number�� 20, text�� �ʷϻ����� ����
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

            // ����� �̼� Ŭ���� Ƚ�� ���??

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
            // �ڽ��� ���� ó�� ����
            int myViewID = gameRoomManager.seekerDataObject.GetComponent<PhotonView>().ViewID;
            photonView.RPC("KillPlayer", RpcTarget.All, myViewID);
        }
    }
}
