using UnityEditor;
using System;
using LiveLarson.DataTableManagement.DataSheet.Editor;

namespace DataTables
{
    [CustomEditor(typeof(GameConst))]
    public class GameConstEditor : DataScriptEditor
    {
        public override string FileID => "1F4kMCsig6TtykmkxeervPBPXQkx2xAoTvG_zBurE-7g";
        public override string SheetName => "GameConst";
        public override DataScript.DataType DataType => DataScript.DataType.Const;
        public override Type SubClassType => typeof(GameConst.DataClass);

        public override void SetAssetData(string json)
        {
            var obj = target as GameConst;
            obj.Data = DataScript.MakeObjectFromJsonString<GameConst.DataClass>(json);
        }
    }
}

