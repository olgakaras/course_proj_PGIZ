using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Graphics
{
    public class Textures : IDisposable
    {
        private List<Texture> _textures;
        public int Count { get => _textures.Count; }
        public Texture this[int index] { get => _textures[index]; }
        public Texture this[string name]
        {
            get
            {
                foreach (Texture texture in _textures)
                {
                    if (texture.Name == name) return texture;
                }
                return null;
            }
        }

        public Textures()
        {
            _textures = new List<Texture>(4);
        }

        public void Add(Texture texture)
        {
            _textures.Add(texture);
            texture.Index = _textures.Count - 1;
        }

        public void Dispose()
        {
            for (int i = _textures.Count - 1; i >= 0; --i)
            {
                Texture texture = _textures[i];
                _textures.RemoveAt(i);
                texture.Dispose();
            }
        }
    }
}
