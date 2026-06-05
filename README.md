# Pathfinding

### 프로젝트 설명
- 경로탐색 알고리즘 A*, HPA*(hierarchical pathfinding a*), HPA* + PathSmoothing(Line Of Sight)를 구현, 비교했습니다.
- 고수준 Cluster경로를 바탕으로 오브젝트의 이동에 따른 Lazy Refine을 구현했습니다.

#### 비교군
- 경로 길이 : 노드간 이동 비용 1, smoothing된 경로는 실제 거리(Euclidean distance)
- 소요 시간 : pathfinding 중 탐색한 노드의 수
- 메모리 사용량 : pathfinding 중 사용한 collection들의 크기의 합

### 시연 방법
#### 조작법
- wasd키를 통해 이동할 수 있습니다.
- 마우스 휠을 통해 줌인,아웃이 가능합니다.

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
