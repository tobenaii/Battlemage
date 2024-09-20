using Unity.Collections;
using Unity.Entities;
using Waddle.GameplayBehaviours.Data;
using Waddle.GameplayEffects.Data;

namespace Waddle.GameplayEffects.Extensions
{
    public ref struct GameplayEffectBuilder
    {
        private GameplayState _gameplayState;
        private NativeList<GameplayAttributeModifier> _attributeModifiers;
        private NativeList<GameplayTagModifier> _tagModifiers;

        public static GameplayEffectBuilder Create(GameplayState gameplayState)
        {
            var builder = new GameplayEffectBuilder
            {
                _gameplayState = gameplayState,
                _attributeModifiers = new NativeList<GameplayAttributeModifier>(Allocator.Temp),
                _tagModifiers = new NativeList<GameplayTagModifier>(Allocator.Temp)
            };
            return builder;
        }

        public GameplayEffectBuilder WithAttributeModifier(byte attribute,
            GameplayAttributeModifier.Operation operation, float value)
        {
            _attributeModifiers.Add(new GameplayAttributeModifier
            {
                ModAttribute = attribute,
                OperationType = operation,
                Value = value
            });
            return this;
        }

        public GameplayEffectBuilder WithTagModifier(ComponentType tag, GameplayTagModifier.Operation operation)
        {
            _tagModifiers.Add(new GameplayTagModifier()
            {
                Tag = tag.TypeIndex,
                OperationType = operation
            });
            return this;
        }

        public void Apply(Entity source, Entity target, float duration = 0)
        {
            if (!_gameplayState.IsServer) return;

            var effectPrefab = _gameplayState.GetSingleton<GameplayEffectPrefab>().Value;
            var effectInstance = _gameplayState.Instantiate(effectPrefab);

            var attributeModifiers = _gameplayState.GetBuffer<GameplayAttributeModifier>(effectInstance);
            var tagModifiers = _gameplayState.GetBuffer<GameplayTagModifier>(effectInstance);

            _gameplayState.SetComponent(effectInstance, new GameplayEffect
            {
                Source = source,
                Target = target,
                Duration = duration
            });

            attributeModifiers.AddRange(_attributeModifiers.AsArray());
            tagModifiers.AddRange(_tagModifiers.AsArray());

            _attributeModifiers.Dispose();
            _tagModifiers.Dispose();
        }
    }
}