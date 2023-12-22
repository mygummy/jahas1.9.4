using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SeekerMoveRoom : MonoBehaviourPunCallbacks
{
    public bl_Joystick js;
    public float speed;

    private Animator seekerani;
    void Start()
    {
        Debug.Log("연결완료");
        CameraController _cameraController = this.gameObject.GetComponent<CameraController>();
        if (_cameraController != null)
        {
            if (photonView.IsMine)
            {
                _cameraController.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }
        js = GameObject.Find("Canvas/Joystick").GetComponent<bl_Joystick>();
        seekerani = GetComponent<Animator>();
    }
    public void Update()
    {
        if (photonView.IsMine)
        {
            // 스틱이 향해있는 방향을 저장해준다.
            Vector3 dir = new Vector3(js.Horizontal, js.Vertical, 0);

            // Vector의 방향은 유지하지만 크기를 1로 줄인다. 길이가 정규화 되지 않을시 0으로 설정.
            dir.Normalize();
            if (dir.sqrMagnitude > 0)
            {
                seekerani.SetBool("isMove", true);

                photonView.RPC("Flip", RpcTarget.All, dir.x);

            }
            else
            {
                seekerani.SetBool("isMove", false);
            }

            // 오브젝트의 위치를 dir 방향으로 이동시킨다.
            transform.position += dir * speed * Time.deltaTime;
        }
    }

    [PunRPC]
    void Flip(float x)
    {
        if (x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }

        else if (x > 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }
}