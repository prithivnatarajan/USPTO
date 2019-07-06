using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VRTK;

public class FaceViewer : MonoBehaviour
{

    private void Update()
    {
		if (VRTK_DeviceFinder.HeadsetCamera() != null)
			transform.rotation = Quaternion.LookRotation(transform.position - VRTK_DeviceFinder.HeadsetCamera().position);
    }
}