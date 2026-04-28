using ExitGames.Client.Photon;
using MonkePhone.Tools;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MonkePhone.Networking
{
    public class NetworkHandler : MonoBehaviourPunCallbacks
    {
        public static volatile NetworkHandler Instance;

        public Action<NetPlayer, Dictionary<string, object>> OnPlayerPropertyChanged;

        private readonly Dictionary<string, object> properties = [];
        private bool set_properties = false;
        private float properties_timer;

        public void Awake() => Instance = this;

        public void Start()
        {
            if (NetworkSystem.Instance && NetworkSystem.Instance is NetworkSystemPUN)
            {
                SetProperty("Version", Constants.Version);

                PhotonNetwork.AddCallbackTarget(this);
                Application.quitting += () => PhotonNetwork.RemoveCallbackTarget(this);
                return;
            }

            enabled = false; 
        }

        public void FixedUpdate()
        {
            properties_timer -= Time.fixedDeltaTime;

            if (set_properties && properties.Count > 0 && properties_timer <= 0)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new()
                {
                    {
                        Constants.CustomProperty,
                        new Dictionary<string, object>(properties)
                    },
                    {
                        Constants.OurCustomProperty,
                        Constants.OurCustomProperty
                    }
                });

                set_properties = false;
                properties_timer = 0.225f;
            }
        }

        public void SetProperty(string key, object value)
        {
            if (properties.ContainsKey(key))
            {
                properties[key] = value;
                Logging.Info($"Updated network key - {key}: {value}");
            }
            else
            {
                properties.Add(key, value);
                Logging.Info($"Added network key - {key}: {value}");
            }
            set_properties = true;
        }

        public void RemoveProperty(params string[] key)
        {
            foreach (string NetKey in key)
            {
                if (properties.ContainsKey(NetKey))
                    properties.Remove(NetKey);

                Logging.Log($"Removed network key - {NetKey}");
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            NetPlayer netPlayer = NetworkSystem.Instance.GetPlayer(targetPlayer.ActorNumber);

            if (netPlayer.IsLocal || !VRRigCache.Instance.TryGetVrrig(netPlayer, out RigContainer playerRig) || !playerRig.TryGetComponent(out NetworkedPlayer networkedPlayer))
                return;

            if (changedProps.TryGetValue(Constants.CustomProperty, out object props_object) && props_object is Dictionary<string, object> properties)
            {
                networkedPlayer.HasMonkePhone = true;

                Logging.Info($"Recieved properties from {netPlayer.NickName}: {string.Join(", ", properties.Select(prop => $"[{prop.Key}: {prop.Value}]"))}");
                OnPlayerPropertyChanged?.Invoke(netPlayer, properties);
            }
        }
    }
}