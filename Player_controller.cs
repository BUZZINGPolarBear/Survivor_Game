using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_controller : MonoBehaviour
{
    [SerializeField]//이걸 선언하면 private가 보이지 않으면서도(보호되면서) 값을 수정할 수 있게 된다.
    private float walkSpeed;

    [SerializeField]
    private float runSpeed;
    private float applySpeed;//이 변수에 walkspeed 혹은 runspeed를 넣어서 캐릭터의 속도를 결정한다.

    [SerializeField]
    private float lookSensitivity;

    [SerializeField]
    private float cameraRotationLimit;//고개가 과하게 젖혀지지 않도록 조절함.
    private float currentCameraRotationX = 0f;//정면을 바라보도록 초기화함.

    [SerializeField]
    private Camera theCamera;

    [SerializeField]
    private float jumpForce;

    [SerializeField]
    private float crouchSpeed;

    [SerializeField]
    private float crouchPosY;//얼마나 앉을 지 결정하는 변수
    private float originPosY;
    private float applyCrouchPosY;

    //캐릭터 상태 변수
    private bool isRun = false;// 뛰고있는지 안 뛰고 있는지 check
    private bool isGround = true;// 점프했는지 안했는지 check
    private bool isCrouch = false;

    private Rigidbody myRigid;// Rigidbody를 선언하면 캐릭터의 몸의 충돌영역이 생긴다.
    private CapsuleCollider capsuleCollider;//땅과 캡슐의 접지 여부를 check.

    /* private를 넣어주면 유니티 내의 인스펙터 창에서 값이 보이지 않는다. <---> public
    막 접근할 수 없도록 스크립트 창에서만 수정되고, 필요시 유니티엔진에서도 수정 할 수 있게 함.*/
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y;
        //캐릭터를 앉히는게 아니라 카메라를 내린다. 카메라의 로컬 포지션을 기준으로 함.
        applyCrouchPosY = originPosY;
    }

    // Update is called once per frame
    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        move();
        CameraRotation();
        CharacterRotation();
    }

    //지면에 붙어있는지 확인
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y+0.1f);
        //오차를 수정하기 위해 약간의 값을 더해준다.
    }
    //점프 시도
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround == true)
        {
            Jump();
        }
    }
    //점프
    private void Jump()
    {
        //앉아있는 상태에서 점프를 하면 서있는 상태로 바뀜.
        if (isCrouch) Crouch();

        myRigid.velocity = transform.up * jumpForce;
    }
    //달리기 시도
    private void TryRun()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
                     
    }
    //달리기
    private void Running()
    {
        isRun = true;
        //앉아있는 상태에서 달리기를 하면 서있는 상태로 바뀜.
        if (isCrouch) Crouch();
        applySpeed = runSpeed;
    }

    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    private void move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        //좌우 화살표 혹은 A,D키 입력을 감지하여 _moveDirx변수 안에 넣는다. 오른쪽:1 왼쪽:-1 안누르면 0 리턴함.
        float _moveDirZ = Input.GetAxisRaw("Vertical");//Unity에서는 Z가 앞뒤 움직임을 나타냄.


        Vector3 _moveHorizontal = transform.right * _moveDirX;//기본값 (1,0,0)
        Vector3 _moveVertiacal = transform.forward * _moveDirZ;//기본값 (0,0,1)
        Vector3 _velocity = ((_moveHorizontal + _moveVertiacal).normalized * applySpeed);
        //normalized를 해줌으로써 두 값을 더한 값이 2가 아닌 1이 된다. 유니티에서 권장하는 방식임.

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
        //델타 타임 == 순간이동하지 않도록 선형적으로 움직이게 해줌. deltatime은 약 0.016초임.


        /*Vector3로 새게의 float 변수를 저장하는 공간을 만든다. 
         * transform으로 유니티 환경 내의 좌표를 받아 유저가 입력한 값을 곱해 플레이어의 위치값을 바꾼다.
         */ 
         
    }

    //상하 카메라 회전
    private void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y"); // 마우스는 2차원이므로 xrotation에 마우스의 y값을 넣어준다.
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);
        // 화면이 돌아가는 각을 가둔다. -45도와 +45도 사이에 갖히도록 설정된 것임. 
        //만약 최댓값을 넘거나 최솟값보다 낮다면 최대값이나 최솟값으로 고정된다.
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    //좌우 캐릭터 회전
    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f)*lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }
    private void TryCrouch()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl)&&isGround)
        {
            Crouch();
        }
    }
    //앉기
    private void Crouch()
    {
        isCrouch = !isCrouch;//상태를 맞바꾸는 구간임.
        if(isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }
        StartCoroutine(CrouchCoroutine());
    }

    IEnumerator CrouchCoroutine()//컴파일러가 병렬처리 하는 것 처럼 보이게 하는 코로틴
    {
        float _posY = theCamera.transform.localPosition.y;
        int cnt = 0;
        while(_posY != applyCrouchPosY)
        {
            cnt++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (cnt > 15) break;
            yield return null;//1프레임씩 대기.
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0);
    }
}
