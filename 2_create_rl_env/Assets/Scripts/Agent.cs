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

public class RlResult {
    public float reward;
    public bool finished;
    public MyVector3 obs;
    public RlResult(float reward, bool finished, MyVector3 obs) {
        this.reward = reward;
        this.finished = finished;
        this.obs = obs;
    }
}

public class Agent : MonoBehaviour
{
    class Rpc : JsonRpcService {
        Agent agent;

        public Rpc(Agent agent) {
            this.agent = agent;
        }

        [JsonRpcMethod]
        void Say(string message) {
            Debug.Log($"you sent {message}");
        }

        [JsonRpcMethod]
        float GetHeight() {
            return agent.transform.position.y;
        }

        [JsonRpcMethod]
        MyVector3 GetPos() {
            return new MyVector3(agent.transform.position);
        }

        [JsonRpcMethod]
        void Translate(MyVector3 translate) {
            agent.transform.position += translate.AsVector3();
        }

        [JsonRpcMethod]
        RlResult Step(string action) {
            return agent.Step(action);
        }

        [JsonRpcMethod]
        MyVector3 Reset() {
            return agent.Reset();
        }
    }

    public GameObject Target;
    Rpc rpc;
    Simulation simulation;
    float reward;
    bool finished;

    void Start()
    {
        simulation = GetComponent<Simulation>();
        rpc = new Rpc(this);
    }

    public RlResult Step(string action) {
        reward = 0;

        Vector3 direction = Vector3.zero;
        switch(action) {
            case "north":
                direction = Vector3.forward;
                break;
            case "south":
                direction =  - Vector3.forward;
                break;
            case "east":
                direction =  Vector3.right;
                break;
            case "west":
                direction =  - Vector3.right;
                break;
        }
        Vector3 newPos = transform.position + direction * 10 * simulation.SimulationStepSize;
        newPos.x = Mathf.Clamp(newPos.x, -3.0f, 3.0f);
        newPos.y = 0;
        newPos.z = Mathf.Clamp(newPos.z, -3.0f, 3.0f);
        transform.position = newPos;

        simulation.Simulate();

        return new RlResult(reward, finished, GetObservation());
    }

    public MyVector3 Reset() {
        transform.position = Vector3.zero;

        Vector3 newPos = transform.position;
        while((newPos - transform.position).magnitude < 1.5f) {
            newPos = new Vector3(Random.Range(-3.0f, 3.0f), 0, Random.Range(-3.0f, 3.0f));
        }

        Target.gameObject.transform.position = newPos;

        finished = false;

        return GetObservation();
    }

    public MyVector3 GetObservation() {
        return new MyVector3(Target.transform.position - transform.position);
    }

    void OnTriggerEnter(Collider coll) {
        Debug.Log("win");
        reward += 1;
        finished = true;
    }
}
