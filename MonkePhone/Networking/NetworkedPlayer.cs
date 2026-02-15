using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonkePhone.Behaviours;
using MonkePhone.Interfaces;
using MonkePhone.Models;
using MonkePhone.Patches;
using MonkePhone.Tools;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace MonkePhone.Networking
{
    [RequireComponent(typeof(VRRig)), DisallowMultipleComponent]
    public class NetworkedPlayer : MonoBehaviour, IPhoneAnimation
    {
        public static volatile NetworkedPlayer Instance;

        private DummyPhoneManager _dummyPhoneManager;

        public NetPlayer Owner;

        public VRRig Rig;

        public bool InRange => Vector3.Distance(Camera.main.transform.position, transform.position) < 5f;

        public bool HasMonkePhone;

        public ObjectGrabbyState State { get; set; } = ObjectGrabbyState.Ignore;
        public bool UseLeftHand { get; set; }
        public float InterpolationTime { get; set; }
        public Vector3 GrabPosition { get; set; }
        public Quaternion GrabQuaternion { get; set; }

        private bool _isLeftHand;

        public byte GrabData;

        public float Zoom;

        public float _ownerTimeOffset;

        public bool Flipped;

        public GameObject Phone, _MonkeGramApp;
        private MeshRenderer _meshRenderer;

        // Camera
        private RenderTexture _renderTexture;
        private RawImage _background;
        private Camera _camera;

        // Time
        private Text _topBarText, _lockScreenText, _lockScreenDateText;

        private Task createPhoneTask;

        public void Start()
        {
            NetworkHandler.Instance.OnPlayerPropertyChanged += OnPlayerPropertyChanged;
            RigLocalInvisiblityPatch.OnSetInvisibleToLocalPlayer += OnLocalInvisibilityChanged;

            float _UTCOffest = (float)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes;
            NetworkHandler.Instance.SetProperty("PhoneTimeOffset", _UTCOffest);

            if (!HasMonkePhone && Owner is PunNetPlayer punPlayer && punPlayer.PlayerRef is Player playerRef)
                NetworkHandler.Instance.OnPlayerPropertiesUpdate(playerRef, playerRef.CustomProperties);
        }

        public void OnDestroy()
        {
            NetworkHandler.Instance.OnPlayerPropertyChanged -= OnPlayerPropertyChanged;
            RigLocalInvisiblityPatch.OnSetInvisibleToLocalPlayer -= OnLocalInvisibilityChanged;

            if (HasMonkePhone)
            {
                HasMonkePhone = false;
                Destroy(Phone);
            }
        }

        public async void OnPlayerPropertyChanged(NetPlayer player, Dictionary<string, object> properties)
        {
            if (player == Owner)
            {
                Logging.Info($"{player.NickName} got properties: {string.Join(", ", properties.Select(prop => $"[{prop.Key}: {prop.Value}]"))}");

                if (Phone is null)
                {
                    createPhoneTask ??= CreateMonkePhone(properties);
                    await createPhoneTask;
                }

                if (properties.TryGetValue("Grab", out object objectForGrab) && objectForGrab is byte grab)
                {
                    GrabData = grab;
                }

                if (properties.TryGetValue("Zoom", out object objectForZoom) && objectForZoom is float zoom)
                {
                    Zoom = zoom;
                }

                if (properties.TryGetValue("Flip", out object objectForFlipped) && objectForFlipped is bool flip)
                {
                    Flipped = flip;
                }

                if (properties.TryGetValue("PhoneTimeOffset", out object _timeoffset) && _timeoffset is float _offsetMins)
                {
                    _ownerTimeOffset = _offsetMins;
                }

                if (properties.TryGetValue("Version", out object value))
                {
                    if (value.ToString() == "1.0.6")
                    {
                        // devs version so keep it on the camera app4
                        _dummyPhoneManager.OpenDummyApp("MonkeGramApp");
                    }
                    else if (value.ToString() == Constants.Version)
                    {
                        // our version enable custom networking
                        _dummyPhoneManager.CloseDummyApps();
                        ScreenNetworking(properties);
                    }
                }

                ConfigurePhone();
            }
        }

        private void OnLocalInvisibilityChanged(VRRig targetRig, bool isInvisible)
        {
            if (targetRig is null || Phone is null || targetRig != Rig)
                return;

            Phone.SetActive(!isInvisible);
        }


        public async Task CreateMonkePhone(Dictionary<string, object> properties)
        {
            Phone = Instantiate(await AssetLoader.LoadAsset<GameObject>(Constants.NetPhoneName));

            _dummyPhoneManager = Phone.AddComponent<DummyPhoneManager>();
            _dummyPhoneManager.Initialize(Phone);

            Phone.SetActive(!Rig.IsInvisibleToLocalPlayer);
            Phone.transform.localEulerAngles = Vector3.zero;

            _meshRenderer = Phone.transform.Find("Model").GetComponent<MeshRenderer>();

            try
            {
                _background = Phone.transform.Find("Canvas/MonkeGramApp/Background").GetComponent<RawImage>();

                RenderTexture baseRT = (RenderTexture)_background.material.mainTexture;
                _renderTexture = new RenderTexture(baseRT);
                _renderTexture.filterMode = FilterMode.Point;

                _camera = Phone.transform.Find("Canvas/MonkeGramApp/cam").GetComponent<Camera>();
                _camera.targetTexture = _renderTexture;
                _camera.cullingMask = 1224081207;

                _lockScreenText = Phone.transform.Find("Canvas/Lock Screen/time").GetComponent<Text>();
                _lockScreenDateText = Phone.transform.Find("Canvas/Lock Screen/day").GetComponent<Text>();
                _topBarText = Phone.transform.Find("Canvas/Top Bar/Time Text").GetComponent<Text>();

                _background.material = new(_background.material)
                {
                    mainTexture = _renderTexture
                };

                if (properties.TryGetValue("Version", out object value))
                {
                    if (value.ToString() == "1.0.6")
                    {
                        // devs version so keep it on the camera app
                        _dummyPhoneManager.OpenDummyApp("MonkeGramApp");
                    }
                    else if (value.ToString() == Constants.Version)
                    {
                        // our version so enable custom networking
                        _dummyPhoneManager.CloseDummyApps();
                        ScreenNetworking(properties);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"Error when attempting to prepare unique camera texture for {Rig.Creator.NickName}'s NetPhone: {ex}");
            }
        }

        public void ScreenNetworking(Dictionary<string, object> properties)
        {
            if (!properties.TryGetValue("DummyApp", out object appId)) return;
            _dummyPhoneManager.OpenDummyApp(appId as string);
        }

        public void ConfigurePhone()
        {
            bool phoneConfigured = Phone.transform.parent is not null;

            bool isHeld = GrabData > 0 && GrabData < 3;
            bool inLeftHand = (GrabData % 2) == 1;
            bool levitate = GrabData == 3;

            try
            {
                State = isHeld ? ObjectGrabbyState.InHand : (levitate ? ObjectGrabbyState.Ignore : ObjectGrabbyState.Mounted);
                _isLeftHand = inLeftHand;
                InterpolationTime = 0f;

				var ik = Rig.myIk ?? Rig.GetComponent<GorillaIK>();
				Phone.transform.SetParent(isHeld ? (inLeftHand ? ik.leftHand : ik.rightHand) : (levitate ? null : (ik.bodyBone.Find("body") ?? ik.bodyBone.GetChild(0))));

				GrabPosition = Phone.transform.localPosition;
				GrabQuaternion = Phone.transform.localRotation;

				if (!phoneConfigured)
                {
                    Phone.transform.localScale = new Vector3(0.05f, 0.048f, 0.05f);
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"Error when updating network-content for phone of {Rig.Creator.NickName}: {ex}");
            }

            try
            {
                _camera.fieldOfView = Constants.FieldOfView / Zoom;
                _camera.transform.localRotation = Flipped ? Constants.CameraBackward.Rotation : Constants.CameraForward.Rotation;
                _camera.transform.localPosition = Flipped ? Constants.CameraBackward.Position : Constants.CameraForward.Position;
            }
            catch (Exception ex)
            {
                Logging.Error($"Error when updating network-content for camera of {Rig.Creator.NickName}: {ex}");
            }
        }

        public void FixedUpdate()
        {
            if (Phone is null)
                return;

            if (InRange && !_camera.gameObject.activeSelf)
            {
                _camera.gameObject.SetActive(true);
                _background.gameObject.SetActive(true);
            }
            else if (!InRange && _camera.gameObject.activeSelf)
            {
                _camera.gameObject.SetActive(false);
                _background.gameObject.SetActive(false);
            }

            if (_meshRenderer != null)
            {
                if (_meshRenderer.material.color != Rig.playerColor)
                {
                    _meshRenderer.material.color = Rig.playerColor;
                }
            }

            // set there local time networked. (HOPEFULLY THIS WORKS)
            DateTime _theirLocalTime = DateTime.UtcNow.AddMinutes(_ownerTimeOffset);
            _lockScreenText.text = _theirLocalTime.ToString("hh:mm tt");
            _lockScreenDateText.text = _theirLocalTime.ToString("dddd, dd MMMM");
            _topBarText.text = _theirLocalTime.ToString("hh:mm tt");

            _camera.nearClipPlane = Constants.NearClipPlane * Rig.scaleFactor;
            _camera.farClipPlane = Camera.main.farClipPlane;

            HandlePhoneState();
        }

        public void HandlePhoneState()
        {
            switch (State)
            {
                case ObjectGrabbyState.Mounted:
                    Phone.transform.localPosition = Vector3.Lerp(GrabPosition, Constants.Waist.Position, InterpolationTime);
                    Phone.transform.localRotation = Quaternion.Lerp(GrabQuaternion, Constants.Waist.Rotation, InterpolationTime);
                    InterpolationTime += Time.deltaTime * 5f;
                    break;

                case ObjectGrabbyState.InHand:
                    Phone.transform.localPosition = Vector3.Lerp(GrabPosition, _isLeftHand ? Constants.LeftHandBasic.Position : Constants.RightHandBasic.Position, InterpolationTime);
                    Phone.transform.localRotation = Quaternion.Lerp(GrabQuaternion, _isLeftHand ? Constants.LeftHandBasic.Rotation : Constants.RightHandBasic.Rotation, InterpolationTime);
                    InterpolationTime += Time.deltaTime * 5f;
                    InterpolationTime += Time.deltaTime * 5f;
                    break;
            }
        }
    }
}
