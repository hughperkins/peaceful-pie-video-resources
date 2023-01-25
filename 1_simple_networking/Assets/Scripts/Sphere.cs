using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AustinHarris.JsonRpc;

public class MyVector3 {
    public float x;
    public float y;
    public float z;
    public MyVector3(Vector3 v) {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
    }
    public Vector3 AsVector3() {
        return new Vector3(x, y, z);
    }
}


public class RlReward {
    public float reward;
    public bool finished;
    public MyVector3 obs;
    public RlReward(float reward, bool finished, MyVector3 obs) {
        this.reward = reward;
        this.finished = finished;
        this.obs = obs;
    }
}

public class Sphere : MonoBehaviour
{
    class Rpc : JsonRpcService {
        Sphere sphere;

        public Rpc(Sphere sphere) {
            this.sphere = sphere;
        }

        [JsonRpcMethod]
        void Say(string message) {
            Debug.Log($"you sent {message}");
        }

        [JsonRpcMethod]
        float GetHeight() {
            return sphere.transform.position.y;
        }

        [JsonRpcMethod]
        MyVector3 GetPos() {
            return new MyVector3(sphere.transform.position);
        }

        [JsonRpcMethod]
        void Translate(MyVector3 translate) {
            sphere.transform.position += translate.AsVector3();
        }

        [JsonRpcMethod]
        RlReward Step(string action) {
            return sphere.Step(action);
        }

        [JsonRpcMethod]
        MyVector3 Reset() {
            return sphere.Reset();
        }
    }

    public GameObject Target;

    Rpc rpc;
    Simulation simulation;

    float reward = 0;
    bool finished = false;

    void Start()
    {
        rpc = new Rpc(this);
        simulation = GetComponent<Simulation>();
    }

    public MyVector3 Reset() {
        transform.position = Vector3.zero;

        Vector3 newPos = transform.position;
        while((newPos - transform.position).magnitude < 1.5) {
            newPos = new Vector3(Random.Range(-3.0f, 3.0f), 0, Random.Range(-3.0f, 3.0f));
        }
        Target.transform.position = newPos;

        finished = false;

        return GetObservation();
    }

    RlReward Step(string action) {
        reward = 0;

        switch(action) {
            case "forward":
                Vector3 newPos = transform.position;
                newPos += transform.TransformDirection(Vector3.forward) * 10 * simulation.SimulationStepSize;
                newPos.x = Mathf.Clamp(newPos.x, -3, 3);
                newPos.z = Mathf.Clamp(newPos.z, -3, 3);
                transform.position = newPos;
                break;
            case "left":
                transform.Rotate(Vector3.up, - simulation.SimulationStepSize * 200);
                break;
            case "right":
                transform.Rotate(Vector3.up, + simulation.SimulationStepSize * 200);
                break;
        }

        // int angle = 45 * action;
        // Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
        // Vector3 newPos = transform.position + direction * 10 * simulation.SimulationStepSize;

        simulation.Simulate();

        return new RlReward(reward, finished, GetObservation());
    }

    MyVector3 GetObservation() {
        return new MyVector3(Target.transform.position - transform.position);
    }

    void OnTriggerEnter(Collider coll) {
        Debug.Log("collide");
        reward += 1;
        finished = true;
    }
}
