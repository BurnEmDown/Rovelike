using GameEvents;
using UnityEngine;
using UnityCoreKit.Runtime.Core.Services;
using UnityCoreKit.Runtime.Core.Interfaces;

namespace Logging
{
    public class TileClickLogger : MonoBehaviour
    {
        private void OnEnable()
        {
             CoreServices.Get<IEventListenerManager>().AddListener(this, GameEventType.TileClicked, OnTileClicked);
        }

        private void OnDisable()
        {
            CoreServices.Get<IEventListenerManager>().RemoveListener(this, GameEventType.TileClicked, OnTileClicked);
        }

        private void OnTileClicked(object eventData)
        {
            if (eventData is TileClickedEventData data)
            {
                Debug.Log($"Tile {data.TypeKey} clicked at position: {data.TilePosition}, World position: {data.WorldPosition}");
            }
        }
    }
}
