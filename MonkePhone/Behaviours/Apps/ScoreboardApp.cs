using MonkePhone.Behaviours;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine;
using MonkePhone.Extensions;
using System.Collections.Generic;
using GorillaNetworking;
using System;

namespace MonkePhone.Behaviours.Apps
{
	public class ScoreboardApp : PhoneApp
	{
		public override string AppId => "Scoreboard";

		private List<Text> _playerNameTexts = new List<Text>();
		private List<Image> _playerSwatches = new List<Image>();
		private List<GameObject> _playerLines = new List<GameObject>();
		private Text _roomIdText;
		private Transform _grid;
		private GameObject _templateLine;
		private const int MaxPlayers = 10;

		public override void Initialize()
		{
			_grid = FindChild(transform, "Grid");
			if (_grid == null) return;

			_templateLine = FindChild(_grid, "Person (1)")?.gameObject;
			if (_templateLine == null) return;

			_roomIdText = FindChild(transform, "Header (1)")?.GetComponent<Text>();
			if (_roomIdText == null)
				_roomIdText = FindChild(transform, "RoomInfo")?.GetComponent<Text>();

			_templateLine.SetActive(false);
			_playerNameTexts.Clear();
			_playerSwatches.Clear();
			_playerLines.Clear();

			for (int i = 0; i < MaxPlayers; i++)
			{
				GameObject line = i == 0 ? _templateLine : Instantiate(_templateLine, _grid);
				line.name = $"Person ({i + 1})";

				Transform nameTransform = FindChild(line.transform, "Name");
				Transform swatchTransform = FindChild(line.transform, "Swatch");

				if (nameTransform != null)
					_playerNameTexts.Add(nameTransform.GetComponent<Text>());

				if (swatchTransform != null)
					_playerSwatches.Add(swatchTransform.GetComponent<Image>());

				_playerLines.Add(line);
				line.SetActive(false);
			}
		}

		private Transform FindChild(Transform parent, string name)
		{
			Transform direct = parent.Find(name);
			if (direct != null) return direct;

			Transform withBrackets = parent.Find($"[1] {name}");
			if (withBrackets != null) return withBrackets;

			foreach (Transform child in parent)
			{
				if (child.name.Contains(name))
					return child;
			}

			return null;
		}

		public override void AppOpened()
		{
			base.AppOpened();

			RoomSystem.JoinedRoomEvent += UpdateBoard;
			RoomSystem.LeftRoomEvent += UpdateBoard;
			RoomSystem.PlayerJoinedEvent += OnPlayerChanged;
			RoomSystem.PlayerLeftEvent += OnPlayerChanged;

			UpdateBoard();
		}

		public override void AppClosed()
		{
			base.AppClosed();

			RoomSystem.JoinedRoomEvent -= UpdateBoard;
			RoomSystem.LeftRoomEvent -= UpdateBoard;
			RoomSystem.PlayerJoinedEvent -= OnPlayerChanged;
			RoomSystem.PlayerLeftEvent -= OnPlayerChanged;
		}

		private void OnPlayerChanged(NetPlayer player) => UpdateBoard();

		public void UpdateBoard()
		{
			UpdatePlayerList();
			UpdateRoomInfo();
		}

		private void SetSwatchColor(Image swatch, VRRig rig)
		{
			if (rig == null || swatch == null)
			{
				if (swatch != null)
				{
					swatch.color = Color.white;
					swatch.material = null;
				}
				return;
			}

			int setMatIndex = rig.setMatIndex;
			Material material;
			Color color;

			if (setMatIndex == 0)
			{
				material = rig.scoreboardMaterial;
				color = rig.playerColor;
			}
			else
			{
				material = rig.materialsToChangeTo[setMatIndex];
				color = material.color;
			}

			if (swatch.material != material)
				swatch.material = material;

			if (swatch.color != color)
				swatch.color = color;
		}

		private void UpdatePlayerList()
		{
			if (_playerLines.Count == 0) return;

			NetPlayer[] players = NetworkSystem.Instance.AllNetPlayers;
			Array.Sort(players, (x, y) => x.ActorNumber.CompareTo(y.ActorNumber));

			for (int i = 0; i < _playerLines.Count; i++)
			{
				if (i < players.Length && PhotonNetwork.InRoom)
				{
					NetPlayer player = players[i];

					if (player == null || player.IsNull || (!player.IsLocal && !player.InRoom))
					{
						_playerLines[i].SetActive(false);
						continue;
					}

					_playerLines[i].SetActive(true);

					VRRig rig = null;
					if (GorillaParent.instance.vrrigDict.TryGetValue(player, out VRRig foundRig))
					{
						rig = foundRig;
					}

					if (i < _playerNameTexts.Count && _playerNameTexts[i] != null)
					{
						_playerNameTexts[i].text = player.GetName();
						_playerNameTexts[i].color = rig != null ? rig.playerText1.color : Color.white;
					}

					if (i < _playerSwatches.Count && _playerSwatches[i] != null)
					{
						SetSwatchColor(_playerSwatches[i], rig);
					}
				}
				else
				{
					_playerLines[i].SetActive(false);
				}
			}
		}

		private void UpdateRoomInfo()
		{
			if (_roomIdText == null) return;

			_roomIdText.text = PhotonNetwork.CurrentRoom.GetRoomInfo();
		}

		
	}
}