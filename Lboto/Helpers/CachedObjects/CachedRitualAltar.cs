using DreamPoeBot.Loki.Game.Objects;
using Lboto.Helpers.Positions;
using System.Collections.Generic;

namespace Lboto.Helpers
{
    public class CachedRitualAltar : CachedObject
    {
        private NetworkObject _object;
        public bool Interacted => _state == 2;
        public bool Active => _state == 1;

        public List<CachedObject> AltarMonsters = new List<CachedObject>();

        private int? _cachedState;
        private int _state
        {
            get
            {
                if (_object == null)
                    return _cachedState ?? 0; // Return last known state

                _cachedState = _object.Components.StateMachineComponent.Encounter_Started == false ? 0 :
                              _object.Components.StateMachineComponent.Encounter_Finished == false ? 1 : 2;
                return _cachedState.Value;
            }
        }
        //0,1,2 0 not interacted 1 active 2 finished

        public CachedRitualAltar(int id, WalkablePosition position)
            : base(id, position)
        {
            _object = GetObject();
        }

        public new NetworkObject Object => GetObject();

        /// Metadata/Terrain/Leagues/Ritual/RitualRuneObject
        /// DreamPoeBot.Loki.Game.Objects.NetworkObject

    }
}
