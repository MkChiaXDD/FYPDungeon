using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class DirectionTarget : MonoBehaviour
{
    [SerializeField] private GameObject Target_position;
    [SerializeField] private RectTransform PointerTransform;
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject TargetUI;
    [SerializeField] private RectTransform Minimap;
    [SerializeField] private Camera Camera;
    [SerializeField] private float Range;

    [SerializeField] private GameObject PointerContainer;
    [SerializeField] private RectTransform PointerPrefab;
    [SerializeField] private List<GameObject> targets = new List<GameObject>();
    private Dictionary<GameObject, RectTransform> TargetDirection = new Dictionary<GameObject, RectTransform>();

    private void UpdatePointerPos(GameObject target, RectTransform pointer)
    {
        Vector2 Pos_OnCam = Camera.WorldToScreenPoint(target.transform.position);
        Vector2 Player_Pos_OnCam = Camera.WorldToScreenPoint(Player.transform.position);
        Vector2 direction = (Player_Pos_OnCam - Pos_OnCam).normalized;


        if (Pos_OnCam.magnitude > Range)
        {
            Pos_OnCam = Player_Pos_OnCam + direction * -Range;
        }

        pointer.position = Pos_OnCam;

        float Distance = Vector3.Distance(new Vector3(Player.transform.position.x, 0, Player.transform.position.z), new Vector3(target.transform.position.x, 0, target.transform.position.z));
        if (Distance <= 15)
        {
            pointer.position = Camera.WorldToScreenPoint(target.transform.position);
            Vector3 origin = pointer.position;
            pointer.position = new Vector3(pointer.position.x, pointer.position.y + 250, pointer.position.z);
            Vector3 Direction = Camera.transform.InverseTransformDirection(Camera.WorldToScreenPoint(target.transform.position) - pointer.position);
            float angle2 = Mathf.Atan2(Direction.z, Direction.x) * Mathf.Rad2Deg;
            pointer.localEulerAngles = new Vector3(0f, 0f, angle2 + 90);
            return;
        }


        Vector3 Dir = Camera.transform.InverseTransformDirection(target.transform.position - Player.transform.position);
        Dir.y = 0;
        float angle = Mathf.Atan2(Dir.z, Dir.x) * Mathf.Rad2Deg;
        pointer.localEulerAngles = new Vector3(0f, 0f, angle - 90.0f);


    }

    private void UpdateTarget()
    {

        ClearNullTargets();

        AutoTarget();

        //PointerContainer.SetActive(true);
        if (targets == null || targets.Count <= 0 || Player == null)
        {
            //PointerContainer.SetActive(false);
            //Debug.Log("E");
            return;
        }

        foreach (GameObject Target in targets)
        {
            if (Target == null) continue;

            if (!TargetDirection.ContainsKey(Target))
            {

                RectTransform Pointer = Instantiate(PointerPrefab , PointerContainer.transform);

                //if (Target.GetComponent<BossPortal>() != null)
                //{
                //    Pointer.GetComponent<Image>().tintColor = Color.yellow;
                //}
                //else
                //{
                //    Pointer.GetComponent<Image>().tintColor = Color.blue;
                //}
                Debug.Log("Pointer Created");
                TargetDirection.Add(Target, Pointer);

            }


            if (TargetDirection.TryGetValue(Target, out RectTransform pointer))
            {
                pointer.gameObject.SetActive(true);
                UpdatePointerPos(Target, pointer);
            }

        }
    }

    public void AddTargets(GameObject Obj)
    {
        if (!TargetDirection.ContainsKey(Obj)) {
            targets.Add(Obj);
        }

        //Debug.Log("Obj Target: " + Obj.name);
    }

    public void RemoveTargets(GameObject Obj)
    {
        targets.Remove(Obj);
        

        foreach (var kvp in TargetDirection)
        {
            if (kvp.Key == Obj)
            {
                Destroy(kvp.Value.gameObject);
                
            }
        }

        TargetDirection.Remove(Obj);
    }

    private void AutoTarget()
    {


        if (FindFirstObjectByType<BossPortal>() != null)
        {
            Target_position = FindFirstObjectByType<BossPortal>().gameObject;
            if (!targets.Contains(Target_position)) {
                targets.Add(Target_position);
            }
        }
        else
        {
            Target_position = null;
            if (targets.Contains(Target_position))
            {
                targets.Remove(Target_position);
            }
        }


    }
    private void ClearNullTargets()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (var kvp in TargetDirection)
        {
            if (kvp.Key == null)
            {
                Destroy(kvp.Value.gameObject);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            TargetDirection.Remove(key);
            targets.Remove(key);
        }

        
    }

    private void ClearDestroyedItems()
    {
        foreach (GameObject target in targets)
        {
            if (TargetDirection.ContainsKey(target))
            {

            }
        }
    }

    private void DebugDictionary()
    {
        if (TargetDirection == null || TargetDirection.Count == 0)
        {
            Debug.Log("TargetDirection is empty or null.");
            return;
        }

        Debug.Log("--- Current TargetDirection Contents ---");
        foreach (var kvp in TargetDirection)
        {
            GameObject target = kvp.Key;
            RectTransform pointer = kvp.Value;

            string targetName = (target != null) ? target.name : "NULL (Destroyed)";
            string pointerStatus = (pointer != null) ? pointer.gameObject.activeSelf ? "Active" : "Inactive" : "NULL (Destroyed)";

            Debug.Log($"Target: {targetName} | Pointer: {pointerStatus}");
        }
        Debug.Log("----------------------------------------");
    }

    void Update()
    {
        UpdateTarget();

        if (Input.GetKeyUp(KeyCode.T) )
        {
            DebugDictionary();
        }
        //if (FindFirstObjectByType<BossPortal>() != null)
        //{
        //    Target_position = FindFirstObjectByType<BossPortal>().gameObject;
        //}
        //else
        //{
        //    Target_position = null;
        //}
        ////Target_position = FindFirstObjectByType<Portal>().gameObject;
        //if (Target_position == null || Player == null)
        //{
        //    TargetUI.SetActive(false);
        //    return;
        //}
        //TargetUI.SetActive(true);

        #region Supported
        //Vector3 Screen_Target_Pos = Camera.WorldToScreenPoint(Target_position.transform.position);

        //Screen_Target_Pos.x = Mathf.Clamp(Screen_Target_Pos.x, Range, Screen.width - Range);
        //Screen_Target_Pos.y = Mathf.Clamp(Screen_Target_Pos.y, Range, Screen.height - Range);


        //PointerTransform.position = Screen_Target_Pos;


        //Vector3 dir = Camera.transform.InverseTransformDirection(Target_position.transform.position - Player.transform.position);
        //dir.y = 0;
        //float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        //PointerTransform.rotation = Quaternion.Euler(0, 0, angle + 270);
        #endregion

        #region Try my self

        //Vector2 Pos_OnCam = Camera.WorldToScreenPoint(Target_position.transform.position);
        //Vector2 Player_Pos_OnCam = Camera.WorldToScreenPoint(Player.transform.position);
        //Vector2 direction = (Player_Pos_OnCam - Pos_OnCam).normalized;

        ////Pos_OnCam.x = Mathf.Clamp(Pos_OnCam.x, Range, Screen.width - Range);
        ////Pos_OnCam.y = Mathf.Clamp(Pos_OnCam.y, Range, Screen.height - Range);



        //if (Pos_OnCam.magnitude > Range)
        //{
        //    Pos_OnCam = Player_Pos_OnCam + direction * -Range;
        //}

        //PointerTransform.position = Pos_OnCam;


        //Vector3 Dir = Camera.transform.InverseTransformDirection(Target_position.transform.position - Player.transform.position);
        //Dir.y = 0;
        //float angle = Mathf.Atan2(Dir.z, Dir.x) * Mathf.Rad2Deg;
        //PointerTransform.localEulerAngles = new Vector3(0f , 0f , angle - 90.0f);

        //float Distance = Vector3.Distance(new Vector3(Player.transform.position.x , 0 ,Player.transform.position.z) , new Vector3(Target_position.transform.position.x, 0, Target_position.transform.position.z));
        //if (Distance <= 15)
        //{
        //    PointerTransform.position = Camera.WorldToScreenPoint(Target_position.transform.position);
        //}
        ////PointerTransform.
        #endregion

    }
}
