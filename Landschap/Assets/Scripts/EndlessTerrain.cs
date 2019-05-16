﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
	public LODInfo[] detailLevels;
	public static float maxViewDst;
	public Transform viewer;
	public Material mapMaterial;
	public static Vector2 viewerPosition;
	Vector2 viewerPositionOld;
	static MapGenerator mapGen;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	static List<TerrainChunk> terrainChunkVisibleLastUpdate = new List<TerrainChunk>();

	int chunkSize;
	int chunksVisibleInViewDst;

	void Start()
	{
		mapGen = FindObjectOfType<MapGenerator>();
		maxViewDst = detailLevels[detailLevels.Length -1].visibleDistThreshold;
		chunkSize = MapGenerator.mapChunkSize -1;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst/ chunkSize);
		UpdateVisibleChunks();
	}
	void Update()
	{
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGen.terrainData.uniformScale;

		if((viewerPositionOld-viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
		{
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
		
	}

	void UpdateVisibleChunks()
	{
		for(int i = 0; i < terrainChunkVisibleLastUpdate.Count; i++)
		{
			terrainChunkVisibleLastUpdate[i].SetVisible(false);
		}
		terrainChunkVisibleLastUpdate.Clear();

		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

		for(int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for(int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX+xOffset, currentChunkCoordY+yOffset);
				if(terrainChunkDictionary.ContainsKey(viewedChunkCoord))
				{
					terrainChunkDictionary[viewedChunkCoord].UpdateTerrain();
					//terrainChunkVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
				}
				else
				{
					terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
				}
			}

		}
	}

	public class TerrainChunk
	{
		GameObject meshObject;
		MeshData meshdata;
		Vector2 position;
		Bounds bounds;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;

		MapData mapData;
		bool mapDataReceived;
		int previousLODIndex = -1;
		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
		{
			this.detailLevels = detailLevels;
			position = coord * size;
			Vector3 positionV3 = new Vector3(position.x,0,position.y);
			bounds = new Bounds(position, Vector2.one * size);
			meshObject = new GameObject("Terrain Chunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer.material = material;
			
			meshObject.transform.position = positionV3 * mapGen.terrainData.uniformScale;
			meshObject.transform.localScale = Vector3.one * mapGen.terrainData.uniformScale;
			meshObject.transform.parent = parent;
			SetVisible(false);
			lodMeshes = new LODMesh[detailLevels.Length];
			for(int i = 0; i < detailLevels.Length; i++)
			{
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrain);
			}
			mapGen.RequestMapData(position, OnMapDataReceived);
		}
		void OnMapDataReceived(MapData mapData)
		{
			this.mapData = mapData;
			mapDataReceived = true;

		
			UpdateTerrain();
		}
		void OnMeshDataReceived(MeshData meshData)
		{
			meshFilter.mesh = meshData.CreateMesh();
		}

		public void UpdateTerrain()
		{
			if(mapDataReceived)
			{
				float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
				bool visible = viewerDstFromNearestEdge <= maxViewDst;

				if(visible)
				{
					int lodIndex = 0;
					for(int i = 0; i < detailLevels.Length -1; i++)
					{
						if(viewerDstFromNearestEdge > detailLevels[i].visibleDistThreshold)
						{
							lodIndex = i+1;
							
						}
						else
							break;
					}
					if(lodIndex != previousLODIndex)
					{
						LODMesh lodMesh = lodMeshes[lodIndex];
						if(lodMesh.hasMesh)
						{
							previousLODIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
						}
						else if(!lodMesh.hasRequestedMesh)
						{
							lodMesh.RequestMesh(mapData);
						}

					}
                    terrainChunkVisibleLastUpdate.Add(this);
				}
				SetVisible(visible);
			}
		}

		public void SetVisible(bool visible)
		{
			meshObject.SetActive(visible);
		}
	}
	class LODMesh
	{
		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;
		System.Action updateCallback;

		public LODMesh(int lod, System.Action updateCallback)
		{
			this.lod = lod;
			this.updateCallback = updateCallback;
		}
		void OnMeshDataReceived(MeshData meshData)
		{
			mesh = meshData.CreateMesh();
			hasMesh = true;
			updateCallback();
		}

		public void RequestMesh(MapData mapData)
		{
			hasRequestedMesh = true;
			mapGen.RequestMeshData(mapData,lod, OnMeshDataReceived);
		}
	}

	[System.Serializable]
	public struct LODInfo
	{
		public int lod;
		public float visibleDistThreshold;
	}

}
