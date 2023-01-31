using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EquipUtils
{
    public static GameObject EquipItem(GameObject target, GameObject itemPrefab)
    {
        try
        {
            var item = GameObject.Instantiate(itemPrefab);
            SkinnedMeshRenderer targetRenderer = target.GetComponentInChildren<SkinnedMeshRenderer>();

            RootBone rootBone = target.GetComponentInChildren<RootBone>();

            if (rootBone.transform == null)
            {
                Debug.Log("Couldnt get root bone.");
                return null;
            }
            GameObject newGo = new GameObject();
            newGo.transform.position = rootBone.transform.position;
            newGo.transform.parent = rootBone.transform;

            Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
            foreach (Transform bone in targetRenderer.bones)
            {
                boneMap[bone.name] = bone;
            }

            SkinnedMeshRenderer[] itemRenderers = item.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (var itemRenderer in itemRenderers)
            {
                Transform[] boneArray = itemRenderer.bones;
                for (int idx = 0; idx < boneArray.Length; ++idx)
                {
                    string boneName = boneArray[idx].name;
                    if (false == boneMap.TryGetValue(boneName, out boneArray[idx]))
                    {
                        //Debug.LogError("failed to get bone: " + boneName);
                        //Debug.Break();
                    }
                }
                itemRenderer.bones = boneArray; //take effect
                itemRenderer.transform.parent = newGo.transform;//rootBone.transform;
                itemRenderer.rootBone = rootBone.transform;
                itemRenderer.updateWhenOffscreen = true;
                MeshFilter mFilter = itemRenderer.GetComponent<MeshFilter>();
                if (mFilter != null)
                {
                    mFilter.mesh.RecalculateBounds();
                }
            }
            GameObject.Destroy(item.gameObject);
            return newGo.gameObject;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
            return null;
        }
    }
}
