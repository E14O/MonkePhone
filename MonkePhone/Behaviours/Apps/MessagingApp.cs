using System;
using System.Collections.Generic;
using MonkePhone.Extensions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

namespace MonkePhone.Behaviours.Apps
{
    public class MessagingApp : PhoneApp
    {
        public override string AppId => "Messaging";

        public GameObject _chatBox, _templateLine, _mainGrid;

        public override void Initialize()
        {
            _mainGrid = transform.Find("Chat Messages").gameObject;
            _templateLine = _mainGrid.transform.Find("Message (1)").gameObject;
            _chatBox = transform.Find("Chat Box").gameObject;

            foreach (Transform _child in _templateLine.transform.parent)
            {
                if (_child.name != _templateLine.name)
                {
                    Destroy(_child.gameObject);
                }
            }

            _templateLine.SetActive(false);
        }

        public override void AppOpened()
        {
            base.AppOpened();

            RefreshApp();
        }

        public override void AppClosed()
        {

        }

        void RefreshApp()
        {

        }
    }
}
