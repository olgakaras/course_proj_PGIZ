using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Graphics;

namespace Template.Game.gameObjects.newObjects
{
    public class DrawableObject : PositionalObject, IDisposable
    {
        public override Vector4 Position { 
            get => base.Position; 
            set 
            {
                base.Position = value;
                foreach(var meshObject in MeshObjects)
                {
                    if (!meshObject.IsMoveable) continue;
                    meshObject.Position = value;
                }
                if (ColliderMesh != null)
                {
                    ColliderMesh.Collider = ColliderMesh.GetNewCollider(ColliderMesh.Position);
                }
            }
        }

        public void SetRawPosition(Vector4 position)
        {
            base.Position = position;
        }

        public override float Yaw 
        {
            get => base.Yaw; 
            set
            {
                base.Yaw = value;
                foreach (var meshObject in MeshObjects)
                {
                    meshObject.Yaw = value;
                }
            }
        }
        public MeshObject ColliderMesh { get; set; }
        public List<MeshObject> MeshObjects { get; protected set; }
        public MeshObject this[string name] { get => MeshObjects.Find(m => m.Name.Equals(name)); }
        public DrawableObject(Vector4 initialPosition) : base(initialPosition)
        {
            MeshObjects = new List<MeshObject>();
        }

        public void SetCollider()
        {
            ColliderMesh = MeshObjects.Find(m => m.Name.Equals("collider"));
            ColliderMesh.Collider = ColliderMesh.GetNewCollider(Position);
        }

        public void SetCollider(string name)
        {
            ColliderMesh = MeshObjects.Find(m => m.Name.Equals(name));
            ColliderMesh.Collider = ColliderMesh.GetNewCollider(Position);
        }

        public void AddMeshObject(MeshObject meshObject)
        {
            meshObject.Position = position;
            MeshObjects.Add(meshObject);
        }

        public void AddMeshObjects(List<MeshObject> meshObjects)
        {
            meshObjects.ForEach(mesh => mesh.Position = position);
            MeshObjects.AddRange(meshObjects);
        }

        public virtual void Render(Matrix view, Matrix projection)
        {
            foreach (var mesh in MeshObjects)
                mesh.Render(view, projection);
        }

        public void Dispose()
        {
            MeshObjects.ForEach(m => m.Dispose());
        }
    }
}
