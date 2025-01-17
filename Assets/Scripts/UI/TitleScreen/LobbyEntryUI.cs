using System;

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

using Epic.OnlineServices.Lobby;

using UZSG.EOS.Lobbies;
using UnityEngine.UI;

namespace UZSG.UI.Lobbies
{
    public class LobbyEntryUI : MonoBehaviour, IPointerDownHandler
    {
        public Lobby Lobby { get; private set; }
        public LobbyDetails LobbyDetails { get; private set; }

        public event EventHandler OnClick;

        [Header("Elements")]
        [SerializeField] Button button;
        [SerializeField] TextMeshProUGUI worldNameTmp;
        [SerializeField] TextMeshProUGUI infoTmp;
        [SerializeField] TextMeshProUGUI playerCountTmp;
        [SerializeField] TextMeshProUGUI versionMismatchText;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnClick?.Invoke(this, new());
        }

        public void SetLobbyInfo(Lobby lobby, LobbyDetails lobbyDetails)
        {
            if (lobby == null || lobbyDetails == null) return;

            Lobby = lobby;
            LobbyDetails = lobbyDetails;
            string worldName = string.Empty;
            string levelId = string.Empty;
            string gameVersion = string.Empty;
            string maxPlayers = string.Empty;
            string playerCount = string.Empty;

            if (lobby.TryGetAttribute(AttributeKeys.WORLD_NAME, out var wn))
            {
                worldName = wn.AsString;
            }
            if (lobby.TryGetAttribute(AttributeKeys.LEVEL_ID, out var lid))
            {
                levelId = lid.AsString;
            }
            if (lobby.TryGetAttribute(AttributeKeys.GAME_VERSION, out var gv))
            {
                gameVersion = gv.AsString;
            }
            if (lobby.TryGetAttribute(AttributeKeys.MAX_PLAYERS, out var mp))
            {
                maxPlayers = mp.AsString;
            }
            if (lobby.TryGetAttribute(AttributeKeys.PLAYER_COUNT, out var pc))
            {
                playerCount = pc.AsString;
            }

            worldNameTmp.text = worldName;
            infoTmp.text = $"Map: {levelId} | Game Version: {gameVersion}";
            playerCountTmp.text = $"{playerCount}/{maxPlayers}";
        }

        public void SetVersionMismatch(bool value)
        {
            button.interactable = !value;
            versionMismatchText.gameObject.SetActive(value);
        }
    }
}