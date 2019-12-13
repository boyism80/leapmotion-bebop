# Leapmotion & Bebop

Leapmotion을 이용하여 Parrot Bebop2를 조작하고 이를 오큘러스VR로 관측할 목적의 어플리케이션입니다.

유니티를 이용하여 제작하였고 LeapmotionSDK를 이용했습니다.

Parrot Bebop2의 SDK는 C#용으로 나온게 없어서 [이곳](https://github.com/u10116032/Bebop2-csharp-SDK)을 참고했습니다.

총 3가지 화면(립모션 적외선 카메라, 드론 카메라, 유니티 인게임 화면)을 볼 수 있습니다.

졸업과제로 하려던 작품이었는데 교수님께서 거절하기도 하셨고, 동작하기 위해 필요한 준비물들이 너무 비싸고 복잡한 부분도 있어서 더 멋지게 만들지는 못했지만 묻히기 아까워서 저장해둔 프로젝트입니다.

## 튜토리얼 가이드
[Leapmotion Blocks](https://www.youtube.com/watch?v=EJWuD-GWIkU)를 벤치마킹했습니다.

VR의 회전정보와 leapmotion의 데이터를 프레임단위로 저장하여 리플레이를 동작시키는 개념으로 구현했습니다.

## Bebop2 작동
Parrot Bebop2은 D2C(Drone to Client), C2D(Client to Drone) 2개의 소켓을 이용해 통신합니다.

배터리상태, GPS정보, 고도, 회전 등의 정보는 D2C, 회전명령, 이동명령 등의 명령을 줄때는 C2D를 이용합니다.

드론의 스트리밍 영상은 opencv를 이용했습니다. (정보가 너무 없었는데 구글링으로 찾아냄. sdp를 단순히 VideoCapture하면 된다고 함)

## 실행결과
* [유튜브 영상 보기](https://youtu.be/Kb_JobdWZ70)
