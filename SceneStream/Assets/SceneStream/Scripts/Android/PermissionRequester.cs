using UnityEngine;
using System.Collections;
using UnityEngine.Android;

public class PermissionRequester : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(RequestPermissions());
    }

    IEnumerator RequestPermissions()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
                yield return new WaitForSeconds(1.0f);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
                yield return new WaitForSeconds(1.0f);
            }
        }
    }
}