using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
    [Header ("Character Level")]
    //Character's level:
    [SerializeField] int level = 1;
    [SerializeField] float exp;

    [Space (10)]
    [Header ("Combat Stats")]
    //Combat stats:
    [SerializeField] float currentHealth;
    [SerializeField] float maxHealth;

    [SerializeField] float currentStamina;
    [SerializeField] float maxStamina;
    [SerializeField] float staminaRecoveryTimer = 100;
    [SerializeField] float staminaRecoveryTime = 50; //in ms
 
    //Movement:
    [SerializeField] MoveState moveState;

    [Header("Walking"), Space(10)]

    [SerializeField] float maxAcceleration;
    [SerializeField] float maxSpeed;
    [SerializeField] float runSpeedMod;
    [SerializeField] float rollHoldTimer;
    [SerializeField] float rollTimer;

    [SerializeField] bool isRunning;

    //Gliding:
    [Header("Gliding"), Space(10)]
    [SerializeField] float maxGlidingAcceleration_H;
    [SerializeField] float maxGlidingAcceleration_V;
    [SerializeField] float maxGlideHorizontal;
    [SerializeField] float maxGlideVertical;
    [SerializeField] float updraftMagnitude;
    [SerializeField] float maxUpdraftSpeed;

    //Functions:
    void SetStats ()
    {
        currentHealth = maxHealth;

        currentStamina = maxStamina;
        staminaRecoveryTimer = 100;
    }





}
