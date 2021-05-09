using System;
using MonoGame.Extended.Collections;

namespace MonoGame.Extended.Entities
{
    internal class EntitySubscription : IDisposable
    {
        private readonly Bag<int> _activeEntities;
        private readonly Bag<int> _changedEntities;
        private readonly EntityManager _entityManager;
        private readonly Aspect _aspect;
        private bool _rebuildActives;

        internal EntitySubscription(EntityManager entityManager, Aspect aspect)
        {
            _entityManager = entityManager;
            _aspect = aspect;
            _activeEntities = new Bag<int>(entityManager.Capacity);
            _changedEntities = new Bag<int>(entityManager.Capacity);
            _rebuildActives = true;

            _entityManager.EntityAdded += OnEntityAdded;
            _entityManager.EntityRemoved += OnEntityRemoved;
            _entityManager.EntityChanged += OnEntityChanged;
        }

        private void OnEntityAdded(int entityId)
        {
            if (_activeEntities.Contains(entityId))
            {
                if (_aspect.IsInterested(_entityManager.GetComponentBits(entityId)))
                    return;

                _activeEntities.Remove(entityId);
            }
            else
            {
                if (!_aspect.IsInterested(_entityManager.GetComponentBits(entityId)))
                    _activeEntities.Add(entityId);
            }
        }

        private void OnEntityRemoved(int entityId) => _activeEntities.Remove(entityId);
        private void OnEntityChanged(int entityId) => _rebuildActives = true;

        public void Dispose()
        {
            _entityManager.EntityAdded -= OnEntityAdded;
            _entityManager.EntityRemoved -= OnEntityRemoved;
            _entityManager.EntityChanged -= OnEntityChanged;
        }

        public Bag<int> ActiveEntities
        {
            get
            {
                if (_rebuildActives)
                    RebuildActives();

                return _activeEntities;
            }
        }

        private void RebuildActives()
        {
            foreach (var entity in _changedEntities)
            {
                OnEntityAdded(entity);
            }

            _changedEntities.Clear();
            _rebuildActives = false;
        }
    }
}
