# Pathfinding

### 프로젝트 설명
- 경로탐색 알고리즘 A*, HPA*(hierarchical pathfinding a*), HPA* + PathSmoothing(Line Of Sight)를 구현, 비교했습니다.
- 고수준 Cluster경로를 바탕으로 오브젝트의 이동에 따른 Lazy Refine을 구현했습니다.
- 구현한 경로 탐색을 오브젝트에 적용하고 다수의 유닛의 이동을 위해 Steering Behaviour를 적용했습니다.

#### 비교군
- 경로 길이 : 노드간 이동 비용 1, smoothing된 경로는 실제 거리(Euclidean distance)
- 소요 시간 : pathfinding 중 탐색한 노드의 수
- 메모리 사용량 : pathfinding 중 사용한 collection들의 크기의 합

### 시연 방법
#### 씬 구성
- Pathfinding : pathfinding 방식에 따른 경로와 성능을 비교합니다.
- ContorllUnit : pathfinding에 따라 움직이는 유닛을 컨트롤 합니다.

#### 조작법
키보드 마우스
- 화면 이동 : 마우스를 화면 외각에 위치하면 화면이 이동합니다.
- 줌인, 아웃 : 마우스 휠을 통해 줌인, 아웃을 작동할 수 있습니다.
- 유닛 이동 : 유닛을 선택한 상태에서 우클릭을 통해 유닛을 이동시킵니다.
- 유닛 추가 선택 : Shift를 누른 상태로 좌클릭을 이용해 유닛을 추가 선택할 수 있습니다.
- 유닛 이동 예약 : Shift를 누른 상태로 우클릭을 이용해 이동을 예약시킵니다.

터치
- 화면 이동 : 화면 외각을 터치하고 유지시키면 화면이 이동합니다.
- 줌인, 아웃 : 두 손가락으로 드래그 해 줌인, 아웃을 작동할 수 있습니다.
- 유닛 이동 : 유닛을 선택한 상태에서 빈 공간을 터치해 유닛을 이동시킵니다.
- 유닛 추가 선택 : Shift UI가 활성화 된 상태에서 유닛을 터치해 추가 선택합니다.
- 유닛 이동 예약 : Shift UI가 활성화 된 상태에서 빈 공간을 터치해 이동을 예약시킵니다.

#### 맵 생성
- 우측 상단 (Generate Map) 버튼을 눌러 맵을 생성합니다. MapSize와 ClusterSize를 입력할 수 있습니다. 
- 입력하지 않은 경우 20 * 20크기의 맵과 5*5크기의 Cluster들이 생성됩니다.

#### 맵 배치
- 각 노드를 클릭해 시작점(Unit), 도착점(Dest), 장애물(Obst), 빈공간(Room)을 배치할 수 있습니다.
- 시작점과 도착점이 설정되었다면 우측 하단 (Find All Path) 버튼을 눌러 경로를 탐색합니다.

#### 경로 확인
- 우측 상단 (Show Result) 버튼을 눌러 알고리즘별 경로 탐색 결과를 비교할 수 있습니다.
- 우측 하단 버튼들을 눌러 알고리즘별 경로와 Lazy Refine를 시각적으로 확인할 수 있습니다.

### 시연 링크 : https://play.unity.com/en/games/6f736cb8-fa4a-44e8-b04a-6945b54def50/webgl-builds
