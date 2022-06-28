using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancingTest : MonoBehaviour
{
   [SerializeField] private Mesh _mesh;
   [SerializeField] private Material _material;
   private static int InstCount = 400;

   private static int baseColorId = Shader.PropertyToID("_BaseColor");

   private MaterialPropertyBlock _matPropBlock;

   private Matrix4x4[] _matrices = new Matrix4x4[400];
   private Vector4[] _colors = new Vector4[400];

   private void Awake()
   {
      for (int i = 0; i < _matrices.Length; i++)
      {
         _matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10f, Quaternion.identity, Vector3.one);
         _colors[i] = new Vector4(Random.value, Random.value, Random.value,Random.value);
      }

      _matPropBlock = new MaterialPropertyBlock();
      _matPropBlock.SetVectorArray(baseColorId, _colors);
   }

   private void Update()
   {
      Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices,400,_matPropBlock);
   }
   
}
