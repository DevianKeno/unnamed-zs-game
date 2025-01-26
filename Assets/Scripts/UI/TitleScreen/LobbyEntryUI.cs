using System;

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

using Epic.OnlineServices.Lobby;

using UZSG.EOS.Lobbies;
using UnityEngine.UI;

namespace UZSG.UI.Lobbies
{
    public class LobbyEntryUI : MonoBehaviour, IPointerUpHandler
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

        public void OnPointerUp(PointerEventData eventData)
        {
            OnClick?.Invoke(this, new());
        }

        public void SetLobbyInfo(Lobby lobby, LobbyDetails lobbyDetails)
        {
            if (lobby == null || lobbyDetails == null) return;

            Lobby = lobby;
            LobbyDetails = lobbyDetails;
            string ownerName = string.Empty;
            string worldName = string.Empty;
            string levelDisplayName = string.Empty;
            string gameVersion = string.Empty;
            string maxPlayers = "-";
            string playerCount = "-"; 

            if (lobby.OwnerProductUserId != null && lobby.OwnerProductUserId.IsValid())
            {
                ownerName = lobby.LobbyOwnerDisplayName;
            }
            if (lobby.TryGetAttribute(AttributeKeys.LOBBY_OWNER_DISPLAY_NAME, out var own))
            {
                ownerName = own.AsString;
            }
            if (lobby.TryGetAttribute(AttributeKeys.WORLD_NAME, out var wn))
            {
                worldName = wn.AsString;
            }
            if (lobby.TryGetAttribute(AttributeKeys.LEVEL_DISPLAY_NAME, out var lid))
            {
                levelDisplayName = lid.AsString;
            }
            if (lobby.TryGetAttribute(AttributeKeys.GAME_VERSION, out var gv))
            {
                gameVersion = gv.AsString;
            }

            maxPlayers = lobby.MaxNumLobbyMembers.ToString();
            playerCount = (lobby.MaxNumLobbyMembers - lobby.AvailableSlots).ToString();

            worldNameTmp.text = worldName;
            infoTmp.text = $"Host: {ownerName} | Map: {levelDisplayName} | Game Version: {gameVersion}";
            playerCountTmp.text = $"{playerCount}/{maxPlayers}";
        }

        public void SetVersionMismatch(bool value)
        {
            button.interactable = !value;
            versionMismatchText.gameObject.SetActive(value);
        }
    }
}