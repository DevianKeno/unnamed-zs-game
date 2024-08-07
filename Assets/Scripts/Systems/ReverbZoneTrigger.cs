using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverbZoneTrigger : MonoBehaviour
{
    Collider _reverbTrigger;
    AudioReverbZone _reverbZone;

    private void Awake()
    {
        _reverbTrigger = GetComponent<Collider>();
        _reverbZone = GetComponent<AudioReverbZone>();
    }

    private void OnTriggerEnter(Collider other)
    {
        print("ENTERED!");
        _reverbZone.enabled = !_reverbZone.enabled;
    }

    private void OnTriggerExit(Collider other)
    {
        print("EXITED!");
        _reverbZone.enabled = !_reverbZone.enabled;
    }
}
