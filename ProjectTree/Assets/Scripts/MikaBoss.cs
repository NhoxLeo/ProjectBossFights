﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MikaBoss : MonoBehaviour {
    Collider myHitbox;
    [SerializeField] Transform barrierPointsParent;
    public enum State {
        Idle,
        Attacking
    }
    public State curState = State.Idle;
    bool isAttacking = false;
    PlayerController player;
    PlayerCam cam;
    [SerializeField] Vector3 centerPos;
    [Header ("Memory inferno")]
    [SerializeField] GameObject[] memoryInfernoHitboxes = new GameObject[3];
    [SerializeField] GameObject[] memoryInfernoIndicators = new GameObject[3];
    [Header ("TeleSlash")]
    [SerializeField] GameObject teleslashHitbox;
    [SerializeField] GameObject jumpslashHitbox;
    [SerializeField] GameObject blackholeSlashHitbox;
    [Header("Reality Slash")]
    [SerializeField] GameObject realitySlashHitbox;
    [Header("Black Hole")]
    [SerializeField] GameObject blackholePrefab;

    void Start () {
        myHitbox = GetComponent<Collider> ();
        memoryInfernoPattern = new int[10];
        for (int i = 0; i < 10; i++) {
            memoryInfernoPattern[i] = Random.Range (0, 3);
        }
        player = FindObjectOfType<PlayerController> ();
        cam = FindObjectOfType<PlayerCam> ();

    }

    void Update () {
        UpdateBarrierActive ();
        DebugInput ();
        SetCam();
    }

    float camX = 10;
    void SetCam () {
        cam.angleGoal.x = camX;
        cam.angleGoal.y = Quaternion.LookRotation (transform.position - cam.transform.position, Vector3.up).eulerAngles.y;
        cam.offset = cam.transform.forward * -10 + cam.transform.right * 2;
    }

    void DebugInput () {
        if (Input.GetKeyDown (KeyCode.Tab) == true) {
            StartAttack (State.Attacking, "BlackHole");//activate attack
        }
    }

    void UpdateBarrierActive () {
        SetHitboxActive ((barrierPointsParent.childCount <= 0));
    }

    void SetHitboxActive (bool active) {
        bool wasActive = myHitbox.enabled;

        if (active == true && wasActive == false) {
            myHitbox.enabled = active;
        }

        if (active == false && wasActive == true) {
            myHitbox.enabled = active;
        }
    }

    void StartAttack (State atk, string coroutineName) {
        if (isAttacking == false) {

            curState = atk;

            StartCoroutine (coroutineName);
            isAttacking = true;

        }
    }

    void StopAttack () {
        isAttacking = false;
        curState = State.Idle;
    }

    int[] memoryInfernoPattern;
    int memoryInfernoCOunt = 5;
    float memoryInfernoSpeedMulitplier = 2;
    IEnumerator MemoryInferno () {
        camX = 40;
        yield return new WaitForSeconds (0.5f * memoryInfernoSpeedMulitplier);
        for (int i = 0; i < memoryInfernoCOunt; i++) {
            memoryInfernoIndicators[memoryInfernoPattern[i]].SetActive (true);
            yield return new WaitForSeconds (0.2f * memoryInfernoSpeedMulitplier);
            memoryInfernoIndicators[memoryInfernoPattern[i]].SetActive (false);
            yield return new WaitForSeconds (0.05f * memoryInfernoSpeedMulitplier);
        }
        yield return new WaitForSeconds (0.5f * memoryInfernoSpeedMulitplier);
        for (int i = 0; i < memoryInfernoCOunt; i++) {
            memoryInfernoHitboxes[memoryInfernoPattern[i]].SetActive (true);
            yield return new WaitForSeconds (0.45f * memoryInfernoSpeedMulitplier);
            memoryInfernoHitboxes[memoryInfernoPattern[i]].SetActive (false);
            yield return new WaitForSeconds (0.05f * memoryInfernoSpeedMulitplier);
        }
        yield return new WaitForSeconds (0.5f * memoryInfernoSpeedMulitplier);
        StopCoroutine ("PushPlayerToBH");
        camX = 10;
        StopAttack ();
    }

    IEnumerator TeleSlash () {
        yield return new WaitForSeconds (0.5f);
        transform.position = player.transform.position + (Vector3.up * 2) - player.transform.forward * 5;
        transform.LookAt (new Vector3 (player.transform.position.x, transform.position.y, player.transform.position.z));
        yield return new WaitForSeconds (0.3f);
        Instantiate (teleslashHitbox, transform.position + transform.forward, Quaternion.identity).transform.LookAt (new Vector3 (player.transform.position.x, transform.position.y, player.transform.position.z));
        cam.MediumShake (0.2f);
        yield return new WaitForSeconds (0.8f);
        transform.position = centerPos;
        StopAttack ();
    }

    IEnumerator JumpSlash () {
        yield return new WaitForSeconds (0.2f);
        jumpSlashY = 45;
        JumpSlashMove ();
        float range = 3;
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < 3; i++) {
            yield return new WaitForSeconds (0.1f);
            GameObject g = Instantiate (jumpslashHitbox, transform.position + transform.forward, Quaternion.identity);
            g.transform.LookAt (player.transform.position + player.transform.forward * -2);
            g.transform.Rotate(0,0,Random.Range(0,360));
            cam.MediumShake (0.2f);
        }
        yield return new WaitForSeconds(0.3f);
        CancelInvoke ("JumpSlashMove");
        JumpSlashFall ();
        yield return new WaitForSeconds (1.5f);
        CancelInvoke ("JumpSlashFall");
        StopAttack ();
    }

    float jumpSlashY;
    void JumpSlashMove () {
        transform.position += Vector3.up * Time.deltaTime * jumpSlashY;
        jumpSlashY = Mathf.MoveTowards (jumpSlashY, 0, Time.deltaTime * 100);
        Invoke ("JumpSlashMove", Time.deltaTime);
    }

    void JumpSlashFall () {
        transform.position = Vector3.MoveTowards (transform.position, new Vector3 (transform.position.x, centerPos.y, transform.position.z), Time.deltaTime * 29);
        if (transform.position.y != centerPos.y) {
            Invoke ("JumpSlashFall", Time.deltaTime);
        }
    }

    IEnumerator BlackHole () {
        cc = player.GetComponent<CharacterController> ();
        yield return new WaitForSeconds (1);
        StartCoroutine ("PushPlayerToBH");
        cam.SmallShake (2);
        GameObject bHole = Instantiate(blackholePrefab,transform.position,Quaternion.identity);
        yield return new WaitForSeconds (1);
        float curRot = 0;
        for (int i = 0; i < 10; i++) {
            GameObject g = Instantiate (blackholeSlashHitbox, transform.position + transform.forward, Quaternion.Euler (0, curRot, Random.Range (-45, 45)));
            //g.AddComponent<AutoScale> ().goal = transform.localScale + new Vector3 (transform.localScale.x * 10, 0, 0);
            //g.transform.localScale = Vector3.one / 2;
            //g.AddComponent<AutoScale> ().speed *= 2;
            cam.MediumShake (0.2f);
            curRot += 360 / 10;
            yield return new WaitForSeconds (0.15f);
        }
        yield return new WaitForSeconds (0.35f);
        Destroy(bHole);
        StopCoroutine ("PushPlayerToBH");
        StopAttack ();

    }

    List<GameObject> realitySlashHitboxes = new List<GameObject>();
    IEnumerator RealitySlash(){
        camX = 50;
        realitySlashHitboxes.Clear();
        for (int i = 0; i < 5; i++)
        {
            GameObject g = Instantiate(realitySlashHitbox,transform.position,Quaternion.Euler(0,Random.Range(0,360),0));
            g.transform.position += g.transform.forward * Random.Range(3,15);
            g.transform.Rotate(0,Random.Range(0,360),90);
            realitySlashHitboxes.Add(g);
            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 5; i++)
        {
            realitySlashHitboxes[i].GetComponent<Collider>().enabled = true;
            Destroy(realitySlashHitboxes[i],0.5f);
        }
        cam.HardShake(0.1f);
        yield return new WaitForSeconds(1);
        StopAttack();
        camX = 10;
    }

    IEnumerator LastResort(){
        cc = player.GetComponent<CharacterController> ();
        for (int i = 0; i < memoryInfernoHitboxes.Length; i++)
        {
            memoryInfernoHitboxes[i].GetComponent<Hurtbox>().damage = JustDontGetHitAndItWillBeFine();
        }
        yield return new WaitForSeconds(1);
        StartCoroutine ("PushPlayerToBH");
        StartCoroutine(MemoryInferno());
    }

    float JustDontGetHitAndItWillBeFine(){
        return Mathf.Infinity;
    }

    CharacterController cc;
    IEnumerator PushPlayerToBH () {
        Vector3 dir = -(player.transform.position - transform.position).normalized;
        dir.y = 0;
        cc.Move (dir * 15 * Time.deltaTime);
        yield return new WaitForSeconds (0);
        StartCoroutine ("PushPlayerToBH");
    }
}