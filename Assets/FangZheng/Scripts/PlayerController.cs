using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;


public class PlayerController : MonoBehaviour, IDamageable
{

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _normalspeed = 4 ;
    [SerializeField] private float _runspeed = 10;
    [SerializeField] private float _turnspeed = 360;
    [SerializeField] private Transform _body;
    [SerializeField] private Camera _camera;
    [SerializeField] private float parryThreshold = 0.5f;
    [SerializeField] private bool _IsParry;
    [SerializeField] private bool _IsBlock;
    [SerializeField] private bool _IsInv;


    [SerializeField] private Renderer _renderer;
    [SerializeField] private GameObject _parryzone;
    [SerializeField] private GameObject _hitBox;
    [SerializeField] private GameObject _Area;
    [SerializeField] private LayerMask EnemyLayer;
    [SerializeField] private float _ParryCooldown = 0;

    [SerializeField] private bool Lockon = false;
    [SerializeField] private bool Auto = true;
    [SerializeField] private Transform TargetEnemy = null;
    [SerializeField] private List<GameObject> EnemyNearby;
    [SerializeField] private float AngleThreshold = 35f;
    [SerializeField] private Dictionary<GameObject, float> EnemyNear = new Dictionary<GameObject, float>();
    [SerializeField] private Dictionary<GameObject, float> EnemyInView = new Dictionary<GameObject, float>();
    [SerializeField] private int DepthOfRange;
    [SerializeField] private GameObject ForEasyLocation;

    [SerializeField] private float dot;
    private float BlockHoldTime;

    private Vector3 _MousePos;
    private Vector3 _Input;
    private float _speed = 4;
    [SerializeField] private List<Weapon> weapons;
    private int currentIndex = 0;

    // Update is called once per frame
    void Update()
    {
        GatherInput();
        look();
        MousePosition();
        Dash();
        Blocking();
        changeCollor();
        
        Lockingon();
        if (Lockon)
        {
            handleEnemyInView();
            SwitchTarget();
            GetEnemiesZone();
            if (Auto == true) {
                HandleAutoTargetTracking();
            }

            if (EnemyInView.Count == 0)
            {
                ClearTargets();
            }
        }
        LocateTarget();
        Attack();
        Check();
        CheckDictionary(EnemyNear);
        CheckDictionary(EnemyInView);

        _ParryCooldown -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && weapons.Count > 0)
            weapons[currentIndex].Attack();

        // Switch weapons with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWeapon(1);
    }

    void CheckDictionary(Dictionary<GameObject, float> objectDictionary)
    {
        if (objectDictionary.Values.Any(value => value == null))
        {
            objectDictionary.Clear();
            Debug.Log("Dictionary cleared due to null reference");
        }
    }

    private void Check()
    {
        if (TargetEnemy == null)
        {
           
            ClearTargets();
        }
    }
    private void Lockingon()
    {
        if (Input.GetKeyDown(KeyCode.P)) {
            if (Lockon == false)
            {
                Lockon = true;
            }
            else
            {
                Lockon = false;
                ClearTargets();
            }
        }
    }

    private void ConsolelogginList()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            int i = 1;
            foreach (KeyValuePair<GameObject, float> enemy in EnemyNear)
            {
                Debug.Log(i + "." + enemy.Value);
                i++;
            }
        }
    }

    private void GetEnemiesZone()
    {
        EnemyNear.Clear();
        BoxCollider box = _Area.GetComponent<BoxCollider>();
        if (box == null)
        {
            return;
        }

        Vector3 center = box.transform.TransformPoint(box.center);
        Vector3 halfExtents = Vector3.Scale(box.size, box.transform.lossyScale) / 2f;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, box.transform.rotation, EnemyLayer);
        foreach (Collider hit in hits)
        {
            if (hit.GetComponent<Enemy>() != null) {
                //EnemyNearby.Add(hit.gameObject);
                EnemyNear.Add(hit.gameObject, Vector3.Distance(hit.gameObject.transform.position, this.transform.position));
            }
        }
    }

    private void HandleAutoTargetTracking()
    {
        TargetEnemy = null;
        float Diatance = Mathf.Infinity;
        foreach (GameObject enemy in EnemyNear.Keys)
        {
            Vector3 dir = (  enemy.transform.position - this.transform.position  ).normalized;
            Vector3 forward = _body.transform.forward;
            float dotProduct = Vector3.Dot(forward, dir);
            Debug.Log(dotProduct);
            float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
            dot = angle;
            if (angle < AngleThreshold)
            {
                if (Diatance > Vector3.Distance(enemy.transform.position , this.transform.position))
                {
                    Diatance = Vector3.Distance(enemy.transform.position, this.transform.position);
                    TargetEnemy = enemy.transform;
                }
            }
        }
    }

    public void handleEnemyInView()
    {
        EnemyInView.Clear();
        float Diatance = Mathf.Infinity;
        foreach (KeyValuePair<GameObject, float> enemy in EnemyNear)
        {
            if (enemy.Key != null) {
                Vector3 dir = (enemy.Key.transform.position - this.transform.position).normalized;
                Vector3 forward = _body.transform.forward;
                float dotProduct = Vector3.Dot(forward, dir);
                Debug.Log(dotProduct);
                float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
                dot = angle;
                if (angle < AngleThreshold)
                {
                    EnemyInView.Add(enemy.Key, enemy.Value);
                }
            }
        }
    }
    public void SwitchTarget()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Auto = false;
            DepthOfRange += 1;
            SwitchingTarget();
        }
    }

    public void SwitchingTarget()
    {
        if (DepthOfRange >= EnemyInView.Count )
        {
            DepthOfRange = 0;
        }
        var sortedEnemy = EnemyInView.OrderBy(pair => pair.Value);
        KeyValuePair<GameObject, float> enemy = sortedEnemy.ElementAt(DepthOfRange);
        TargetEnemy = enemy.Key.transform;
    }

    public void ClearTargets()
    {
        DepthOfRange = 0;
        Auto = true;

    }

    public void LocateTarget()
    {
        if (TargetEnemy != null) {
            ForEasyLocation.transform.position = TargetEnemy.position;
        }
    }
    public void Attack()
    {
        if (Input.GetMouseButtonDown(1))
        {
            
            if (Lockon && TargetEnemy != null)
            {

                StartCoroutine(FFCombat(TargetEnemy.position + (TargetEnemy.forward * 1f)));
            }

            //StartCoroutine(ActiveHitbox());
            
        }
    }

    private IEnumerator FFCombat(Vector3 targetPos)
    {
        float elapsed = 0;
        Vector3 startPos = transform.position;
        //Vector3 targetPos = TargetEnemy.position - (TargetEnemy.forward * 1.5f);

        while (elapsed < 0.2f)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / 0.15f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        //transform.position = targetPos;
        StartCoroutine(slowmo());
        //yield return new WaitForSeconds(0.1f);
    }

    private void changeCollor()
    {
        if (_IsInv == true)
        {
            _renderer.material.color = Color.gray;
        }
        else if (_IsParry)
        {
            _renderer.material.color = Color.blue;
        }
        else if (_IsBlock)
        {
            _renderer.material.color = Color.black;
        }
        else
        {
            _renderer.material.color = Color.yellow;
        }
    }
    public bool GetParry()
    {
        return _IsParry;
    }

    public bool GetBlock()
    {
        return _IsBlock;
    }
    void SelectWeapon(int idx)
    {
        if (idx >= 0 && idx < weapons.Count)
            currentIndex = idx;
    }

    private void FixedUpdate()
    {
        Move();
        
    }
    void GatherInput()
    {
        _Input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    void look()
    {
        if (_Input != Vector3.zero || _MousePos != Vector3.zero)
        {
            Vector3 flatMousePos = new Vector3(_MousePos.x, _body.position.y, _MousePos.z);

            Vector3 direction = (flatMousePos - _body.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                _body.rotation = Quaternion.RotateTowards(_body.rotation, targetRotation, _turnspeed * Time.deltaTime);
            }
        }
    }


    void MousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = _camera.ScreenPointToRay(mousePos);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits) {
            if (hit.transform.gameObject.tag == "Ground")
            {
                _MousePos = hit.point;
                //ball.transform.position = _MousePos;
                return;
            }
        }
        
        //Debug.Log("Mouse pos : " + _MousePos);
         
/*         {

            Camera.main.ViewportPointToRay(mousePos);
            Vector3 Position = new Vector3(_camera.ScreenToWorldPoint(mousePos).x, ball.transform.position.y, _camera.ScreenToWorldPoint(mousePos).z);
                
             Debug.Log(_camera.ScreenToWorldPoint(mousePos));
                ball.transform.position = Position;
             //DrawRay(_camera.transform.position, Position, 100);
              
         }*/
        
    }

/*    public void Attack()
    {
        if (Input.GetMouseButtonDown(0)) {
            Debug.Log("attack");
            
            StartCoroutine(ActiveHitbox());

            BoxCollider box = _Area.GetComponent<BoxCollider>();
            if (box == null)
            {
                return;
            }

            Vector3 center = box.transform.TransformPoint(box.center);
            Vector3 halfExtents = Vector3.Scale(box.size, box.transform.lossyScale) / 2f;

            Collider[] hits = Physics.OverlapBox(center, halfExtents, box.transform.rotation, EnemyLayer);
            float Diatance = Mathf.Infinity;
            GameObject target = null;
            foreach (Collider hit in hits)
            {
                if (Diatance > Vector3.Distance(hit.gameObject.transform.position, this.transform.position))
                {
                    target = hit.gameObject;
                }
            }

            if (target != null)
            {
                //transform.position += (target.transform.position - transform.position).normalized * 5 * Time.deltaTime;
                _rb.AddForce((target.transform.position - transform.position).normalized * 50, ForceMode.Impulse);
                //transform.position = Vector3.Lerp( transform.position, target.transform.position + target.transform.forward , 1);
            }

        }
    }
*/
    public IEnumerator ActiveHitbox()
    {
        _hitBox.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        _hitBox.gameObject.SetActive(false);
    }
    public void Interact()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Interact");
        }
    }

    public Vector3 GetDirection()
    {
        return ( _MousePos - transform.position ).normalized;
    }


    public void DrawRay(Vector3 origin, Vector3 direction, float length)
    {
        Debug.DrawRay(origin, direction.normalized * length, Color.red);
    }
    private void Move()
    {
        //_rb.MovePosition(transform.position + (transform.forward * _Input.magnitude )* _speed * Time.deltaTime);

        //_rb.MovePosition(_body.position + _Input.ToIso() * _Input.normalized.magnitude * _speed * Time.deltaTime);

        _rb.velocity = new Vector3(_rb.velocity.x, _rb.velocity.y, _rb.velocity.z);

        Vector3 force = _Input.ToIso().normalized * _speed;

        _rb.AddForce(force,ForceMode.Impulse);

        if (_rb.velocity.magnitude > _speed)
        {
            _rb.velocity = _rb.velocity.normalized * _speed;
        }

    }

    public void Dash()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(Dashing());
        }
    }

    public void Blocking()
    {
        stopBlock();
        if (Input.GetKeyDown(KeyCode.F))
        {
            BlockHoldTime = Time.time;
            Debug.Log("F");
        }

        if (Input.GetKey(KeyCode.F))
        {
            if (!_IsBlock && Time.time - BlockHoldTime > parryThreshold)
            {
                Block();
            }
 
        }

        if (Input.GetKeyUp(KeyCode.F) && _ParryCooldown <= 0)
        {
            float heldTime = Time.time - BlockHoldTime;
            if (heldTime <= parryThreshold)
            {
                Parry();
            }
            else
            {
                stopBlock();
            }
        }
        
    }

    public void Block()
    {
        _IsBlock = true;
        Debug.Log("Block");
    }

    public void stopBlock()
    {
        _IsBlock= false;
        Debug.Log("UnBlock()");
    }

    public void Parry()
    {
        if (_IsParry) return;
        _IsParry = true;
        _parryzone.active = true;
        Debug.Log("Parry");
        StartCoroutine(ParryWindow());
    }

    public void resetParryCooldown()
    {
        _ParryCooldown = 0;
    }
    IEnumerator ParryWindow()
    {
        _ParryCooldown = 4.0f;
        yield return new WaitForSeconds(0.5f);
        _IsParry = false;
        _parryzone.active = false;
    }

    IEnumerator Dashing()
    {
        _speed = 30;
        yield return new WaitForSeconds(0.1f);
        _speed = _normalspeed;
    }

    public void OnDrawGizmos()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = _camera.ScreenPointToRay(mousePos);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(ray.origin, ray.direction * 100f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(ray.origin, 10000f);
    }


    public void ToggleSlowmo()
    {

    }

    public IEnumerator slowmo()
    {
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = 0.02f * 0.2f;
        yield return new WaitForSeconds(0.01f);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f * 1f;
    }
    public void TakeDamage(int damage)
    {
        Debug.Log("ouch");
    }

    public void Die()
    {
        Debug.Log("die");
    }
} 
public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}