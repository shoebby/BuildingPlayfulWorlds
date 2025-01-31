﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Status { idle, moving, crouching, sliding, climbingLadder, wallRunning, grabbedLedge, climbingLedge, vaulting, hookshotThrown, hookshotPulled}

public class PlayerController : MonoBehaviour
{
    public Status status;
    [SerializeField]
    private LayerMask vaultLayer;
    [SerializeField]
    private LayerMask ledgeLayer;
    [SerializeField]
    private LayerMask ladderLayer;
    [SerializeField]
    private LayerMask wallrunLayer;
    [SerializeField]
    private LayerMask HookshotLayer;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform hookshotTransform;

    public CharacterController CharacterController;

    GameObject vaultHelper;

    Vector3 wallNormal = Vector3.zero;
    Vector3 ladderNormal = Vector3.zero;
    Vector3 pushFrom;
    Vector3 slideDir;
    Vector3 vaultOver;
    Vector3 vaultDir;
    Vector3 hookshotPos;

    ParticleSystem speedboostParticleSystem;
    ParticleSystem jumpboostParticleSystem;
    PlayerMovement movement;
    PlayerInput playerInput;
    AnimateLean animateLean;

    bool canInteract;
    bool canGrabLedge;
    bool controlledSlide;

    float rayDistance;
    float slideLimit;
    float slideTime;
    float radius;
    float height;
    float halfradius;
    float halfheight;
    float hookshotSize;

    int wallDir = 1;

    private void Start()
    {
        CreateVaultHelper();
        playerInput = GetComponent<PlayerInput>();
        movement = GetComponent<PlayerMovement>();

        if (GetComponentInChildren<AnimateLean>())
            animateLean = GetComponentInChildren<AnimateLean>();

        slideLimit = movement.controller.slopeLimit - .1f;
        radius = movement.controller.radius;
        height = movement.controller.height;
        halfradius = radius / 2f;
        halfheight = height / 2f;
        rayDistance = halfheight + radius + .1f;

        hookshotTransform.gameObject.SetActive(false);

        speedboostParticleSystem = transform.Find("LeanAnimator").Find("Main Camera").Find("speedboost").GetComponent<ParticleSystem>();
        jumpboostParticleSystem = transform.Find("LeanAnimator").Find("Main Camera").Find("jumpboost").GetComponent<ParticleSystem>();
    }

    /******************************* UPDATE ******************************/
    void Update()//begrijpelijk, checks voor de verschillende states mogelijk (voeg hier een state check voor grappling hook toe)
    {
        //Updates
        UpdateInteraction();
        UpdateMovingStatus();


        //Check for movement updates
        CheckSliding();
        CheckCrouching();
        CheckForWallrun();
        CheckLadderClimbing();
        UpdateLedgeGrabbing();
        CheckForVault();
        CheckHookshot();
        //Add new check to change status right here

        //Misc
        UpdateLean();
    }

    void UpdateInteraction() //begrijpelijk, veranderingen maken als nieuwe states toegevoegd worden
    {
        if (!canInteract)
        {
            if (movement.grounded || movement.moveDirection.y < 0) //kan alleen interacten wanneer de speler grounded is of wanneer de movedirection van de speler < 0 is
                canInteract = true;
        }
        else if ((int)status >= 6) //kan niet interacten als de state 6 of meer is
            canInteract = false;
    }

    void UpdateMovingStatus() //begrijpelijk en gecomment
    {
        if ((int)status <= 1) //idle state als er niks gedaan wordt, en moving state bij inputs
        {
            status = Status.idle;
            if (playerInput.input.magnitude > 0.02f)
                status = Status.moving;
        }
    }

    void UpdateLean() //animaties voor wallrun, slide, ledgegrabbing, en vaulting, nog uitvogelen hoe animaties in Unity werken
    {
        if (animateLean == null) return;
        Vector2 lean = Vector2.zero;
        if (status == Status.wallRunning)
            lean.x = wallDir;
        if (status == Status.sliding && controlledSlide)
            lean.y = -1;
        else if (status == Status.grabbedLedge || status == Status.vaulting)
            lean.y = 1;
        animateLean.SetLean(lean);
    }
    /*********************************************************************/


    /******************************** MOVE *******************************/
    void FixedUpdate()//snap enigsinds, uitzoeken hoe enum en switch/case/break werken
    {
        switch (status) //worden gepakt uit de status enum en verbonden met een functie
        {
            case Status.sliding:
                SlideMovement();
                break;
            case Status.climbingLadder:
                LadderMovement();
                break;
            case Status.grabbedLedge:
                GrabbedLedgeMovement();
                break;
            case Status.climbingLedge:
                ClimbLedgeMovement();
                break;
            case Status.wallRunning:
                WallrunningMovement();
                break;
            case Status.vaulting:
                VaultMovement();
                break;
            case Status.hookshotPulled:
                HookshotMovement();
                break;
            case Status.hookshotThrown:
                HookshotThrow();
                break;
            default:
                DefaultMovement();
                break;
        }
    }

    void DefaultMovement()//begrijpelijk, kan toevoegingen maken wellicht
    {
        if (playerInput.run && status == Status.crouching)//uncrouch wanneer je tijdens crouch begint met sprinten
            Uncrouch();

        movement.Move(playerInput.input, playerInput.run, (status == Status.crouching));
        if (movement.grounded && playerInput.Jump())
        {
            if (status == Status.crouching)//bij jump uncrouch ook
                Uncrouch();

            movement.Jump(Vector3.up, 1f);//movement omhoog bij jump
            playerInput.ResetJump();
        }
    }
    /*********************************************************************/

    /****************************** SLIDING ******************************/
    void SlideMovement()//begrijpelijk, hier aanpassen voor slide jump slide
    {
        if (movement.grounded && playerInput.Jump())//kan uit een slide springen, warbij de speler de richting van de camera opspringt
        {
            if (controlledSlide)
                slideDir = transform.forward;
            movement.Jump(slideDir + Vector3.up, 0.1f);
            playerInput.ResetJump();
            slideTime = 0;
        }

        movement.Move(slideDir, movement.slideSpeed, 0.1f);//na bepaalde tijd sliden gaat de slide over tot een standaard crouch als C ingedrukt wordt gehouden
        if (slideTime <= 0)
        {
            if (playerInput.crouching)
                Crouch();
            else
                Uncrouch();
        }
    }

    void CheckSliding()
    {
        //Check slide tijdens sprint
        if(playerInput.crouch && canSlide())//check de canSlide en input
        {
            slideDir = transform.forward;//slide in de richting waar de speler nu opgaat
            movement.controller.height = halfheight;//halveert lengte van de controller
            controlledSlide = true;
            slideTime = 1f;//bepaalt de duur van de slide
        }

        //verlaging slidetime
        if (slideTime > 0)
        {
            status = Status.sliding;
            slideTime -= Time.deltaTime;//geleidelijke afname van de slideTime
        }

        if (Physics.Raycast(transform.position, -Vector3.up, out var hit, rayDistance))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > slideLimit && movement.moveDirection.y < 0)
            {
                Vector3 hitNormal = hit.normal;
                slideDir = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                Vector3.OrthoNormalize(ref hitNormal, ref slideDir);
                controlledSlide = false;
                status = Status.sliding;
            }
        }
    }

    bool canSlide()//begrijpelijk en gecomment
    {
        if (!movement.grounded) return false;//moet grounded zijn
        if (!playerInput.run) return false;//moet bewegen
        if (slideTime > 0 || status == Status.sliding) return false;//moet niet al aan t sliden zijn
        return true;
    }
    /*********************************************************************/

    /***************************** CROUCHING *****************************/
    void CheckCrouching()//begrijpelijk, gecomment
    {
        if (!movement.grounded || (int)status > 2) return;//kan niet crouchen bij de state 2 en hoger en wanneer player niet grounded is

        if(playerInput.crouch)
        {
            if (status != Status.crouching)//crouch wanneer je nog niet croucht, uncrouch wanneer je croucht
                Crouch();
            else
                Uncrouch();
        }
    }

    void Crouch()//begrijpelijk, gecomment
    {
        movement.controller.height = halfheight;//halveer de height
        status = Status.crouching;//zet status naar crouching
    }

    void Uncrouch()//begrijpelijk, gecomment
    {
        movement.controller.height = height;//verander height terug naar standaard
        status = Status.moving;//status terug naar moving
    }
    /*********************************************************************/

    /************************** LADDER CLIMBING **************************/
    void LadderMovement()
    {
        Vector3 input = playerInput.input;
        Vector3 move = Vector3.Cross(Vector3.up, ladderNormal).normalized;
        move *= input.x;
        move.y = input.y * movement.walkSpeed;

        bool goToGround = false;
        goToGround = (move.y < -0.02f && movement.grounded);

        if (playerInput.Jump())
        {
            movement.Jump((-ladderNormal + Vector3.up * 2f).normalized, 1f);
            playerInput.ResetJump();
            status = Status.moving;
        }

        if (!hasObjectInfront(0.05f, ladderLayer) || goToGround)
        {
            status = Status.moving;
            Vector3 pushUp = ladderNormal;
            pushUp.y = 0.25f;

            movement.ForceMove(pushUp, movement.walkSpeed, 0.25f, true);
        }
        else
            movement.Move(move, 1f, 0f);
    }

    void CheckLadderClimbing()
    {
        if (!canInteract)
            return;
        //Check for ladder all across player (so they cannot use the side)
        bool right = Physics.Raycast(transform.position + (transform.right * halfradius), transform.forward, radius + 0.125f, ladderLayer);
        bool left = Physics.Raycast(transform.position - (transform.right * halfradius), transform.forward, radius + 0.125f, ladderLayer);

        if (Physics.Raycast(transform.position, transform.forward, out var hit, radius + 0.125f, ladderLayer) && right && left)
        {
            if (hit.normal != hit.transform.forward) return;

            ladderNormal = -hit.normal;
            if (hasObjectInfront(0.05f, ladderLayer) && playerInput.input.y > 0.02f)
            {
                canInteract = false;
                status = Status.climbingLadder;
            }
        }
    }
    /*********************************************************************/

    /**************************** WALLRUNNING ****************************/
    void WallrunningMovement()
    {
        Vector3 input = playerInput.input;
        float s = (input.y > 0) ? input.y : 0;

        Vector3 move = wallNormal * s;

        if (playerInput.Jump())
        {
            movement.Jump(((Vector3.up * (s + 0.5f)) + (wallNormal * 2f * s) + (transform.right * -wallDir * 1.25f)).normalized, s + 0.5f);
            playerInput.ResetJump();
            status = Status.moving;
        }

        if (!hasWallToSide(wallDir) || movement.grounded)
            status = Status.moving;

        movement.Move(move, movement.runSpeed, (1f - s) + (s / 4f));
    }

    void CheckForWallrun()
    {
        if (!canInteract || movement.grounded || movement.moveDirection.y >= 0 || !playerInput.run)
            return;

        int wall = 0;
        if (hasWallToSide(1))
            wall = 1;
        else if (hasWallToSide(-1))
            wall = -1;

        if (wall == 0) return;

        if(Physics.Raycast(transform.position + (transform.right * wall * radius), transform.right * wall, out var hit, halfradius, wallrunLayer))
        {
            wallDir = wall;
            wallNormal = Vector3.Cross(hit.normal, Vector3.up) * -wallDir;
            status = Status.wallRunning;
        }
    }

    bool hasWallToSide(int dir)
    {
        //Check for ladder in front of player
        Vector3 top = transform.position + (transform.right * 0.25f * dir);
        Vector3 bottom = top - (transform.up * radius);
        top += (transform.up * radius);

        return (Physics.CapsuleCastAll(top, bottom, 0.25f, transform.right * dir, 0.05f, wallrunLayer).Length >= 1);
    }
    /*********************************************************************/

    /******************** LEDGE GRABBING AND CLIMBING ********************/
    void GrabbedLedgeMovement()//gecomment en begrijpelijk
    {
        if (playerInput.Jump()) //als de speler springt terwijl hij aan de ledge hangt, springt hij er van af in de richting waar hij naar kijkt
        {
            movement.Jump((Vector3.up + transform.forward).normalized, 1.2f);
            playerInput.ResetJump();
            status = Status.moving;
        }

        movement.Move(Vector3.zero, 0f, 0f); //Stay in place
    }

    void ClimbLedgeMovement()//gecomment en begrijpelijk
    {
        Vector3 dir = pushFrom - transform.position; //beweging naar voren
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized; //beweging naar boven
        Vector3 move = Vector3.Cross(dir, right).normalized; //beweging naar voren en naar boven nemen om zo een ledge te beklimmen

        movement.Move(move, movement.walkSpeed, 0f); //uitvoering van de beweging wanneer W of S worden ingedrukt
        if (new Vector2(dir.x, dir.z).magnitude < 0.125f) //brengt player van de "climbing Ledge" state terug naar de "idle" state, als input <0.125f is
            status = Status.idle;
    }

    void CheckLedgeGrab()//gecomment en begrijpelijk
    {
        //Check for ledge to grab onto 
        Vector3 dir = transform.TransformDirection(new Vector3(0, -0.5f, 1).normalized); //richting van de centerpoint, iets naar beneden angled voor ledges
        Vector3 pos = transform.position + (Vector3.up * height / 3f) + (transform.forward * radius / 2f); //positie van de centerpoint
        bool right = Physics.Raycast(pos + (transform.right * radius / 2f), dir, radius + 0.125f, ledgeLayer); //rechterkant van de player controller
        bool left = Physics.Raycast(pos - (transform.right * radius / 2f), dir, radius + 0.125f, ledgeLayer); //linkerkant van de controller

        if (Physics.Raycast(pos, dir, out var hit, radius + 0.125f, ledgeLayer) && right && left) //wanneer centerpoint, linker-, en rechter raycast true zijn
        {
            Vector3 rotatePos = transform.InverseTransformPoint(hit.point);
            rotatePos.x = 0; rotatePos.z = 1;
            pushFrom = transform.position + transform.TransformDirection(rotatePos); //grab the position with local z = 1
            rotatePos.z = radius * 2f;

            Vector3 checkCollisions = transform.position + transform.TransformDirection(rotatePos); //grab it closer now

            canInteract = false;
            status = Status.grabbedLedge;

        }
    }

    void UpdateLedgeGrabbing()//gecomment en begrijpelijk
    {
        if (movement.grounded || movement.moveDirection.y > 0)//grounded of in de lucht zorgt ervoor dat je ledge kan pakken
            canGrabLedge = true;

        if (status != Status.climbingLedge)
        {
            if (canGrabLedge && !movement.grounded)//check ledgegrab wanneer de canGrabLedge true is, de player niet grounded is, en de player mid-air is
            {
                if (movement.moveDirection.y < 0)
                    CheckLedgeGrab();
            }

            if (status == Status.grabbedLedge) //wanneer de ledge grabbed is laat de speler S of W gebruiken om van de ledge af te vallen of op de ledge te klimmen
            {
                canGrabLedge = false; //cangrabledge is false omdat je al aan de ledge hangt
                Vector2 down = playerInput.down;
                if (down.y == -1)
                    status = Status.moving; //laat je van de ledge vallen als je hangend op S drukt
                //else if (down.y == 1)
                //    status = Status.climbingLedge; //laat je op de ledge klimmen als je hangend op W drukt
            }
        }
    }
    /*********************************************************************/

    /***************************** VAULTING ******************************/
    void VaultMovement()
    {
        Vector3 dir = vaultOver - transform.position;
        Vector3 localPos = vaultHelper.transform.InverseTransformPoint(transform.position);
        Vector3 move = (vaultDir + (Vector3.up * -(localPos.z - radius) * height)).normalized;

        if(localPos.z > halfheight)
        {
            movement.controller.height = height;
            status = Status.moving;
        }

        movement.Move(move, movement.runSpeed, 0f);
    }

    void CheckForVault()
    {
        if (status == Status.vaulting) return;

        float checkDis = 0.05f;
        checkDis += (movement.controller.velocity.magnitude / 16f); //Check farther if moving faster
        if(hasObjectInfront(checkDis, vaultLayer) && playerInput.Jump())
        {
            if (Physics.SphereCast(transform.position + (transform.forward * (radius - 0.25f)), 0.25f, transform.forward, out var sphereHit, checkDis, vaultLayer))
            {
                if (Physics.SphereCast(sphereHit.point + (Vector3.up * halfheight), radius, Vector3.down, out var hit, halfheight - radius, vaultLayer))
                {
                    //Check above the point to make sure the player can fit
                    if (Physics.SphereCast(hit.point + (Vector3.up * radius), radius, Vector3.up, out var trash, height-radius))
                        return; //If cannot fit the player then do not vault

                    vaultOver = hit.point;
                    vaultDir = transform.forward;
                    SetVaultHelper();

                    canInteract = false;
                    status = Status.vaulting;
                    movement.controller.height = radius;
                }
            }
        }
    }

    void CreateVaultHelper()
    {
        vaultHelper = new GameObject();
        vaultHelper.transform.name = "(IGNORE) Vault Helper";
    }

    void SetVaultHelper()
    {
        vaultHelper.transform.position = vaultOver;
        vaultHelper.transform.rotation = Quaternion.LookRotation(vaultDir);
    }
    /*********************************************************************/

    /**************************** HOOKSHOT  ******************************/
    void CheckHookshot()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit, 100, HookshotLayer))
            {
                hookshotPos = raycastHit.point;
                hookshotSize = 0f;
                hookshotTransform.gameObject.SetActive(true);
                hookshotTransform.localScale = Vector3.zero;
                status = Status.hookshotThrown;
            }
        }
    }

    void HookshotThrow()
    {
        hookshotTransform.LookAt(hookshotPos);

        float hookshotThrowSpeed = 250f;
        hookshotSize += hookshotThrowSpeed * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        if (hookshotSize >= Vector3.Distance(transform.position, hookshotPos))
        {
            status = Status.hookshotPulled;
        }
    }
    void HookshotMovement()
    {
        float hookshotSpeedMin = 10f;
        float hookshotSpeedMax = 40f;
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(hookshotPos, transform.position), hookshotSpeedMin, hookshotSpeedMax);
        float hookshotSpeedMultiplier = 3f;

        Vector3 hookshotDir = (hookshotPos - transform.position).normalized;

        hookshotTransform.LookAt(hookshotPos);

        CharacterController.Move(hookshotDir * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime);

        float reachedHookshotPosDistance = 2f;
        if (Vector3.Distance(transform.position, hookshotPos) < reachedHookshotPosDistance)
        {
            status = Status.grabbedLedge;
            hookshotTransform.gameObject.SetActive(false);
        }
    }
    /*********************************************************************/

    /***************************** BOOSTS  *******************************/
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch(hit.gameObject.tag)
        {
            case "Speedboost":
                speedboostParticleSystem.Play();
                movement.runSpeed = 20f;
                movement.walkSpeed = 16f;
                movement.slideSpeed = 24f;
                break;
            case "JumpPad":
                jumpboostParticleSystem.Play();
                movement.jumpSpeed = 16f;
                break;
            case "Ground":
                speedboostParticleSystem.Stop();
                jumpboostParticleSystem.Stop();
                movement.jumpSpeed = movement.initialJumpSpeed;
                movement.runSpeed = movement.initialRunSpeed;
                movement.walkSpeed = movement.initialWalkSpeed;
                movement.slideSpeed = movement.initialSlideSpeed;
                break;
        }
    }
    /*********************************************************************/
    bool hasObjectInfront(float dis, LayerMask layer)
    {
        Vector3 top = transform.position + (transform.forward * 0.25f);
        Vector3 bottom = top - (transform.up * halfheight);

        return (Physics.CapsuleCastAll(top, bottom, 0.25f, transform.forward, dis, layer).Length >= 1);
    }
}
