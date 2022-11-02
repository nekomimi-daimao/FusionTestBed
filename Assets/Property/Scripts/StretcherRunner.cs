using Cysharp.Threading.Tasks;
using Fusion;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Property
{
    public sealed class StretcherRunner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
    {
        [SerializeField]
        private NetworkRunner networkRunner;

        [SerializeField]
        private NetworkSceneManagerDefault sceneManager;

        [SerializeField]
        private Stretcher stretcherPrefab;

        [SerializeField]
        private StretchScaleFusion stretchScaleFusionPrefab;

        public ReactiveCollection<StretchScaleFusion> stretchScaleFusions = new ReactiveCollection<StretchScaleFusion>();

        private readonly ReactiveDictionary<PlayerRef, StretchScaleFusion> _stretchScaleFusions
            = new ReactiveDictionary<PlayerRef, StretchScaleFusion>();


        private void Awake()
        {
            stretchScaleFusions.ObserveAdd()
                .TakeUntilDisable(this)
                .Subscribe(AddStretch);
            this.OnDestroyAsObservable()
                .Subscribe(_ => Runner.Shutdown());
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            var startGameArgs = new StartGameArgs();
            startGameArgs.SessionName = nameof(Stretcher);
            startGameArgs.GameMode = GameMode.AutoHostOrClient;
            startGameArgs.SceneManager = sceneManager;
            startGameArgs.PlayerCount = 2;
            var start = await networkRunner.StartGame(startGameArgs);
            if (start.Ok)
            {
                Debug.Log($"Start Game Success! {start}");
            }
            else
            {
                Debug.LogError($"Start Game Failed... {start}");
            }
        }

        private void AddStretch(CollectionAddEvent<StretchScaleFusion> e)
        {
            var s = Instantiate(stretcherPrefab, e.Value.transform, false);
            var sTs = s.transform;
            sTs.localPosition = Vector3.zero;
            e.Value.GetComponent<NetworkTransform>().InterpolationTarget = sTs;
            s.Init(e.Value);
        }

        void IPlayerJoined.PlayerJoined(PlayerRef player)
        {
            Debug.Log($"PlayerJoined {player}");
            if (!networkRunner.IsServer)
            {
                return;
            }

            if (_stretchScaleFusions.ContainsKey(player))
            {
                return;
            }

            var pos = player == 0 ? Vector3.right : Vector3.left;
            var stretchScale = networkRunner.Spawn(stretchScaleFusionPrefab, pos * 5f, inputAuthority: player);
            networkRunner.SetPlayerObject(player, stretchScale.Object);
            _stretchScaleFusions[player] = stretchScale;
        }

        void IPlayerLeft.PlayerLeft(PlayerRef player)
        {
            if (!networkRunner.IsServer)
            {
                return;
            }

            if (_stretchScaleFusions.TryGetValue(player, out var stretchScale))
            {
                networkRunner.Despawn(stretchScale.Object);
                _stretchScaleFusions.Remove(player);
            }

            networkRunner.SetPlayerObject(player, null);
        }
    }
}
