using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.Animations;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;
using Template.Game.GameObjects.Objects;
using static Template.Game.gameObjects.interfaces.Character;

namespace Template.Game.GameObjects.Services
{
    public class CharacterService : IService
    {
        private RotateSceneAnimation animation;
        private readonly InputController controller;
        private readonly List<DrawableObject> walls;
        private readonly Character character;
        private DrawableObject lastFloor;
        private Vector2 flyingHeight;
        private bool isAnimation;

        public CharacterService(Loader loader, InputController controller, List<DrawableObject> walls)
        {
            flyingHeight = Vector2.Zero;
            this.controller = controller;
            this.walls = walls;
            character = new Character(new Vector4(-66, 20, 0, 0));
            character.Yaw = 0;
            character.AddMeshObjects(loader.LoadMeshesFromObject("resources\\objects\\character\\character.obj", null));
            character.SetCollider();

            animation = new RotateSceneAnimation(walls.ToList<PositionalObject>());

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
            if (controller[Key.W])
                EnterTheDoor();


            if (isAnimation)
            {
                animation.Animate();
                return;
            }

            character.Horizontal = 0;
            if (controller[Key.A]) character.Horizontal = -1;
            if (controller[Key.D]) character.Horizontal = 1;

            CheckFloor();

            VerticalMove();
            HorizontalMove();

        }

        private void EnterTheDoor()
        {
            Ray checkRay = new Ray((Vector3)character.Position, new Vector3(1, 0, 0));
            DrawableObject door = walls.Find(w => {
                Vector3 point;
                if (w.ColliderMesh != null && w.ColliderMesh.Collider.Intersects(ref checkRay, out point) && w is Door)
                {
                    return Math.Abs(character.Position.X - point.X) <= 20;
                }
                return false;
            });

            if (door != null)
            {
                Console.WriteLine("NOT_NULL");
                isAnimation = true;
                Door other = ((Door)door).OtherDoor;
                animation["target_pos"] = other.Position;
                animation["target_angle"] = other.Yaw;
                animation["angle"] = 0.01f;
                animation["radius"] = other.Radius;
                character.IsAlive = false;
                animation.AnimationEnded += (s, a) =>
                {
                    isAnimation = false;
                    Vector4 pos = door.Position;
                    pos.X = character.InitialPosition.X;
                    pos.Y += 11;
                    character.Position = pos;
                    character.IsAlive = true;
                };
            }
            else
                Console.WriteLine("NO");
        }

        private void CheckFloor()
        {
            Ray checkRay = new Ray((Vector3)character.Position, new Vector3(0, -1, 0));
            Vector3 interPoint;
            BoundingBox newStep = character.ColliderMesh.GetNewCollider(character.GetNewVerticalPosition(-0.1f));
            if ((!lastFloor.ColliderMesh.Collider.Intersects(ref checkRay, out interPoint) || checkRay.Position.Y - interPoint.Y > 1)
                && lastFloor.ColliderMesh.Collider.Contains(newStep) == ContainmentType.Disjoint)
            {
                character.IsFlying = true;
            }
            else if (character.ColliderMesh.Collider.Contains(lastFloor.ColliderMesh.Collider) != ContainmentType.Disjoint)
            {
                while (true)
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
            if (character.IsAlive) character.Render(view, projection);
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
                            flyingHeight.Y = character.Position.Y;
                            Console.WriteLine($"start: {flyingHeight.X} stop: {flyingHeight.Y}");
                            if (flyingHeight.X - flyingHeight.Y >= 30)
                            {
                                //character.IsAlive = false;
                                character.SetInitialStates();
                                SetScene();
                            }
                            flyingHeight.X = character.Position.Y;
                            return;
                        }
                        character.Position = newPos;
                    }
                }
            }
            //Console.WriteLine(character.Speed.Y);
            character.Position = newPos;
            character.Speed.Y -= GRAVITY;
            if (character.Speed.Y + GRAVITY >= 0 && character.Speed.Y < 0)
            {
                //Console.WriteLine("Less");
                flyingHeight.X = character.Position.Y;
            }
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

        private void HorizontalMove()
        {
            if (character.Horizontal == 0) return;
            List<DrawableObject> platforms = walls.FindAll(w => w is RotatiableObject && w.ColliderMesh != null);
            foreach(var platform in platforms)
            {
                float yaw;
                Vector4 newPos = ((RotatiableObject)platform).GetNewHorizontalPosition(character.Speed.X * character.Horizontal, out yaw);
                BoundingBox platformBox = platform.ColliderMesh.GetNewCollider(newPos);
                if (character.ColliderMesh.Collider.Contains(platformBox) != ContainmentType.Disjoint)
                {
                    while (true)
                    {
                        newPos = ((RotatiableObject)platform).GetNewHorizontalPosition(0.01f * character.Horizontal, out yaw);
                        platformBox = platform.ColliderMesh.GetNewCollider(newPos);
                        if (character.ColliderMesh.Collider.Contains(platformBox) != ContainmentType.Disjoint)
                            return;
                        MoveScene(0.01f * character.Horizontal, 65);
                    }
                }
            }
            MoveScene(character.Speed.X * character.Horizontal, 65);
        }


        private void SetScene(float angle)
        {
            DrawableObject obj = walls[1];
            obj.Yaw = angle;

            walls.Skip(2).ToList().ForEach(w =>
            {
                w.Position = ((RotatiableObject)w).GetNewHorizontalPosition(angle);
                //Console.WriteLine($"YAW: {yaw}");
                w.Yaw = angle;
            });
        }

        private void SetScene()
        {
            walls.ForEach(w => w.SetInitialStates());
        }

        private void MoveScene(float delta, float radius)
        {
            walls.ForEach(w =>
            {
                float yaw;
                w.Position = ((RotatiableObject)w).GetNewHorizontalPosition(delta, out yaw);
                //Console.WriteLine($"YAW: {yaw}");
                w.Yaw = yaw;
            });
        }
    }
}
