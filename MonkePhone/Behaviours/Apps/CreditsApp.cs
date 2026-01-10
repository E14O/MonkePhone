using MonkePhone.Behaviours.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MonkePhone.Behaviours.Apps
{
    public class CreditsApp : PhoneApp
    {
        public override string AppId => "Credits";

        private int _currentPage;

        private GameObject _pageOneObj;
        private GameObject _pageTwoObj;
        private GameObject _navLabelObj;

        private Text _navLabel;

        public override void Initialize()
        {
            _pageOneObj = transform.Find("PageOne").gameObject;
            _pageTwoObj = transform.Find("PageTwo").gameObject;
            _navLabelObj = transform.Find("Category Nav Label").gameObject;
            _navLabel = _navLabelObj.GetComponent<Text>();

            _currentPage = 0;

            RefreshCreditsPage();
        }

        public override void ButtonClick(PhoneUIObject phoneUIObject, bool isLeftHand)
        {
            switch (phoneUIObject.name)
            {
                case "Credits Last":
                    _currentPage = 0;
                    _navLabel.text = "1/2";
                    RefreshCreditsPage();
                    break;
                case "Credits Next":
                    _currentPage = 1;
                    _navLabel.text = "2/2";
                    RefreshCreditsPage();
                    break;
            }
        }

        private void RefreshCreditsPage()
        {
            if (_pageOneObj != null && _pageTwoObj != null)
            {
                _pageOneObj.SetActive(_currentPage == 0);
                _pageTwoObj.SetActive(_currentPage == 1);
            }
        }
    }
}