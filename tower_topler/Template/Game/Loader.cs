using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.WIC;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Template.Graphics;
using ObjLoader.Loader.Loaders;
using static Template.MeshObject;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Data;
using Material = Template.Graphics.Material;

namespace Template
{
    /// <summary>Load objects data from text files, material libraries from text files, textures from images.</summary>
    public class Loader
    {
        public Texture StubTexture { get; set; }
        private SamplerStates samplerState;
        private DirectX3DGraphics _directX3DGraphics;
        private DirectX2DGraphics _directX2DGraphics;
        private Renderer _renderer;
        private ImagingFactory _imagingFactory;
        private Dictionary<string, Texture> textures;
        private Dictionary<string, Material> materials;

        public Loader(DirectX3DGraphics directX3DGraphics, DirectX2DGraphics directX2DGraphics, Renderer renderer, ImagingFactory imagingFactory)
        {
            materials = new Dictionary<string, Material>();
            textures = new Dictionary<string, Texture>();

            _directX3DGraphics = directX3DGraphics;
            _directX2DGraphics = directX2DGraphics;
            _renderer = renderer;
            _imagingFactory = imagingFactory;
            samplerState = new SamplerStates(_directX3DGraphics);
        }

        private float ParseFloat(string str)
        {
            return float.Parse(str, CultureInfo.InvariantCulture);
        }

        private Vector2 ParseFloat2(string str)
        {
            string[] strs = str.Trim().Split(' ');
            return new Vector2(ParseFloat(strs[0]), ParseFloat(strs[1]));
        }

        private Vector4 ParseFloat4(string str)
        {
            string[] strs = str.Trim().Split(' ');
            return new Vector4(ParseFloat(strs[0]), ParseFloat(strs[1]), ParseFloat(strs[2]), ParseFloat(strs[3]));
        }

        private string NormalizeStringAndRemoveComment(string str)
        {
            string result = str.Replace('\n', ' ').Replace('\t', ' ');
            while (result.IndexOf("  ") >= 0) result = result.Replace("  ", " ");
            int commentPosition = result.IndexOf("//");
            if (commentPosition >= 0) result = result.Remove(commentPosition);
            return result.Trim();
        }

        private string SkipEmptyStrings(StreamReader reader)
        {
            string str;
            do
            {
                str = NormalizeStringAndRemoveComment(reader.ReadLine());
            } while (string.IsNullOrEmpty(str));
            return str;
        }

        public Texture LoadTextureFromFile(string fileName, bool generateMip, SamplerState samplerState)
        {
            BitmapDecoder decoder = new BitmapDecoder(_imagingFactory, fileName, DecodeOptions.CacheOnDemand);
            BitmapFrameDecode bitmapFirstFrame = decoder.GetFrame(0);

            Utilities.Dispose(ref decoder);

            FormatConverter imageFormatConverter = new FormatConverter(_imagingFactory);
            imageFormatConverter.Initialize(
                bitmapFirstFrame,
                PixelFormat.Format32bppRGBA,
                BitmapDitherType.None, null, 0.0, BitmapPaletteType.Custom);

            int stride = imageFormatConverter.Size.Width * 4;
            DataStream buffer = new DataStream(imageFormatConverter.Size.Height * stride, true, true);
            imageFormatConverter.CopyPixels(stride, buffer);

            int width = imageFormatConverter.Size.Width;
            int height = imageFormatConverter.Size.Height;

            Texture2DDescription textureDescription = new Texture2DDescription()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = _directX3DGraphics.SampleDescription, // new SampleDescription(1,0)
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = (generateMip ? ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.None)
            };
            Texture2D textureObject = new Texture2D(_directX3DGraphics.Device, textureDescription, new DataRectangle(buffer.DataPointer, stride));

            int mipLevels = (int)Math.Log(width, 2) + 1;
            ShaderResourceViewDescription shaderResourceViewDescription = new ShaderResourceViewDescription();
            shaderResourceViewDescription.Dimension = ShaderResourceViewDimension.Texture2D;
            shaderResourceViewDescription.Format = Format.R8G8B8A8_UNorm;
            shaderResourceViewDescription.Texture2D = new ShaderResourceViewDescription.Texture2DResource
            {
                MostDetailedMip = 0,
                MipLevels = -1 // (generateMip ? mipLevels : -1)
            };
            ShaderResourceView shaderResourceView = new ShaderResourceView(_directX3DGraphics.Device, textureObject, shaderResourceViewDescription);
            if (generateMip) _directX3DGraphics.DeviceContext.GenerateMips(shaderResourceView);

            Utilities.Dispose(ref imageFormatConverter);
            string[] path = fileName.Split('\\');
            fileName = path[path.Length - 1];

            Texture texture = new Texture(textureObject, shaderResourceView, width, height, fileName, samplerState);
            textures.Add(texture.Name, texture);

            return texture;
        }

        public Materials LoadMaterials(string materialFileName, Textures textures)
        {
            Materials materials = new Materials();
            
            string materialName = null;
            Vector4 emissive;
            Vector4 ambient;
            Vector4 diffuse;
            Vector4 specular;
            float specularPower;
            bool textured;
            string textureName = null;

            StreamReader reader = File.OpenText(materialFileName);
            while (!reader.EndOfStream) {
                string str = NormalizeStringAndRemoveComment(reader.ReadLine());
                if (0 == str.IndexOf("name:"))
                {
                    materialName = str.Split(':')[1].Trim();
                    emissive = Vector4.UnitW;
                    ambient = Vector4.One;
                    diffuse = Vector4.UnitW;
                    specular = Vector4.UnitW;
                    specularPower = 32.0f;
                    textured = false;

                    do
                    {
                        str = NormalizeStringAndRemoveComment(reader.ReadLine());
                        if (str.IndexOf("emissive:") == 0) emissive = ParseFloat4(str.Split(':')[1].Trim());
                        if (str.IndexOf("ambient:") == 0) ambient = ParseFloat4(str.Split(':')[1].Trim());
                        if (str.IndexOf("diffuse:") == 0) diffuse = ParseFloat4(str.Split(':')[1].Trim());
                        if (str.IndexOf("specular:") == 0) specular = ParseFloat4(str.Split(':')[1].Trim());
                        if (str.IndexOf("specularPower:") == 0) specularPower = ParseFloat(str.Split(':')[1].Trim());
                        if (str.IndexOf("textured:") == 0) textured = (1 == int.Parse(str.Split(':')[1].Trim()));
                        if (str.IndexOf("texture:") == 0) textureName = str.Split(':')[1].Trim();
                    } while (!reader.EndOfStream && !string.IsNullOrEmpty(str));

                    Texture texture = textures[textureName];
                    Material material = new Material(materialName, emissive, ambient, diffuse, specular, specularPower, textured, texture);
                    materials.Add(material);
                }
            }

            return materials;
        }

        public MeshObject LoadMeshObject(string objectFileName, Materials materials)
        {
            int count = 0;

            StreamReader reader = File.OpenText(objectFileName);
            string str = string.Empty;

            str = SkipEmptyStrings(reader);
            if (str.IndexOf("verticesCount:") != 0) return null;
            else count = int.Parse(str.Split(':')[1].Trim());
            MeshObject.VertexDataStruct[] vertices = new MeshObject.VertexDataStruct[count];

            str = SkipEmptyStrings(reader);
            if (str.IndexOf("vertices:") != 0) return null;
            else
            {
                for (int i = 0; i <= count - 1; ++i)
                {
                    str = NormalizeStringAndRemoveComment(reader.ReadLine());
                    string[] strs = str.Split(';');
                    if (strs.Length < 5) return null;
                    MeshObject.VertexDataStruct vertex;
                    vertex.position = ParseFloat4(strs[0]);
                    vertex.normal = ParseFloat4(strs[1]);
                    vertex.color = ParseFloat4(strs[2]);
                    vertex.texCoord0 = ParseFloat2(strs[3]);
                    vertex.texCoord1 = ParseFloat2(strs[4]);
                    vertices[i] = vertex;
                }
            }

            str = SkipEmptyStrings(reader);
            if (str.IndexOf("trianglesCount:") != 0) return null;
            else count = int.Parse(str.Split(':')[1].Trim());
            uint[] indexes = new uint[count * 3];

            str = SkipEmptyStrings(reader);
            if (str.IndexOf("indices:") != 0) return null;
            else
            {
                for (int i = 0; i <= count - 1; ++i)
                {
                    str = NormalizeStringAndRemoveComment(reader.ReadLine());
                    string[] strs = str.Split(' ');
                    if (strs.Length < 3) return null;
                    for (int j = 0; j <= 2; ++j) indexes[i * 3 + j] = uint.Parse(strs[j]);
                }
            }

            str = SkipEmptyStrings(reader);
            Material material = null;
            if (str.IndexOf("material:") != 0) return null;
            else material = materials[str.Split(':')[1].Trim()];
            string[] path = objectFileName.Split('\\');
            objectFileName = path[path.Length - 1];

            MeshObject meshObject = new MeshObject(objectFileName, _directX3DGraphics, _renderer,
                new Vector4(0.0f),
                vertices, indexes, material);
            return meshObject;
        }

        public MeshObject LoadMeshFromObject(string file, Material material)
        {
            LoadResult result = GetResult(file);
            //Group group = result.Groups[0];
            //Console.WriteLine("----------------------------------");
            //Console.WriteLine(result.Vertices.Count);
            //Console.WriteLine(group.Name);
            //Console.WriteLine("----------------------------------");
            return GetMesh(result, 0, material);
        }

        public List<MeshObject> LoadMeshesFromObject(string file, Material material)
        {
            List<MeshObject> meshObjects = new List<MeshObject>();
            LoadResult result = GetResult(file);
            ((List<Group>)result.Groups).ForEach(el => Console.WriteLine(el.Name));
            for (int groupIndex = 0; groupIndex < result.Groups.Count; groupIndex++)
            {
                meshObjects.Add(GetMesh(result, groupIndex, material));
            }
            return meshObjects;
        }

        private MeshObject GetMesh(LoadResult result, int groupIndex, Material material)
        {
            Group group = result.Groups[groupIndex];
            int vertexCount = group.Faces.Count;
            VertexDataStruct[] vertices = new VertexDataStruct[vertexCount * 3];
            uint[] indexes = new uint[vertexCount * 3];
            uint count = 0;
            for (int index = 0; index < vertexCount; index++)
            {
                for (int vertIndex = 0; vertIndex < 3; vertIndex++)
                {
                    VertexDataStruct vertex = new VertexDataStruct();
                    int position = group.Faces[index][vertIndex].VertexIndex - 1;
                    vertex.position = GetPosition(result.Vertices[position]);
                    position = group.Faces[index][vertIndex].NormalIndex - 1;
                    vertex.normal = GetNormal(result.Normals[position]);
                    vertex.color = new Vector4(0.8f, 0.0f, 0.5f, 1.0f);
                    position = group.Faces[index][vertIndex].TextureIndex - 1;
                    vertex.texCoord0 = (position == -1) ? Vector2.Zero : GetTextureCoord(result.Textures[position]);
                    vertex.texCoord1 = Vector2.Zero;
                    vertices[count] = vertex;
                    indexes[count] = count++;
                }
                VertexDataStruct tempVertex = vertices[count - 3];
                vertices[count - 3] = vertices[count - 2];
                vertices[count - 2] = tempVertex;
            }
            Material targetMaterial = null;
            string[] path = group.Material.Name.Split('\\');
            string matName = path[path.Length - 1];
            if (group.Material != null)
            {
                if (materials.ContainsKey(matName)) targetMaterial = materials[matName];
                else targetMaterial = GetMaterial(group.Material);
            }
            Console.WriteLine($"target mat: {targetMaterial.Name}");
            return new MeshObject(group.Name, _directX3DGraphics, _renderer,
                new Vector4(0.0f),
                vertices, indexes, targetMaterial);
        }

        public Material GetMaterial(ObjLoader.Loader.Data.Material material)
        {
            bool isTextured = string.IsNullOrEmpty(material.AmbientTextureMap) ? false : true;
            Texture texture = null;
            string[] path = material.AmbientTextureMap.Split('\\');
            string textureName = path[path.Length - 1];
            if (!string.IsNullOrEmpty(textureName))
            {
                if (textures.ContainsKey(textureName)) texture = textures[textureName];
                else texture = LoadTextureFromFile(material.AmbientTextureMap, false, samplerState.Textured);
            }
            //string.IsNullOrEmpty(material.AmbientTextureMap) 
             //   ? StubTexture : LoadTextureFromFile(material.AmbientTextureMap, false, samplerState.Textured);
            Console.WriteLine($"isText: {isTextured}; texture: {texture}");

            string[] mPath = material.Name.Split('\\');
            string matName = mPath[mPath.Length - 1];

            Material resultMaterial = new Material(matName,
                new Vector4(0, 0, 0, 1.0f),
                GetFromVec3(material.AmbientColor),
                GetFromVec3(material.DiffuseColor),
                GetFromVec3(material.SpecularColor),
                material.SpecularCoefficient,
                isTextured,
                texture);

            materials.Add(resultMaterial.Name, resultMaterial);
            return resultMaterial;
        }

        public Vector4 GetFromVec3(Vec3 vec3)
        {
            return new Vector4(vec3.X, vec3.Y, vec3.Z, 1.0f);
        }

        private LoadResult GetResult(string file)
        {
            IObjLoaderFactory factory = new ObjLoaderFactory();
            IObjLoader objLoader = factory.Create();
            using(var stream = new FileStream(file, FileMode.Open))
            {          
                return objLoader.Load(stream);
            }
        }

        private Vector4 GetPosition(ObjLoader.Loader.Data.VertexData.Vertex vertex)
        {
            return new Vector4(vertex.X, vertex.Y, vertex.Z, 1.0f);
        }

        private Vector4 GetNormal(ObjLoader.Loader.Data.VertexData.Normal normal)
        {
            return new Vector4(normal.X, normal.Y, normal.Z, 1.0f);
        }

        private Vector2 GetTextureCoord(ObjLoader.Loader.Data.VertexData.Texture texture)
        {
            return new Vector2(texture.X, texture.Y);
        }
    }
}
