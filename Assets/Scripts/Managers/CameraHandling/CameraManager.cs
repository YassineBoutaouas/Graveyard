using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    [NonReorderable] public List<CinemachineVirtualCameraBase> VirtualCameras;

    private GameObject _targetHolder;
    [NonReorderable] public List<GameObject> Targets;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        else
            Instance = this;

        VirtualCameras = GetComponentsInChildren<CinemachineVirtualCameraBase>(true).ToList();
        _targetHolder = transform.Find("---TARGETS---").gameObject;
        Targets = _targetHolder.transform.GetChildren().ToList();
    }

    public CinemachineVirtualCameraBase GetVirtualCamera(string name)
    {
        CinemachineVirtualCameraBase virtualCam = null;

        foreach (CinemachineVirtualCameraBase cameraBase in VirtualCameras)
        {
            if (cameraBase.gameObject.name == name)
            {
                virtualCam = cameraBase;
                break;
            }
        }
        return virtualCam;
    }

    public GameObject GetTarget(string name)
    {
        GameObject t = null;
        foreach (GameObject target in Targets)
        {
            if(target.name == name)
            {
                t = target;
                break;
            }
        }
        return t;
    }

    public void ChangeCameras(CinemachineVirtualCameraBase inactiveCamera, CinemachineVirtualCameraBase activeCamera)
    {
        inactiveCamera.gameObject.SetActive(false);
        activeCamera.gameObject.SetActive(true);
    }
}
