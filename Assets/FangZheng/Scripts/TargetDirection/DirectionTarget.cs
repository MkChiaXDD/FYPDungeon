using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionTarget : MonoBehaviour
{
    [SerializeField] private GameObject Target_position;
    [SerializeField] private RectTransform PointerTransform;
    [SerializeField] private GameObject Player;
    [SerializeField] private Camera Camera;
    void Start()
    {
        
    }


    void Update()
    {
        Vector3 Target = Target_position.transform.position;
        Vector3 From = Player.transform.position;


        Vector3 targetScreenPos = Camera.WorldToScreenPoint(Target_position.transform.position);
        Vector3 playerScreenPos = Camera.WorldToScreenPoint(Player.transform.position);

        //From.z = 0f;
        //Target.z = 0f;

        Vector3 direction = (playerScreenPos - targetScreenPos).normalized;

        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg );

        Debug.Log("Angle: " + (angle + 180f) );
        //angle += 360f;

        PointerTransform.localEulerAngles = new Vector3(0f, 0f, angle);
    }
}
