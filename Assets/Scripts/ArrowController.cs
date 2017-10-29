using HoloToolkit.Unity.InputModule;
using System.Collections;
using UnityEngine;

public class ArrowController : MonoBehaviour, ISpeechHandler, IInputClickHandler
{
    public float Smoothing = 5.0f;
    public float TurnSmoothing = 5.0f;
    public GameObject Damage;
    private Vector3 TargetPosition
    {
        get { return _targetPosition; }
        set
        {
            if (_targetPosition != value)
            {
                _targetPosition = value;
            }
            StopCoroutine("MoveObject");
            StopCoroutine("TurnObject");
            StartCoroutine("TurnObject", _targetPosition);
            StartCoroutine("MoveObject", _targetPosition);
        }
    }
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private GameObject _enemy;
    private Vector3 TargetNormal;
    AudioSource soundEffect;

    // Use this for initialization
    void Start()
    {
        InputManager.Instance.PushModalInputHandler(gameObject);
        soundEffect = GetComponent<AudioSource>();
    }

    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        if (eventData.RecognizedText == "Move")
        {
            MoveArrow();
        }
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        MoveArrow();
    }

    private void MoveArrow()
    {
        _enemy = GazeManager.Instance.HitObject;
        if (_enemy != null)
        {
            TargetPosition = GazeManager.Instance.HitPosition;
            TargetNormal = GazeManager.Instance.HitNormal;
            soundEffect.Play();
        }
        else
        {
            TargetPosition = GazeManager.Instance.GazeTransform.position + (GazeManager.Instance.GazeTransform.forward * GazeManager.Instance.MaxGazeCollisionDistance);
            Vector3 relativePosition = TargetPosition - transform.position;
            transform.rotation = Quaternion.LookRotation(relativePosition);
        }
    }

    IEnumerator MoveObject(Vector3 target)
    {
        Debug.Log("Moving arrow)");
        while (Vector3.Distance(transform.position, target) > 0.0f)
        {
            transform.position = Vector3.Lerp(transform.position, target, Smoothing * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator TurnObject(Vector3 target)
    {
        Debug.Log("Turning arrow)");
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 relativePosition = target - transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(relativePosition);
        while (Vector3.Dot(forward, relativePosition) != -1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, TurnSmoothing * Time.deltaTime);
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Instantiate(Damage, gameObject.transform.position, Quaternion.LookRotation(TargetNormal));
    }


}
