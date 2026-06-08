# [Zenga](Zenga)
### [`Board.cs`](Zenga/Board.cs)
젠가 타워를 구성하고, 수 두기, 수 유효성 인증 등 기본 기능
### [`Balancing.cs`](Zenga/Balancing.cs)
젠가 타워의 안정성을 구하기 위한 기본 요소와 무게중심 모델
### [`ZengaStrings.cs`](Zenga/ZengaStrings.cs)
젠가 타워를 문자열로 바꾸거나 반다로 바꿔주는 기능
# [JengaSim](JengaSim/Assets)
## [JengaSim/Balancing](JengaSim/Assets/Balancing)
`Balancing.cs`를 유니티에서 사용할 수 있도록 개량한 파일들
## [JengaSim/Experiment](JengaSim/Assets/Experiment)
유니티 실험 내용
### [`BlockObserver.cs`](JengaSim/Assetes/Experiment/BlockObserver.cs)
각 젠가 블록에 할당되는 스크립트, 할당된 블록의 상태 관찰
### [`Experiment.cs`](JengaSim/Assets/Experiment/Experiment.cs)
하나의 젠가 타워를 실험하는 스크립트, 실험을 시작하고 결과를 기록
### [`Lab.cs`](JengaSim/Assets/Experiment/Lab.cs)
실험 전체를 진행하는 스크립트, 모든 실험을 진행하고, 매 실험 새 씬을 만들어서 초기화 함
### [`TowerBuilder.cs`](JengaSim/Assets/Experiment/TowerBuilder.cs)
실험하기 위해 타워를 입력받고 유니티에서 지음
### [`Utility.cs`](JengaSim/Assets/Experiment/Utility.cs)
실험에 쓰일 새 타워를 생성하고, 결과를 파일에 쓰고 읽음
# [DepthSearch](DepthSearch)
### [`cruncher.py`](DepthSearch/cruncher/cruncher.py)
결과를 읽어서 산포도로 나타냄
