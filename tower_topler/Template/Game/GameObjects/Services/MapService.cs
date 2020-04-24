﻿using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.newObjects;
using Template.Game.GameObjects.Objects;

namespace Template.Game.GameObjects.Services
{
    class MapService : IService
    {
        private float radius;
        private float delta;
        private float angle = PositionalObject.PI;
        private InputController controller;
        public List<DrawableObject> Walls { get; private set; }

        public MapService(Loader loader, InputController controller)
        {
            radius = 65;
            delta = 0.01f;
            this.controller = controller;
            Walls = new List<DrawableObject>();
            DrawableObject floor = new DrawableObject(Vector4.Zero);
            floor.AddMeshObjects(loader.LoadMeshesFromObject("resources\\objects\\floor\\floor.obj", null));
            floor.SetCollider();
            Walls.Add(floor);

            DrawableObject tower = new DrawableObject(Vector4.Zero);
            tower.AddMeshObjects(loader.LoadMeshesFromObject("resources\\objects\\tower\\tower.obj", null));
            Walls.Add(tower);

            CreatePlatform(new Vector4(0, 2, 0, 0), PositionalObject.PI, loader.LoadMeshesFromObject("resources\\objects\\brick\\brick.obj", null));
            CreatePlatform(new Vector4(0, 2, 0, 0), PositionalObject.HALF_PI / 2, loader.LoadMeshesFromObject("resources\\objects\\brick\\brick.obj", null));
            //CreatePlatform(new Vector4(0, 2, 0, 0), PositionalObject.PI * 8 / 9, loader.LoadMeshesFromObject("resources\\objects\\brick\\brick.obj", null));
            CreateLiftPlatform(new Vector4(0, 2, 0, 0), PositionalObject.PI * 8 / 9, loader.LoadMeshesFromObject("resources\\objects\\brick\\brick.obj", null));
        }

        private void CreatePlatform(Vector4 initialPos, float angle, List<MeshObject> meshes)
        {
            initialPos.X = (float)(Math.Cos(-angle) * radius);
            initialPos.Z = (float)(Math.Sin(-angle) * radius);
            DrawableObject platform = new Platform(initialPos);
            platform.AddMeshObjects(meshes);
            platform.Yaw = angle;
            platform.SetCollider("brick");
            Walls.Add(platform);
        }

        private void CreateLiftPlatform(Vector4 initialPos, float angle, List<MeshObject> meshes)
        {
            initialPos.X = (float)(Math.Cos(-angle) * radius);
            initialPos.Z = (float)(Math.Sin(-angle) * radius);
            DrawableObject platform = new LiftPlatform(initialPos, new Vector2(initialPos.Y, initialPos.Y + 10));
            platform.AddMeshObjects(meshes);
            platform.Yaw = angle;
            platform.SetCollider("brick");
            Walls.Add(platform);
        }

        public void Render(Matrix view, Matrix projection)
        {
            Walls.ForEach(w => w.Render(view, projection));
        }

        public void Update()
        {
            Walls.FindAll(w => w is LiftPlatform).ForEach(w => MoveLift((LiftPlatform)w));
        }

        private void MoveLift(LiftPlatform platform)
        {
            if (platform.Up == 0) return;

            if (platform.Up == 1)
            {
                platform.Position = platform.GetNewVerticalPosition(0.5f);
                if (platform.Position.Y >= platform.MinMax.Y)
                    platform.Up = 0;
            }
            else if (platform.Up == -1)
            {
                platform.Position = platform.GetNewVerticalPosition(-0.5f);
                if (platform.Position.Y <= platform.MinMax.X)
                    platform.Up = 0;
            }
        }
    }
}
