using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using UnityEngine;

public class SeekerGameMove : MonoBehaviourPunCallbacks
{
    public bl_Joystick js;
    public float speed;

    private Animator seekerani;
    private Rigidbody2D rb;

    void Start()
    {
        Debug.Log("연결완료");
        rb = GetComponent<Rigidbody2D>();
        seekerani = GetComponent<Animator>();
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
    }

   /* public void Update()
    {
        if (photonView.IsMine)
        {
            Vector3 dir = new Vector3(js.Horizontal, js.Vertical, 0);
            dir.Normalize();

            if (dir.sqrMagnitude > 0)
            {
                seekerani.SetBool("isMove", true);
                photonView.RPC("Flip", RpcTarget.All, dir.x);
                rb.velocity = dir * speed; // Rigidbody2D의 속도를 변경
            }
            else
            {
                seekerani.SetBool("isMove", false);
                rb.velocity = Vector2.zero; // 속도를 0으로 설정
            }
        }
    }*/


    //pc버전 움직임 테스트용 8방향 방향키 컨트롤 
    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            float moveHorizontal = 0.0f;
            float moveVertical = 0.0f;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                moveVertical += 1.0f;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                moveVertical -= 1.0f;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                moveHorizontal += 1.0f;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                moveHorizontal -= 1.0f;
            }

            // Normalize vector if both horizontal and vertical movement are present to maintain constant speed
            Vector2 movement = new Vector2(moveHorizontal, moveVertical); // Use Vector2 for 2D movement
            if (moveHorizontal != 0.0f && moveVertical != 0.0f)
            {
                movement.Normalize();
            }

            rb.velocity = movement * speed;

            // Check if the velocity is zero
            if (rb.velocity != Vector2.zero)
            {
                seekerani.SetBool("isMove", true);
                photonView.RPC("Flip", RpcTarget.All, movement.x);
            }
            else
            {
                seekerani.SetBool("isMove", false);
            }
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
