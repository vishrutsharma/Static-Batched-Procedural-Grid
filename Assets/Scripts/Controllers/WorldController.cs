using System;
using UnityEngine;
using Snake3D.Views;
using Snake3D.Models;
using Snake3D.Components;
using System.Collections;
using System.Collections.Generic;

namespace Snake3D.Controllers
{
    public class WorldController : MonoBehaviour
    {
        #region ------------------------------- SerializeFields --------------------------------------

#pragma warning disable 649

        [SerializeField]
        private WorldModel worldModel;

        [SerializeField]
        private bool allowRandomGrid;

        [SerializeField]
        private Transform worldTransform;

        [SerializeField]
        private Transform boundaryTransform;

        [SerializeField]
        private Transform edgeTransform;

        [SerializeField]
        private GameObject worldViewPrefab;

        [SerializeField]
        private GameObject boundaryBlockPrefab;

#pragma warning restore 649

        #endregion ------------------------------------------------------------------------------------

        #region ------------------------------- Private Fields --------------------------------------

        private int gridX;
        private int gridY;
        private Vector2 worldBlockSize; // Taking the Collider Size for grid spawning
        private float worldAnimWaitTime = 2;
        private float blocksSpawnDelay = 0.001f;
        private List<WorldView> worldGridBlocks;
        private List<WorldView> unVisitedGridBlocks;


        #endregion ------------------------------------------------------------------------------------


        #region ------------------------------- Private Methods --------------------------------------

        /// <summary>
        /// Init WorldController
        /// </summary>
        private void Start()
        {
            worldGridBlocks = new List<WorldView>();
            unVisitedGridBlocks = new List<WorldView>();
            worldBlockSize = new Vector2(worldViewPrefab.GetComponent<BoxCollider>().size.x,
                                         worldViewPrefab.GetComponent<BoxCollider>().size.z);
            if (allowRandomGrid)
            {
                gridX = Mathf.RoundToInt(UnityEngine.Random.Range(worldModel.minRange, worldModel.maxRange));
                gridY = Mathf.RoundToInt(UnityEngine.Random.Range(worldModel.minRange, worldModel.maxRange));
            }
            else
            {
                gridX = worldModel.gridX;
                gridY = worldModel.gridY;
            }

            StartCoroutine(SpawnWorld(() => Debug.Log("Spawning Complete")));
        }

        /// <summary>
        /// Does Static Batching by combining meshes with same material
        /// </summary>
        private void CombineMeshes()
        {
            Transform parentTransform = worldTransform;

            for (int t = 0; t < 2; t++)
            {
                MeshFilter[] meshFilters = parentTransform.GetComponentsInChildren<MeshFilter>();
                CombineInstance[] combine = new CombineInstance[meshFilters.Length];

                int i = 0;
                while (i < meshFilters.Length)
                {
                    combine[i].mesh = meshFilters[i].sharedMesh;
                    combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

                    i++;
                }
                parentTransform.GetComponent<MeshFilter>().mesh = new Mesh();
                parentTransform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
                parentTransform.gameObject.SetActive(true);


                // To remove duplicate meshes
                foreach (Transform childBlocks in parentTransform)
                {
                    Destroy(childBlocks.GetComponent<MeshFilter>());
                    Destroy(childBlocks.GetComponent<MeshRenderer>());
                    childBlocks.gameObject.isStatic = true;
                }

                parentTransform = boundaryTransform;
            }

        }
        #endregion ------------------------------------------------------------------------------------

        #region ------------------------------- Public Methods --------------------------------------

        /// <summary>
        /// Spawning Random World Grid and Boundary Blocks based on gridX and gridY
        /// </summary>
        public IEnumerator SpawnWorld(Action OnWorldSpawningComplete)
        {
            // This gets the extreme bottom left world position from the transform's center
            Vector3 worldPos = worldTransform.position - Vector3.right * (gridX / 2) * worldBlockSize.x
                             - Vector3.forward * (gridY / 2) * worldBlockSize.y;

            Vector3 spawnPos;
            WorldView worldViewTemp;
            GameObject gridBlocksTemp = null;
            int id = 0;

            // For World Grid Spawning
            for (int y = 0; y < gridY; y++)
            {
                for (int x = 0; x < gridX; x++)
                {
                    spawnPos = worldPos + new Vector3(x * worldBlockSize.x, worldPos.y, y * worldBlockSize.y);
                    gridBlocksTemp = Instantiate(worldViewPrefab, spawnPos, Quaternion.identity);
                    gridBlocksTemp.transform.SetParent(worldTransform);
                    worldViewTemp = gridBlocksTemp.GetComponent<WorldView>();
                    worldViewTemp.GetComponent<SmoothAnimationComponent>().DoTranslation();
                    worldViewTemp.InitView(id, this);
                    worldGridBlocks.Add(worldViewTemp);
                    unVisitedGridBlocks.Add(worldViewTemp);
                    id++;
                    yield return new WaitForSeconds(blocksSpawnDelay);
                }
            }

            float xVal = -worldBlockSize.x;
            float zVal = -worldBlockSize.y;
            GameObject boundary = null;
            Transform parentTransform;
            //Horizontal Boundary Spawning
            for (int horizontal = 0; horizontal < 2; horizontal++)
            {
                for (int h = 0; h < gridY; h++)
                {
                    spawnPos = worldPos + new Vector3(xVal, worldPos.y, h * worldBlockSize.y);
                    boundary = Instantiate(boundaryBlockPrefab, spawnPos, Quaternion.identity);
                    boundary.transform.GetChild(0).gameObject.SetActive(true);
                    boundary.transform.SetParent(boundaryTransform);
                    boundary.GetComponent<SmoothAnimationComponent>().DoTranslation();
                    yield return new WaitForSeconds(blocksSpawnDelay);
                }

                xVal = worldBlockSize.x * gridX;
            }

            //Vertical Boundary Spawning
            for (int vertical = 0; vertical < 2; vertical++)
            {
                for (int v = 0; v < gridX + 2; v++) // As We need 2 extra wall blocks to spawn
                {
                    spawnPos = worldPos + new Vector3(worldBlockSize.x * v, worldPos.y, zVal);
                    spawnPos.x -= worldBlockSize.x;
                    boundary = Instantiate(boundaryBlockPrefab, spawnPos, Quaternion.Euler(0, 90, 0));

                    // If  blocks are Edges block activate Tree else activate Fence
                    if (v == 0 || v == gridX + 1)
                    {
                        parentTransform = edgeTransform;
                        boundary.transform.GetChild(1).gameObject.SetActive(true);
                        boundary.transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else
                    {
                        parentTransform = boundaryTransform;
                        boundary.transform.GetChild(1).gameObject.SetActive(false);
                        boundary.transform.GetChild(0).gameObject.SetActive(true);
                    }
                    boundary.transform.SetParent(parentTransform);
                    boundary.GetComponent<SmoothAnimationComponent>().DoTranslation();
                    yield return new WaitForSeconds(blocksSpawnDelay);
                }

                zVal = worldBlockSize.y * gridY;
            }

            yield return new WaitForSeconds(worldAnimWaitTime);
            CombineMeshes(); // For Static Batching at Runtime
            OnWorldSpawningComplete();
        }
    
        #endregion ------------------------------------------------------------------------------------

    }
}


