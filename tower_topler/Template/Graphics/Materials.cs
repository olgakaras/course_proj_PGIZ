using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Graphics
{
    public class Materials
    {
        private List<Material> _materials;
        public int Count { get => _materials.Count; }
        public Material this[int index] { get { return _materials[index]; } }
        public Material this[string name]
        {
            get
            {
                foreach (Material material in _materials)
                {
                    if (material.Name == name) return material;
                }
                return null;
            }
        }

        public Materials()
        {
            _materials = new List<Material>(4);
        }

        public void Add(Material material)
        {
            _materials.Add(material);
            material.Index = _materials.Count - 1;
        }
    }
}
