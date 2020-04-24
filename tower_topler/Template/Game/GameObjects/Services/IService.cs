using SharpDX;

namespace Template.Game.GameObjects.Services
{
    /// <summary>
    /// describes all the services in the game
    /// </summary>
    interface IService
    {
        /// <summary>
        /// updates data each frame
        /// </summary>
        void Update();
        /// <summary>
        /// renders data each frame
        /// </summary>
        /// <param name="view">view matrix</param>
        /// <param name="projection">projection matrix</param>
        void Render(Matrix view, Matrix projection);
    }
}
