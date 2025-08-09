using System.Collections.Generic;
using UnityEngine;

public class EnemyCheck : MonoBehaviour
{
    public GameObject Container;
    public List<GameObject> Child = new List<GameObject>();
    private TutorialProggresion _Tutorial;
    // Start is called before the first frame update
    void Start()
    {
        _Tutorial = FindFirstObjectByType<TutorialProggresion>();
        if (Container == null)
        {
            Container = this.gameObject;
        }
        ChildrenList();
    }

    public void ChildrenList()
    {
        Child.Clear();

        if (Container != null)
        {
            foreach (Transform child in Container.transform)
            {
                Child.Add(child.gameObject);
            }
        }

    }
    // Update is called once per frame
    void Update()
    {
        ChildrenList();

        if (Child.Count <= 0)
        {
            if (_Tutorial != null)
            {
                _Tutorial.IfPlayerPerformAction("KillAll");
            }
        }
    }
}
