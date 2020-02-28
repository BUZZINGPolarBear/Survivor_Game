using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_controller : MonoBehaviour
{
    [SerializeField]//이걸 선언하면 private가 보이지 않으면서도(보호되면서) 값을 수정할 수 있게 된다.
    private float walkSpeed;
    [SerializeField]
    private float lookSensitivity;
    [SerializeField]
    private float cameraRotationLimit;//고개가 과하게 젖혀지지 않도록 조절함.
    private float currentCameraRotationX = 0f;//정면을 바라보도록 초기화함.

    [SerializeField]
    private Camera theCamera;

    private Rigidbody myRigid;// Rigidbody를 선언하면 캐릭터의 몸의 충돌영역이 생긴다.

    /* private를 넣어주면 유니티 내의 인스펙터 창에서 값이 보이지 않는다. <---> public
    막 접근할 수 없도록 스크립트 창에서만 수정되고, 필요시 유니티엔진에서도 수정 할 수 있게 함.*/
    // Start is called before the first frame update
    void Start()
    {
        myRigid = GetComponent<Rigidbody>(); 
    }

    // Update is called once per frame
    void Update()
    {
        move();
        CameraRotation();
        CharacterRotation();
    }

    private void move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");//좌우 화살표 혹은 A,D키 입력을 감지하여 _moveDirx변수 안에 넣는다. 오른쪽:1 왼쪽:-1 안누르면 0 리턴함.
        float _moveDirZ = Input.GetAxisRaw("Vertical");//Unity에서는 Z가 앞뒤 움직임을 나타냄.


        Vector3 _moveHorizontal = transform.right * _moveDirX;//기본값 (1,0,0)
        Vector3 _moveVertiacal = transform.forward * _moveDirZ;//기본값 (0,0,1)
        Vector3 _velocity = ((_moveHorizontal + _moveVertiacal).normalized * walkSpeed);//normalized를 해줌으로써 두 값을 더한 값이 2가 아닌 1이 된다. 유니티에서 권장하는 방식임.

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);//델타 타임 == 순간이동하지 않도록 선형적으로 움직이게 해줌. deltatime은 약 0.016초임.


        /*Vector3로 새게의 float 변수를 저장하는 공간을 만든다. 
         * transform으로 유니티 환경 내의 좌표를 받아 유저가 입력한 값을 곱해 플레이어의 위치값을 바꾼다.
         */ 
         
    }

    private void CameraRotation()
    {
        //상하 카메라 회전
        float _xRotation = Input.GetAxisRaw("Mouse Y"); // 마우스는 2차원이므로 xrotation에 마우스의 y값을 넣어준다.
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);// 화면이 돌아가는 각을 가둔다. -45도와 +45도 사이에 갖히도록 설정된 것임. 
        //만약 최댓값을 넘거나 최솟값보다 낮다면 최대값이나 최솟값으로 고정된다. (80의 값이 들어와도 최댓값인 45도까지만 바라보도록 설정됨.)
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    private void CharacterRotation()
    {
        //좌우 캐릭터 회전
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f)*lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }
}
