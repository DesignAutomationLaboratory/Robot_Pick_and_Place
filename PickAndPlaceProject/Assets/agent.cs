using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;


public class agent : Agent
{
    // Start is called before the first frame update
    public Rigidbody platform_rBody;
    
    public Transform platform;
    public Transform target_location;
    public Transform targetplacement_location;
    public Transform table;

    public GameObject rrobot;
    private ArticulationBody[] articulationChain;

    public Button Button1;
    [SerializeField]public List<Transform> walls;

    public int i;
    public TrailRenderer tr;

    void Start()
    {
        // tr = GetComponent<TrailRenderer>();
    }

    public override void OnEpisodeBegin()
    {
        reset_the_scene();   
        i = 0;
    }

    public float speed = 1.0f;
    //****************************************************

    public override void OnActionReceived(float[] vectorAction)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        platform_rBody.AddForce(controlSignal * speed);

        if(platform.transform.localPosition.x > 5.0f || platform.transform.localPosition.x < -5.0f || platform.transform.localPosition.z > 5.0f || platform.transform.localPosition.z < -5.0f)
        {
            fell_off_table();
        }  
        
        AddReward(-1f / MaxStep);

        Vector3 aV = target_location.transform.position;
        aV.y = 0f;
        Vector3 bV = targetplacement_location.transform.position;
        bV.y = 0f;
        Vector3 rV = rrobot.transform.position;
        rV.y = 0f;

        if(Vector3.Distance(aV,rV) < 0.35f && Vector3.Distance(bV,rV) < 0.35f)
        {                  
            if(i == 0)
            {
                AddReward(1.0f);
                platform_rBody.constraints = RigidbodyConstraints.FreezeAll;
                tr.Clear();
                Button1.onClick.Invoke();
                i = i + 1;
                // EndEpisode();
            }            
        }
    }
    //**********************************************************************

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Vector3.Distance(platform.transform.position, target_location.transform.position));
        sensor.AddObservation(Vector3.Distance(platform.transform.position, targetplacement_location.transform.position));

        sensor.AddObservation(platform_rBody.velocity.x);
        sensor.AddObservation(platform_rBody.velocity.z);
    }
    //**********************************************************************

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }    
    //**********************************************************************

    private void OnCollisionEnter(Collision collision)
    {
        // if (collision.gameObject.CompareTag("Table") == true)
        // {
        //     AddReward(0.5f);
        //     platform_rBody.constraints = RigidbodyConstraints.FreezeAll;
        //     EndEpisode();
        //     // Button1.onClick.Invoke(); //Uncomment to showcase
        // }   

        if (collision.gameObject.CompareTag("Wall") == true)
        {
            AddReward(-0.1f);
        } 
    }
    //**********************************************************************

    void fell_off_table()
    {
        AddReward(-1.0f);
        EndEpisode();
    }

    void reset_the_scene()
    {
        target_location.transform.SetParent(table); // Keep the pickup object under the table
        targetplacement_location.transform.SetParent(table); // Keep the dropp off location under the table

        articulation_turnoff();
        
        platform_rBody.constraints = ~RigidbodyConstraints.FreezePositionX & ~RigidbodyConstraints.FreezePositionZ; // Change the constrains of the platform
        platform_rBody.velocity = Vector3.zero; 

        platform.localPosition = new Vector3(Random.Range(-4.0f, 4.0f), -0.317f, -4.5f); // Move the platform and robot to a new spot
        table.localPosition = new Vector3(Random.Range(-4.0f, 4.0f), -0.6308754f, 4.0f); // Move the table, target and location to a new spot

        randomize_wall_position();
    }

    void randomize_wall_position()
    {
        walls[0].localPosition = new Vector3(Random.Range(-3.5f, 3.5f), -0.1f, -3.5f); // Move the walls randomly along one axis
        walls[1].localPosition = new Vector3(Random.Range(-3.5f, 3.5f), -0.1f, -1.5f); 
        walls[2].localPosition = new Vector3(Random.Range(-3.5f, 3.5f), -0.1f, 0.5f); 
        walls[3].localPosition = new Vector3(Random.Range(-3.5f, 3.5f), -0.1f, 2.1f);
    }

    void articulation_turnoff()
    {
        articulationChain = rrobot.GetComponentsInChildren<ArticulationBody>(); // Turning off of all 'articulation body' to enable movement of the the robot
        foreach (ArticulationBody joint in articulationChain)
        {
            joint.enabled = false;
        }
    }
}
