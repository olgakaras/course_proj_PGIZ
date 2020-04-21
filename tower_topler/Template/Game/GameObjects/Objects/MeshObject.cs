using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer11 = SharpDX.Direct3D11.Buffer;
using Template.Graphics;

namespace Template
{
    /// <summary>
    /// 3D object with mesh.
    /// </summary>
    public class MeshObject : PositionalObject, IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct VertexDataStruct
        {
            public Vector4 position;
            public Vector4 normal;
            public Vector4 color;
            public Vector2 texCoord0;
            public Vector2 texCoord1;
        }
        public OrientedBoundingBox Collider { get; set; }
        public bool IsVisible { get; set; }
        public bool IsMoveable { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }

        private DirectX3DGraphics _directX3DGraphics;

        /// <summary>Renderer object.</summary>
        private Renderer _renderer;

        #region Vertices and Indexes
        /// <summary>Count of object vertices.</summary>
        private int _verticesCount;

        /// <summary>Array of vertex data.</summary>
        private VertexDataStruct[] _vertices;

        /// <summary>Vertex buffer DirectX object.</summary>
        private Buffer11 _vertexBufferObject;

        private VertexBufferBinding _vertexBufferBinding;

        /// <summary>Count of object vertex Indexes.</summary>
        private int _indexesCount;

        /// <summary>Array of object vertex indexes.</summary>
        private uint[] _indexes;

        private Buffer11 _indexBufferObject;
        #endregion

        private Material _material;
        public Material Material { get => _material; set => _material = value; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="renderer">Renderer object.</param>
        /// <param name="initialPosition">Initial position in 3d scene.</param>
        /// <param name="yaw">Initial angle of rotation around 0Y axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="pitch">Initial angle of rotation around 0X axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="roll">Initial rotation around 0Z axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="vertices">Array of vertex data.</param>
        public MeshObject(string name, DirectX3DGraphics directX3DGraphics, Renderer renderer,
            Vector4 initialPosition,
            VertexDataStruct[] vertices, uint[] indexes, Material material) :
            base(initialPosition)
        {
            IsMoveable = true;
            IsVisible = true;
            Name = name;
            _directX3DGraphics = directX3DGraphics;
            _renderer = renderer;
            if (null != vertices)
            {
                _vertices = vertices;
                _verticesCount = _vertices.Length;
            }
            if (null != indexes)
            {
                _indexes = indexes;
                _indexesCount = _indexes.Length;
            } else
            {
                _indexesCount = _verticesCount;
                _indexes = new uint[_indexesCount];
                for (int i = 0; i <= _indexesCount; ++i) _indexes[i] = (uint)i;
            }
            _material = material;
            
            _vertexBufferObject = Buffer11.Create(_directX3DGraphics.Device, BindFlags.VertexBuffer, _vertices, Utilities.SizeOf<VertexDataStruct>() * _verticesCount);
            _vertexBufferBinding = new VertexBufferBinding(_vertexBufferObject, Utilities.SizeOf<VertexDataStruct>(), 0);
            _indexBufferObject = Buffer11.Create(_directX3DGraphics.Device, BindFlags.IndexBuffer, _indexes, Utilities.SizeOf<int>() * _indexesCount);
            Collider = new OrientedBoundingBox(GetMin(), GetMax());
            
        }

        public virtual void Render(Matrix viewMatrix, Matrix projectionMatrix)
        {
            if (!IsVisible) return;
            _renderer.UpdatePerObjectConstantBuffer(0, GetWorldMatrix(), viewMatrix, projectionMatrix);
            DeviceContext deviceContext = _directX3DGraphics.DeviceContext;
            _renderer.UpdateMaterialProperties(_material);
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.InputAssembler.SetVertexBuffers(0, _vertexBufferBinding);
            deviceContext.InputAssembler.SetIndexBuffer(_indexBufferObject, Format.R32_UInt, 0);
            deviceContext.DrawIndexed(_indexesCount, 0, 0);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref _indexBufferObject);
            Utilities.Dispose(ref _vertexBufferObject);
        }

        public OrientedBoundingBox GetNewCollider(Vector4 position)
        {
            Vector3 min = Vector3.Add((Vector3)position, GetRawMin());
            Vector3 max = Vector3.Add((Vector3)position, GetRawMax());
            return new OrientedBoundingBox(min, max);
        }

        public Vector3 GetMin()
        {   
            return Vector3.Add((Vector3)Position, GetRawMin());
        }

        private Vector3 GetRawMin()
        {
            Vector3 min = (Vector3)_vertices[0].position;
            for (int index = 1; index < _vertices.Length; index++)
            {
                min = Vector3.Min(min, (Vector3)_vertices[index].position);
            }
            return min;
        }

        private Vector3 GetRawMax()
        {
            Vector3 max = (Vector3)_vertices[0].position;
            for (int index = 1; index < _vertices.Length; index++)
            {
                max = Vector3.Max(max, (Vector3)_vertices[index].position);
            }
            return max;
        }

        public Vector3 GetMax()
        {
            return Vector3.Add((Vector3)Position, GetRawMax());
        }
    }
}
