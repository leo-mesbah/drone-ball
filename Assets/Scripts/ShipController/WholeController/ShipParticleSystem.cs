﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipParticleSystem : MonoBehaviour
{
    public ParticleSystem windPs, boostPs;
    public GameObject firePs;

    const int SupersonicThreshold = 2200 / 100;
    InputManager _inputManager;
    ShipController _shipController;
    ShipBoosting _shipBoosting;
    private TrailRenderer[] _trails;
    bool _isBoostAnimationPlaying = false;

    void Start()
    {
        _shipController = GetComponentInParent<ShipController>();
        _shipBoosting = GetComponentInParent<ShipBoosting>();
        _inputManager = transform.parent.GetComponentInParent<InputManager>();
        _trails = GetComponentsInChildren<TrailRenderer>();
        _trails[0].time = _trails[1].time = 0;
        firePs.SetActive(false);

        windPs.transform.position += new Vector3(0, 0, 10);
    }

    void Update()
    {
        if (_shipBoosting.isBoosting)
        {
            if (_isBoostAnimationPlaying == false)
            {
                boostPs.Play();
                firePs.SetActive(true);
                _isBoostAnimationPlaying = true;
            }
        }
        else if (!_shipBoosting.isBoosting)
        {
            boostPs.Stop();
            firePs.SetActive(false);
            _isBoostAnimationPlaying = false;
        }
    }

    const float TrailLength = 0.075f;

    private void FixedUpdate()
    {
        //  Wind and trail effect
        if (_shipController.forwardSpeed >= SupersonicThreshold)
        {
            windPs.Play();
            
            if (_shipController.shipState == ShipController.ShipStates.AllCornersSurface)
                _trails[0].time = _trails[1].time = Mathf.Lerp(_trails[1].time, TrailLength, Time.fixedDeltaTime * 5);
            else 
                _trails[0].time = _trails[1].time = 0;
        }
        
        else
        {
            windPs.Stop();
            
            _trails[0].time = _trails[1].time = Mathf.Lerp(_trails[1].time, 0.029f, Time.fixedDeltaTime * 6);
            if (_trails[0].time <= 0.03f)
                _trails[0].time = _trails[1].time = 0;
        }
    }
}