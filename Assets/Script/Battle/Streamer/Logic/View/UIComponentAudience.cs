using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StreamerReborn
{
    public class UIComponentAudience : UIComponentBase
    {

        public int BindInfo;

        #region ������

        [AutoBind("./")]
        public RectTransform Root;


        #endregion
    }

}
