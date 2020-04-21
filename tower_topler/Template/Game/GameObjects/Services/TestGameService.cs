using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;
using static Template.Game.gameObjects.interfaces.Character;

namespace Template.Game.GameObjects.Services
{
    public class TestGameService
    {
        private InputController controller;
        private List<DrawableObject> walls;
        private Character character;
        private DrawableObject lastFloor;

        public TestGameService(Loader loader, InputController controller)
        {
            this.controller = controller;

            character = new Character(new Vector4(-30, 5, 0, 0));
            character.AddMeshObjects(loader.LoadMeshesFromObject("resources\\objects\\character\\character.obj", null));
            character.SetCollider();
            DrawableObject floor = new DrawableObject(Vector4.Zero);
            floor.AddMeshObjects(loader.LoadMeshesFromObject("resources\\objects\\floor\\floor.obj", null));
            floor.SetCollider();

            walls = new List<DrawableObject>();
            DrawableObject wall1 = new DrawableObject(new Vector4(-30, 1, -15, 0));
            wall1.AddMeshObjects(loader.LoadMeshesFromObject("resources\\objects\\platform\\platform.obj", null));
            wall1.SetCollider();
            walls.Add(wall1);

            DrawableObject wall2 = new DrawableObject(new Vector4(-30, 1, 25, 0));
            wall2.AddMeshObjects(loader.LoadMeshesFromObject("resources\\objects\\platform\\platform.obj", null));
            wall2.SetCollider();
            walls.Add(wall2);
            walls.Add(floor);

            lastFloor = floor;
        }


        public void Update()
        {
            character.Horizontal = 0;
            if (controller[Key.A]) character.Horizontal = 1;
            if (controller[Key.D]) character.Horizontal = -1;

            VerticalMove();
            HorizontalMove();

            CheckFloor();
        }

        private void CheckFloor()
        {
            Ray checkRay = new Ray((Vector3)character.Position, new Vector3(0, -1, 0));
            Vector3 interPoint;
            if (!lastFloor.ColliderMesh.Collider.Intersects(ref checkRay, out interPoint) || checkRay.Position.Y - interPoint.Y > 1)
                character.IsFlying = true;
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

            OrientedBoundingBox characterCollider = character.ColliderMesh.GetNewCollider(newPos);
            foreach(var wall in walls)
            {
                OrientedBoundingBox wallCollider = wall.ColliderMesh.Collider;
                if (characterCollider.Contains(ref wallCollider) != ContainmentType.Disjoint)
                {
                    while (true)
                    {
                        newPos = character.GetNewVerticalPosition(-0.1f);
                        characterCollider = character.ColliderMesh.GetNewCollider(newPos);

                        if (characterCollider.Contains(ref wallCollider) != ContainmentType.Disjoint)
                        {
                            character.IsFlying = false;
                            lastFloor = wall;
                            return;
                        }
                        character.Position = newPos;
                    }
                }
            }
            character.Position = newPos;
            character.Speed.Y -= GRAVITY;

        }

        private void HorizontalMove()
        {
            if (character.Horizontal == 0) return;
            Vector4 newPosition = character.GetNewHorizontalPosition();
            OrientedBoundingBox characterCollider = character.ColliderMesh.GetNewCollider(newPosition);
            foreach(var wall in walls)
            {
                OrientedBoundingBox wallCollider = wall.ColliderMesh.Collider;
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
    }
}
