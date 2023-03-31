using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using My.Framework.Runtime.UIExtention;
using StreamerReborn.Config;
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

        #region °ó¶¨ÇøÓò

        [AutoBind("./")]
        public RectTransform Root;


        #endregion
    }

}
