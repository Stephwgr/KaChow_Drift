using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using TMPro;
using MoreMountains.Feedbacks;
using Unity.VisualScripting;

public class CarController : MonoBehaviour
{
    private Rigidbody _rbPlayer;
    public WheelColliders _wheelColliders;
    public WheelMeshes _wheelMeshes;
    public WheelParticles _wheelParticles;

    private float _gasInput;
    private float _steeringInput;
    private float _brakeInput;
    public float _changeCameraInput;

    [Header("DATA POWER")]
    [SerializeField] private float _motorPower;
    [SerializeField] private float _breakPower;
    [SerializeField] private float _kmhMax;

    [Header("BOOST")]
    [SerializeField] private float _boostTimer = 0f;
    [SerializeField] private float _boostDuration = 2f;
    [SerializeField] private float _boostMultiplier = 1.5f;
    [SerializeField] private float _bosstActivationSpeed = 10f;

    [Header("Tolerance au DRIFT")]
    [SerializeField] private float _driftSpeedThreshold = 50f;

    [ShowInInspector, ReadOnly]
    [SerializeField] private float _currentKmh;


    private float _slipAngle; //angle de glissement
    private float _speed;
    public AnimationCurve _steeringCurve;

    [Header("PARTICLE SYSTEM")]
    [SerializeField] private float slipAllowance;
    public GameObject _smokePrefab;

    [Header("SCORE SYSTEM")]
    [SerializeField] public int _score = 0;
    [SerializeField] private float _driftMultiplier = 2.3f;
    private float _driftTimer = 10.63f;
    [SerializeField] private bool isDrifting = false;
    [SerializeField] private TextMeshProUGUI _scoreText;
    // [SerializeField] private TextMeshProUGUI _multiplierText;
    [SerializeField] private GameObject _scoreGameObject;
    private Coroutine hideScoreCoroutine; // Hide -> Cacher
    public float _timerHideScore = 3f;



    [Header("INPUT SYSTEM")]
    public InputActionReference moveActionFloat;
    public InputActionReference gasAction;
    public InputActionReference brakeAction;

    public InputActionReference changeCamera;

    [Header("CAMERA")]
    [SerializeField] private GameObject _cameraDrift;
    [SerializeField] private GameObject _mainCamera;

    [Header("SOUND AUDIO SYSTEM")]
    // public MMF_Player _mmfPlayer;
    public AudioSource _engineAudioSource;
    public AudioSource _driftAudioSource;
    [SerializeField] private float _volumeFadeSpeed = 1f; // Vitesse de fade-out


    private void Start()
    {
        _rbPlayer = gameObject.GetComponent<Rigidbody>();
        InstantiateSmoke();
        _boostTimer = 0f; // Initialiser le timer du boost à zéro

        SetWheelStiffness(15f);

        _cameraDrift.SetActive(false);
        _mainCamera.SetActive(true);

        _scoreGameObject.SetActive(false);



    }


    private void Update()
    {
        _speed = _rbPlayer.velocity.magnitude;
        _currentKmh = _speed * 3.6f;

        float stiffness = (_currentKmh < _driftSpeedThreshold) ? 20f : 1.5f;
        SetWheelStiffness(stiffness);

        if (_currentKmh < _bosstActivationSpeed && _gasInput > 0)
        {
            _boostTimer = _boostDuration;
        }

        if (_boostTimer > 0)
        {
            _boostTimer -= Time.deltaTime;
        }

        // Update Score Text
        _scoreText.text = Mathf.RoundToInt(_score).ToString();
        // _multiplierText.text = "x " + Mathf.RoundToInt(_driftTimer).ToString();
        _driftTimer += Time.deltaTime;


        CheckInput();
        ApplyWheelPositions();
        ApplyMotor();
        ApplySteering();
        ApplyBrake();
        CheckParticles();
        ChangingCamera();
        UpdateDriftScore();
        UpdateEngineSound();

    }

    public int GetScore()
    {
        return _score;
    }

    void UpdateDriftScore()
    {
        if (_currentKmh > _driftSpeedThreshold && isDrifting == true)
        {
            _scoreGameObject.SetActive(true);

            if (!_driftAudioSource.isPlaying)
            {
                _driftAudioSource.Play();
                // Ajuster le pitch de manière aléatoire entre 1.0 et 1.5
                _driftAudioSource.pitch = Random.Range(1.0f, 1.2f);
            }

            if (hideScoreCoroutine != null)
            {
                StopCoroutine(hideScoreCoroutine);
                hideScoreCoroutine = null;
            }

            _score += Mathf.RoundToInt(_driftTimer * _driftMultiplier);
        }
        else
        {
            if (_driftAudioSource.isPlaying)
            {
                _driftAudioSource.Stop();
            }

            if (hideScoreCoroutine == null)
            {
                hideScoreCoroutine = StartCoroutine(HideScoreAfterDelay(_timerHideScore));
                _driftTimer = 0f;

            }

        }
    }

    private IEnumerator HideScoreAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        _scoreGameObject.SetActive(false);

        hideScoreCoroutine = null;

    }

    void ChangingCamera()
    {
        _changeCameraInput = changeCamera.action.ReadValue<float>();

        if (_changeCameraInput == 1)
        {
            _cameraDrift.SetActive(true);
            _mainCamera.SetActive(false);

        }
        else
        {
            _cameraDrift.SetActive(false);
            _mainCamera.SetActive(true);


        }
    }


    void SetWheelStiffness(float stiffness)
    {
        // Fonction pour ajuster la stiffness des roues sans perdre les autres paramètres
        void UpdateFriction(WheelCollider wheel)
        {
            var sidewaysFriction = wheel.sidewaysFriction;
            sidewaysFriction.stiffness = stiffness;
            wheel.sidewaysFriction = sidewaysFriction;

            var forwardFriction = wheel.forwardFriction;
            forwardFriction.stiffness = stiffness;
            wheel.forwardFriction = forwardFriction;
        }

        // Mise à jour de toutes les roues
        UpdateFriction(_wheelColliders.FRWheel);
        UpdateFriction(_wheelColliders.FLWheel);
        UpdateFriction(_wheelColliders.RRWheel);
        UpdateFriction(_wheelColliders.RLWheel);

    }

    void CheckInput()
    {
        _steeringInput = moveActionFloat.action.ReadValue<float>();
        _gasInput = gasAction.action.ReadValue<float>();
        // _brakeInput = brakeAction.action.ReadValue<float>();
        // _backInput = -backAction.action.ReadValue<float>();



        _slipAngle = Vector3.Angle(transform.forward, _rbPlayer.velocity - transform.forward);

        if (_slipAngle < 120)
        {
            if (_gasInput < 0)
            {
                _brakeInput = Mathf.Abs(_gasInput);
                _gasInput = 0f;
            }
            else
            {
                _brakeInput = 0f;
            }
        }
        else
        {
            _brakeInput = 0f;
        }

    }

    void InstantiateSmoke()
    {
        _wheelParticles.FRWheel = Instantiate(_smokePrefab, _wheelColliders.FRWheel.transform.position - Vector3.up * _wheelColliders.FRWheel.radius, Quaternion.identity, _wheelColliders.FRWheel.transform)
            .GetComponent<ParticleSystem>();
        _wheelParticles.FLWheel = Instantiate(_smokePrefab, _wheelColliders.FLWheel.transform.position - Vector3.up * _wheelColliders.FLWheel.radius, Quaternion.identity, _wheelColliders.FLWheel.transform)
            .GetComponent<ParticleSystem>();
        _wheelParticles.RRWheel = Instantiate(_smokePrefab, _wheelColliders.RRWheel.transform.position - Vector3.up * _wheelColliders.RRWheel.radius, Quaternion.identity, _wheelColliders.RRWheel.transform)
            .GetComponent<ParticleSystem>();
        _wheelParticles.RLWheel = Instantiate(_smokePrefab, _wheelColliders.RLWheel.transform.position - Vector3.up * _wheelColliders.RLWheel.radius, Quaternion.identity, _wheelColliders.RLWheel.transform)
            .GetComponent<ParticleSystem>();
    }

    void CheckParticles()
    {
        WheelHit[] wheelHits = new WheelHit[4];
        _wheelColliders.FRWheel.GetGroundHit(out wheelHits[0]);
        _wheelColliders.FLWheel.GetGroundHit(out wheelHits[1]);
        _wheelColliders.RRWheel.GetGroundHit(out wheelHits[2]);
        _wheelColliders.RLWheel.GetGroundHit(out wheelHits[3]);

        // Condition pour drifter uniquement si la vitesse dépasse le seuil
        if (_currentKmh > _driftSpeedThreshold)
        {
            isDrifting = false;
            if ((Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipAllowance))
            {
                _wheelParticles.FRWheel.Play();
                isDrifting = true;
            }
            else
            {
                _wheelParticles.FRWheel.Stop();
            }
            if ((Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipAllowance))
            {
                _wheelParticles.FLWheel.Play();
                isDrifting = true;
            }
            else
            {
                _wheelParticles.FLWheel.Stop();
            }
            if ((Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipAllowance))
            {
                _wheelParticles.RRWheel.Play();
                isDrifting = true;
            }
            else
            {
                _wheelParticles.RRWheel.Stop();
            }
            if ((Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipAllowance))
            {
                _wheelParticles.RLWheel.Play();
                isDrifting = true;
            }
            else
            {
                _wheelParticles.RLWheel.Stop();
            }
        }
        else
        {
            // Si la vitesse est en dessous du seuil, arrêter les particules
            _wheelParticles.FRWheel.Stop();
            _wheelParticles.FLWheel.Stop();
            _wheelParticles.RRWheel.Stop();
            _wheelParticles.RLWheel.Stop();
        }
    }

    void ApplyBrake()
    {
        // Réduire la force de freinage pour ralentir moins
        float brakeForce = (_gasInput == 0) ? 100000f : 0f;

        _wheelColliders.FRWheel.brakeTorque = brakeForce * 0.7f;
        _wheelColliders.FLWheel.brakeTorque = brakeForce * 0.7f;
        _wheelColliders.RRWheel.brakeTorque = brakeForce * 0.3F;
        _wheelColliders.RLWheel.brakeTorque = brakeForce * 0.3F;
    }

    void ApplySteering()
    {
        float steeringAngle = _steeringInput * _steeringCurve.Evaluate(_speed);

        if (_currentKmh >= _driftSpeedThreshold)
        {
            steeringAngle += Vector3.SignedAngle(transform.forward, _rbPlayer.velocity + transform.forward, Vector3.up);
        }

        steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);

        _wheelColliders.FRWheel.steerAngle = steeringAngle;
        _wheelColliders.FLWheel.steerAngle = steeringAngle;




        // -------------------------------------- A GARDER AU K OU -----------------------------------------
        // Contre-braquage basé sur la vitesse et la dérive
        // float counterSteer = Vector3.SignedAngle(transform.forward, _rbPlayer.velocity, Vector3.up);
        // counterSteer = Mathf.Clamp(counterSteer, -10f, 10f);
        // steeringAngle += counterSteer;

        // if(_gasInput < 0)
        // {
        //     steeringAngle = - steeringAngle;
        // }
    }

    void ApplyMotor()
    {
        // Convertir la vitesse en km/h
        float currentSpeedKmh = _rbPlayer.velocity.magnitude * 3.6f;

        // Calculer l'effet du boost au démarrage (seulement si le boost est actif)
        float boostEffect = (_boostTimer > 0) ? _boostMultiplier : 50f;

        // Si la vitesse dépasse la limite, désactiver le couple moteur
        if (currentSpeedKmh < _kmhMax)
        {
            if (currentSpeedKmh >= _driftSpeedThreshold)
            {
                _wheelColliders.RRWheel.motorTorque = _motorPower * _gasInput * 0.8f * boostEffect;
                _wheelColliders.RLWheel.motorTorque = _motorPower * _gasInput * 0.8f * boostEffect;
            }
            else
            {
                // Applique moins de couple pour éviter le drift à basse vitesse
                _wheelColliders.RRWheel.motorTorque = _motorPower * _gasInput * 0.4f; // Ajuste le facteur si nécessaire
                _wheelColliders.RLWheel.motorTorque = _motorPower * _gasInput * 0.4f;
            }
        }
        else
        {
            _wheelColliders.RRWheel.motorTorque = 0f;
            _wheelColliders.RLWheel.motorTorque = 0f;
        }

        // Gestion du son du moteur
        if (currentSpeedKmh >= _kmhMax)
        {
            _rbPlayer.velocity = _rbPlayer.velocity.normalized * (_kmhMax / 3.6f); // Limite la vitesse
        }

    }

    private void UpdateEngineSound()
    {
        // Si on accélère et que le son du moteur n'est pas déjà en train de jouer
        if (_gasInput > 0 && !_engineAudioSource.isPlaying)
        {
            _engineAudioSource.Play(); // Jouer le son du moteur si on commence à accélérer
        }

        // Calculer la proportion de la vitesse actuelle par rapport à la vitesse maximale
        float speedRatio = Mathf.Clamp01(_currentKmh / _kmhMax); // Clamp pour éviter de dépasser 1

        if (_gasInput > 0)
        {
            // Augmenter progressivement le volume en fonction de la vitesse
            _engineAudioSource.volume = Mathf.Lerp(_engineAudioSource.volume, speedRatio * 0.3f, Time.deltaTime * _volumeFadeSpeed);

            // Ajuster le pitch en fonction de la vitesse pour simuler l'accélération
            _engineAudioSource.pitch = Mathf.Lerp(_engineAudioSource.pitch, 1f + speedRatio, Time.deltaTime * _volumeFadeSpeed);
        }
        else
        {
            // Diminuer progressivement le volume quand on relâche l'accélération
            _engineAudioSource.volume = Mathf.Lerp(_engineAudioSource.volume, 0f, Time.deltaTime * _volumeFadeSpeed);

            // Si le volume est presque à 0, on arrête le son
            if (_engineAudioSource.volume <= 0.05f && _engineAudioSource.isPlaying)
            {
                _engineAudioSource.Stop();
            }
        }
    }


    void ApplyWheelPositions()
    {
        UpdateWheel(_wheelColliders.FRWheel, _wheelMeshes.FRWheel);
        UpdateWheel(_wheelColliders.FLWheel, _wheelMeshes.FLWheel);
        UpdateWheel(_wheelColliders.RRWheel, _wheelMeshes.RRWheel);
        UpdateWheel(_wheelColliders.RLWheel, _wheelMeshes.RLWheel);

    }

    void UpdateWheel(WheelCollider coll, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 position;
        coll.GetWorldPose(out position, out quat);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = quat;
    }


}

[System.Serializable]
public class WheelColliders
{
    // FR -> Front Right / FL -> Front Left // RL RearLeft (Rear = Arrière) // RR etc...
    public WheelCollider FRWheel;
    public WheelCollider FLWheel;
    public WheelCollider RRWheel;
    public WheelCollider RLWheel;
}

[System.Serializable]
public class WheelMeshes
{
    // FR -> Front Right / FL -> Front Left // RL RearLeft (Rear = Arrière) // RR etc...
    public MeshRenderer FRWheel;
    public MeshRenderer FLWheel;
    public MeshRenderer RRWheel;
    public MeshRenderer RLWheel;
}

[System.Serializable]
public class WheelParticles
{
    public ParticleSystem FRWheel;
    public ParticleSystem FLWheel;
    public ParticleSystem RRWheel;
    public ParticleSystem RLWheel;
}

