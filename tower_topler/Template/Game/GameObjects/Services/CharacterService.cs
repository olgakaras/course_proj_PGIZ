using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;
using Template.Game.GameObjects.Objects;
using static Template.Game.gameObjects.interfaces.Character;

namespace Template.Game.GameObjects.Services
{
    public class CharacterService : IService
    {
        private readonly InputController controller;
        private readonly List<DrawableObject> walls;
        private readonly Character character;
        private DrawableObject lastFloor;

        public CharacterService(Loader loader, InputController controller, List<DrawableObject> walls)
        {
            this.controller = controller;
            this.walls = walls;
            character = new Character(new Vector4(-65, 20, 0, 0));
            character.AddMeshObjects(loader.LoadMeshesFromObject("resources\\objects\\character\\character.obj", null));
            character.SetCollider();



            lastFloor = FindFrstFloor(walls);
            if (lastFloor is LiftPlatform)
                ((LiftPlatform)lastFloor).Up = 1;
            //lastFloor = walls[0];
        }

        private DrawableObject FindFrstFloor(List<DrawableObject> walls)
        {
            Ray checkRay = new Ray((Vector3)character.Position, new Vector3(0, -1, 0));
            List<DrawableObject> inters = walls.FindAll(m => m.ColliderMesh != null).FindAll(w => w.ColliderMesh.Collider.Intersects(ref checkRay));

            DrawableObject first = inters[0];
            Vector3 interPoint;
            inters[0].ColliderMesh.Collider.Intersects(ref checkRay, out interPoint);
            float lenght = checkRay.Position.Y - interPoint.Y;

            for (int i = 1; i < inters.Count; i++)
            {
                inters[i].ColliderMesh.Collider.Intersects(ref checkRay, out interPoint);
                if (lenght > checkRay.Position.Y - interPoint.Y)
                {
                    lenght = checkRay.Position.Y - interPoint.Y;
                    first = inters[i];
                }
            }
            return first;
        }

        public void Update()
        {
            character.Horizontal = 0;
            if (controller[Key.A]) character.Horizontal = -1;
            if (controller[Key.D]) character.Horizontal = 1;

            VerticalMove();
            HorizontalMove();

            CheckFloor();

            Console.WriteLine(character.ColliderMesh.Collider.Contains(lastFloor.ColliderMesh.Collider));
        }

        private void CheckFloor()
        {
            Ray checkRay = new Ray((Vector3)character.Position, new Vector3(0, -1, 0));
            Vector3 interPoint;
            if (!lastFloor.ColliderMesh.Collider.Intersects(ref checkRay, out interPoint) || checkRay.Position.Y - interPoint.Y > 1)
                character.IsFlying = true;
            else if (character.ColliderMesh.Collider.Contains(lastFloor.ColliderMesh.Collider) != ContainmentType.Disjoint)
            {
                while(true)
                {
                    Vector4 newPos = character.GetNewVerticalPosition(0.1f);
                    BoundingBox collider = character.ColliderMesh.GetNewCollider(newPos);
                    if (collider.Contains(lastFloor.ColliderMesh.Collider) == ContainmentType.Disjoint)
                    {
                        character.Position = newPos;
                        return;
                    }
                    character.Position = newPos;
                }
            }
        }

        public void Render(Matrix view, Matrix projection)
        {
            walls.ForEach(w => w.Render(view, projection));
            character.Render(view, projection);
        }

        private void VerticalMove()
        {
            if (!character.IsFlying && controller[Key.Space])
            {
                character.IsFlying = true;
                character.Speed.Y = VERCTICAL_SPEED;
            }

            if (!character.IsFlying)
                return;

            Vector4 newPos = character.GetNewVerticalPosition();

            BoundingBox characterCollider = character.ColliderMesh.GetNewCollider(newPos);
            foreach(var wall in walls)
            {
                if (wall.ColliderMesh == null) continue;
                BoundingBox wallCollider = wall.ColliderMesh.Collider;
                if (characterCollider.Contains(ref wallCollider) != ContainmentType.Disjoint)
                {
                    while (true)
                    {
                        newPos = character.GetNewVerticalPosition(-0.1f);
                        characterCollider = character.ColliderMesh.GetNewCollider(newPos);

                        if (characterCollider.Contains(ref wallCollider) != ContainmentType.Disjoint)
                        {
                            ChangeLift(lastFloor as LiftPlatform, "off");
                            character.IsFlying = false;
                            lastFloor = wall;
                            ChangeLift(lastFloor as LiftPlatform, "on");
                            return;
                        }
                        character.Position = newPos;
                    }
                }
            }
            character.Position = newPos;
            character.Speed.Y -= GRAVITY;

        }

        private void ChangeLift(LiftPlatform platform, string action)
        {
            if (platform == null) return;
            switch(action)
            {
                case "off":
                    if (platform.Position.Y > platform.MinMax.X)
                        platform.Up = -1;
                    break;
                case "on":
                    if (platform.Position.Y < platform.MinMax.Y)
                        platform.Up = 1;
                    break;
            }
        }

        private void HoMove()
        {
            if (character.Horizontal == 0) return;
            Vector4 newPosition = character.GetNewHorizontalPosition();
            BoundingBox characterCollider = character.ColliderMesh.GetNewCollider(newPosition);
            foreach(var wall in walls)
            {
                if (wall.ColliderMesh == null) continue;
                BoundingBox wallCollider = wall.ColliderMesh.Collider;
                if (characterCollider.Contains(ref wallCollider) != ContainmentType.Disjoint)
                {
                    while (true)
                    {                       
                        characterCollider = character.ColliderMesh.GetNewCollider(character.GetNewHorizontalPosition(0.1f));

                        if (characterCollider.Contains(ref wallCollider) != ContainmentType.Disjoint)
                            return;
                        character.Position = character.GetNewHorizontalPosition(0.1f);
                    }
                }
            }
            character.Position = newPosition;
        }

        private void HorizontalMove()
        {
            if (character.Horizontal == 0) return;
            List<DrawableObject> platforms = walls.FindAll(w => w is Platform);
            foreach(var platform in platforms)
            {
                float yaw;
                Vector4 newPos = ((Platform)platform).GetNewHorizontalPosition(character.Speed.X * character.Horizontal, 65, out yaw);
                BoundingBox platformBox = platform.ColliderMesh.GetNewCollider(newPos);
                if (character.ColliderMesh.Collider.Contains(platformBox) != ContainmentType.Disjoint)
                {
                    while (true)
                    {
                        newPos = ((Platform)platform).GetNewHorizontalPosition(0.01f * character.Horizontal, 65, out yaw);
                        platformBox = platform.ColliderMesh.GetNewCollider(newPos);
                        if (character.ColliderMesh.Collider.Contains(platformBox) != ContainmentType.Disjoint)
                            return;
                        MoveScene(0.01f * character.Horizontal, 65);
                    }
                }
            }
            MoveScene(character.Speed.X * character.Horizontal, 65);
        }

        private void MoveScene(float delta, float radius)
        {
            DrawableObject obj = walls[1];
            obj.Yaw += delta;

            walls.Skip(2).ToList().ForEach(w =>
            {
                float yaw;
                w.Position = ((Platform)w).GetNewHorizontalPosition(delta, radius, out yaw);
                w.Yaw = yaw;
            });
        }
    }
}
