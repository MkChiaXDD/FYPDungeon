using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DirectionTarget : MonoBehaviour
{
    [SerializeField] private GameObject Target_position;
    [SerializeField] private RectTransform PointerTransform;
    [SerializeField] private GameObject Player;
    [SerializeField] private Camera Camera;
    [SerializeField] private float Range;
    void Update()
    {
        if (Target_position == null || Player == null)
        {
            return;
        }

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



        Pos_OnCam.x = Mathf.Clamp(Pos_OnCam.x, Range, Screen.width - Range);
        Pos_OnCam.y = Mathf.Clamp(Pos_OnCam.y, Range, Screen.height - Range);

        //if (Pos_OnCam.magnitude > Range)
        //{
        //    Pos_OnCam = Pos_OnCam.normalized * Range;
        //}

        //PointerTransform.anchoredPosition = Pos_OnCam;

        Vector3 Dir = Camera.transform.InverseTransformDirection(Target_position.transform.position - Player.transform.position);
        Dir.y = 0;
        float angle = Mathf.Atan2(Dir.z, Dir.x) * Mathf.Rad2Deg;
        PointerTransform.localEulerAngles = new Vector3(0f , 0f , angle - 90.0f);
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
