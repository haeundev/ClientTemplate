using System;
using UnityEngine;
using UnityEngine.Events;

namespace LiveLarson.DataTableManagement.DataSheet.Editor
{
    [Serializable]
    public class OnDownloadEventParam
    {
        public string sheetName;
        public string rawJson;
        public string objectJson;
    }

    public class OnDownloadEvent : ScriptableObject
    {
        public UnityEvent<OnDownloadEventParam> downloadEvent;
    }
}