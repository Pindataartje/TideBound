using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mainAudioMixer;
    [SerializeField] private Slider musicslider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Slider ambienceSlider;
}
