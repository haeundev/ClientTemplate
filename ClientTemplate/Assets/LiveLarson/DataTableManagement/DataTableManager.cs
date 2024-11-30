// using DataTables;
using UnityEngine;

namespace LiveLarson.DataTableManagement
{
    public class DataTableManager : MonoBehaviour
    {
        // [SerializeField] private GameConst gameConst;
        
        public static DataTableManager Instance { get; set; }

        private void Awake()
        {
            Instance = this;
            
            Debug.Log("[DataTableManager]  Awake!");
        }

        private void OnDestroy()
        {
            Debug.Log("[DataTableManager]  OnDestroy!");

            Instance = default;
        }
        
        // public static GameConst GameConst => Instance.gameConst;
    }
}