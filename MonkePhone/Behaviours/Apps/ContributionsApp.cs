using MonkePhone.Behaviours.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MonkePhone.Behaviours.Apps
{
    public class ContributionsApp : PhoneApp
    {
        public override string AppId => "Contributions";

        private int _currentPage;

        private GameObject _page1Obj, _page2Obj;

        public override void Initialize()
        {
            _page1Obj = transform.Find("Page (1)").gameObject;
            _page2Obj = transform.Find("Page (2)").gameObject;

            RefreshApp();
        }

        private void RefreshApp()
        {
            if (_page1Obj != null && _page2Obj != null)
            {
                _page1Obj.SetActive(_currentPage == 0);
                _page2Obj.SetActive(_currentPage == 1);
            }
        }

        public override void ButtonClick(PhoneUIObject phoneUIObject, bool isLeftHand)
        {
            switch (phoneUIObject.name)
            {
                case "Nav Left":
                    _currentPage = 0;
                    RefreshApp();
                    break;
                case "Nav Right":
                    _currentPage = 1;
                    RefreshApp();
                    break;
            }
        }
    }
}