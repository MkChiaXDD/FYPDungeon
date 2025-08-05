using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DirectionTarget : MonoBehaviour
{
    [SerializeField] private GameObject Target_position;
    [SerializeField] private RectTransform PointerTransform;
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject TargetUI;
    [SerializeField] private RectTransform Minimap;
    [SerializeField] private Camera Camera;
    [SerializeField] private float Range;
    void Update()
    {
        if (FindFirstObjectByType<BossPortal>() != null)
        {
            Target_position = FindFirstObjectByType<BossPortal>().gameObject;
        }
        else
        {
            Target_position = null;
        }
        //Target_position = FindFirstObjectByType<Portal>().gameObject;
        if (Target_position == null || Player == null)
        {
            TargetUI.SetActive(false);
            return;
        }
        TargetUI.SetActive(true);

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

        Vector2 Pos_OnCam = Camera.WorldToScreenPoint(Target_position.transform.position);
        Vector2 Player_Pos_OnCam = Camera.WorldToScreenPoint(Player.transform.position);
        Vector2 direction = (Player_Pos_OnCam - Pos_OnCam).normalized;

        //Pos_OnCam.x = Mathf.Clamp(Pos_OnCam.x, Range, Screen.width - Range);
        //Pos_OnCam.y = Mathf.Clamp(Pos_OnCam.y, Range, Screen.height - Range);



        if (Pos_OnCam.magnitude > Range)
        {
            Pos_OnCam = Player_Pos_OnCam + direction * -Range;
        }

        PointerTransform.position = Pos_OnCam;


        Vector3 Dir = Camera.transform.InverseTransformDirection(Target_position.transform.position - Player.transform.position);
        Dir.y = 0;
        float angle = Mathf.Atan2(Dir.z, Dir.x) * Mathf.Rad2Deg;
        PointerTransform.localEulerAngles = new Vector3(0f , 0f , angle - 90.0f);

        float Distance = Vector3.Distance(new Vector3(Player.transform.position.x , 0 ,Player.transform.position.z) , new Vector3(Target_position.transform.position.x, 0, Target_position.transform.position.z));
        if (Distance <= 15)
        {
            PointerTransform.position = Camera.WorldToScreenPoint(Target_position.transform.position);
        }
        //PointerTransform.
        #endregion
        //Vector3 Target = Target_position.transform.position;
        //Vector3 From = Player.transform.position;


        //Vector3 targetScreenPos = Camera.WorldToScreenPoint(Target_position.transform.position);
        //Vector3 playerScreenPos = Camera.WorldToScreenPoint(Player.transform.position);

        ////From.z = 0f;
        ////Target.z = 0f;

        //Vector3 direction = (playerScreenPos - targetScreenPos).normalized;

        //float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg );

        //Debug.Log("Angle: " + (angle + 180f) );
        ////angle += 360f;

        //PointerTransform.localEulerAngles = new Vector3(0f, 0f, angle);
    }
}
